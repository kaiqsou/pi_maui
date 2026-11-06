using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Curso oferecido na tela de cadastro, com o estado de seleção.</summary>
    public partial class CursoCadastroItem : ObservableObject
    {
        public Curso Curso { get; init; } = new();
        public string Nome => Curso.Nome;
        public string Descricao => Curso.Descricao;
        public bool TemDescricao => !string.IsNullOrWhiteSpace(Curso.Descricao);
        public string PrecoTexto => Formatos.Preco(Curso.Preco);
        public string CargaTexto => Curso.CargaHorariaHoras > 0 ? Formatos.Horas(Curso.CargaHorariaHoras) : string.Empty;

        [ObservableProperty]
        public partial bool Selecionado { get; set; }
    }

    /// <summary>
    /// Cadastro do aluno em duas etapas (seus dados → escolha do curso). Gera uma solicitação de matrícula:
    /// a conta só é criada quando a coordenação libera.
    /// </summary>
    public partial class CadastroViewModel : BaseViewModel
    {
        /// <summary>Cursos por página na etapa de escolha do curso.</summary>
        private const int CursosPorPagina = 4;

        private bool _cursosCarregados;
        private CursoCadastroItem? _cursoSelecionado;

        public CadastroViewModel()
        {
            Title = "Acesso";
            Paginas.Mudou += () => CursosPagina = Cursos.Skip(Paginas.Indice * CursosPorPagina).Take(CursosPorPagina).ToList();
        }

        [ObservableProperty]
        public partial string Nome { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Email { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Senha { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemMensagem))]
        public partial string Mensagem { get; private set; } = string.Empty;

        public bool TemMensagem => !string.IsNullOrWhiteSpace(Mensagem);

        // ---- Etapas: 1 = seus dados, 2 = escolha do curso ----

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EtapaDados), nameof(EtapaCurso), nameof(EtapaTexto), nameof(EtapaTitulo))]
        public partial int Etapa { get; private set; } = 1;

        public bool EtapaDados => Etapa == 1;
        public bool EtapaCurso => Etapa == 2;
        public string EtapaTexto => $"Etapa {Etapa} de 2";
        public string EtapaTitulo => Etapa == 1 ? "Seus dados" : "Escolha o curso";

        partial void OnEtapaChanged(int value) => Mensagem = string.Empty;

        // ---- Cursos (etapa 2), paginados ----

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SemCursos))]
        public partial List<CursoCadastroItem> Cursos { get; private set; } = new();

        public bool SemCursos => Cursos.Count == 0;

        public Paginador Paginas { get; } = new();

        [ObservableProperty]
        public partial IReadOnlyList<CursoCadastroItem> CursosPagina { get; private set; } = new List<CursoCadastroItem>();

        /// <summary>Volta da página de detalhes com "Escolher este curso".</summary>
        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            if (parametros.Obter<string>("CursoEscolhidoId") is { } cursoId && Cursos.FirstOrDefault(c => c.Curso.Id == cursoId) is { } item)
            {
                SelecionarCurso(item);
                Paginas.Reiniciar(TotalPaginas(), Cursos.IndexOf(item) / CursosPorPagina);
            }

            parametros.Clear();
            return Task.CompletedTask;
        }

        public override async Task AoAparecerAsync()
        {
            if (_cursosCarregados)
                return;

            try
            {
                await MongoContext.InicializarAsync();
                var cursos = (await CursoRepositorio.ListarAsync()).Where(c => c.IsPublicado).ToList();
                Cursos = cursos.Select(c => new CursoCadastroItem { Curso = c }).ToList();
                Paginas.Reiniciar(TotalPaginas());
                _cursosCarregados = true;

                // com um único curso (o mestrado), ele já vem marcado
                if (Cursos.Count == 1)
                    SelecionarCurso(Cursos[0]);
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(CadastroViewModel), ex);
                Mensagem = LoginViewModel.SemConexao;
            }
        }

        private int TotalPaginas() => (int)Math.Ceiling(Cursos.Count / (double)CursosPorPagina);

        [RelayCommand]
        private void SelecionarCurso(CursoCadastroItem? item)
        {
            if (item == null)
                return;

            foreach (var curso in Cursos)
                curso.Selecionado = ReferenceEquals(curso, item);
            _cursoSelecionado = item;
            Mensagem = string.Empty;
        }

        [RelayCommand]
        private Task VerDetalhes(CursoCadastroItem? item)
            => item == null ? Task.CompletedTask : Navegacao.IrAsync(Navegacao.CursoInfo, ("Curso", item.Curso));

        [RelayCommand]
        private Task IrParaLogin() => Navegacao.IrAsync(Navegacao.Login);

        [RelayCommand]
        private void VoltarEtapa() => Etapa = 1;

        [RelayCommand]
        private async Task AvancarEtapaAsync()
        {
            if (!DadosValidos())
                return;

            var disponivel = false;
            if (!await ExecutarAsync(async () =>
                {
                    await MongoContext.InicializarAsync();
                    disponivel = await UsuarioRepositorio.PorEmailAsync(Email) == null
                                 && await SolicitacaoRepositorio.PorEmailAsync(Email) == null;
                }))
                return;

            if (!disponivel)
            {
                Mensagem = "Este e-mail já está cadastrado ou tem uma solicitação em andamento. Faça login.";
                return;
            }

            Etapa = 2;
        }

        /// <summary>Envia a solicitação de matrícula e entra como candidato (segue para o pagamento).</summary>
        [RelayCommand]
        private async Task CadastroAsync()
        {
            if (!DadosValidos())
            {
                Etapa = 1;
                return;
            }

            if (_cursoSelecionado == null)
            {
                Mensagem = "Escolha o curso que deseja fazer.";
                return;
            }

            await ExecutarAsync(async () =>
            {
                var solicitacao = new SolicitacaoMatricula
                {
                    Nome = Nome,
                    Email = Email,
                    SenhaHash = SenhaService.GerarHash(Senha),
                    CursoId = _cursoSelecionado.Curso.Id
                };

                if (!await SolicitacaoRepositorio.InserirAsync(solicitacao))
                {
                    Mensagem = "Este e-mail já está cadastrado ou tem uma solicitação em andamento. Faça login.";
                    return;
                }

                Mensagem = Senha = string.Empty;
                await AcessoService.EntrarCandidatoAsync(solicitacao);
            });
        }

        private bool DadosValidos()
        {
            Mensagem = ContaService.ValidarDados(Nome, Email, Senha) ?? string.Empty;
            return !TemMensagem;
        }
    }
}
