using System.Net.Mail;
using System.Windows.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Curso oferecido na tela de cadastro, com o estado de seleção.</summary>
    public class CursoCadastroItem : BaseViewModel
    {
        private bool _selecionado;

        public Curso Curso { get; init; } = new();
        public string Nome => Curso.Nome;
        public string Descricao => Curso.Descricao;
        public bool TemDescricao => !string.IsNullOrWhiteSpace(Curso.Descricao);
        public string PrecoTexto => Curso.Preco <= 0 ? "Gratuito" : Curso.Preco.ToString("C", PagamentoViewModel.PtBr);
        public string CargaTexto => Curso.CargaHorariaHoras > 0 ? $"{Curso.CargaHorariaHoras} horas" : string.Empty;

        public bool Selecionado
        {
            get => _selecionado;
            set => SetProperty(ref _selecionado, value);
        }
    }

    public class UserViewModel : BaseViewModel
    {
        /// <summary>Cursos por página na etapa de escolha do curso.</summary>
        private const int CursosPorPagina = 4;

        private List<CursoCadastroItem> _cursos = new();
        private CursoCadastroItem? _cursoSelecionado;
        private IReadOnlyList<CursoCadastroItem> _cursosPagina = new List<CursoCadastroItem>();
        private int _indicePagina;
        private int _etapa = 1;
        private string _nome = string.Empty;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private string _confirmacaoSenha = string.Empty;
        private string _mensagem = string.Empty;
        private bool _precisaConfigurar;

        public UserViewModel()
        {
            LoginCommand = new Command(async () => await LoginAsync());
            CadastroCommand = new Command(async () => await EnviarSolicitacaoAsync());
            ConfigurarCoordenacaoCommand = new Command(async () => await ConfigurarCoordenacaoAsync());
            IrParaCadastroCommand = new Command(async () => await Shell.Current.GoToAsync("cadastro"));
            IrParaPrimeiroAcessoCommand = new Command(async () => await Shell.Current.GoToAsync("primeiroacesso"));
            IrParaLoginCommand = new Command(async () => await Shell.Current.GoToAsync("//login"));
            AvancarEtapaCommand = new Command(async () => await AvancarEtapaAsync());
            VoltarEtapaCommand = new Command(() => Etapa = 1);
            SelecionarCursoCommand = new Command<CursoCadastroItem>(SelecionarCurso);
            VerDetalhesCommand = new Command<CursoCadastroItem>(async item => await VerDetalhesAsync(item));
            PaginaAnteriorCommand = new Command(() => MudarPagina(-1));
            PaginaProximaCommand = new Command(() => MudarPagina(1));
            Title = "Acesso";
        }

        public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Senha { get => _senha; set => SetProperty(ref _senha, value); }
        public string ConfirmacaoSenha { get => _confirmacaoSenha; set => SetProperty(ref _confirmacaoSenha, value); }

        public string Mensagem
        {
            get => _mensagem;
            set
            {
                if (SetProperty(ref _mensagem, value))
                    OnPropertyChanged(nameof(TemMensagem));
            }
        }

        public bool TemMensagem => !string.IsNullOrWhiteSpace(Mensagem);

        /// <summary>Verdadeiro enquanto não existe nenhuma conta de coordenação no banco.</summary>
        public bool PrecisaConfigurar
        {
            get => _precisaConfigurar;
            set => SetProperty(ref _precisaConfigurar, value);
        }

        // ---- Etapas do cadastro: 1 = seus dados, 2 = escolha do curso ----

        public int Etapa
        {
            get => _etapa;
            private set
            {
                if (SetProperty(ref _etapa, value))
                {
                    Mensagem = string.Empty;
                    OnPropertyChanged(nameof(EtapaDados));
                    OnPropertyChanged(nameof(EtapaCurso));
                    OnPropertyChanged(nameof(EtapaTexto));
                    OnPropertyChanged(nameof(EtapaTitulo));
                }
            }
        }

        public bool EtapaDados => Etapa == 1;
        public bool EtapaCurso => Etapa == 2;
        public string EtapaTexto => $"Etapa {Etapa} de 2";
        public string EtapaTitulo => Etapa == 1 ? "Seus dados" : "Escolha o curso";

        // ---- Cursos (etapa 2), paginados ----

        public List<CursoCadastroItem> Cursos
        {
            get => _cursos;
            private set
            {
                if (SetProperty(ref _cursos, value))
                    OnPropertyChanged(nameof(SemCursos));
            }
        }

        public bool SemCursos => Cursos.Count == 0;

        public IReadOnlyList<CursoCadastroItem> CursosPagina
        {
            get => _cursosPagina;
            private set => SetProperty(ref _cursosPagina, value);
        }

        public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(Cursos.Count / (double)CursosPorPagina));
        public bool TemPaginacao => TotalPaginas > 1;
        public bool PodePaginaAnterior => _indicePagina > 0;
        public bool PodePaginaProxima => _indicePagina < TotalPaginas - 1;
        public string IndicadorPagina => $"{_indicePagina + 1} / {TotalPaginas}";

        public ICommand LoginCommand { get; }
        public ICommand CadastroCommand { get; }
        public ICommand ConfigurarCoordenacaoCommand { get; }
        public ICommand IrParaCadastroCommand { get; }
        public ICommand IrParaPrimeiroAcessoCommand { get; }
        public ICommand IrParaLoginCommand { get; }
        public ICommand AvancarEtapaCommand { get; }
        public ICommand VoltarEtapaCommand { get; }
        public ICommand SelecionarCursoCommand { get; }
        public ICommand VerDetalhesCommand { get; }
        public ICommand PaginaAnteriorCommand { get; }
        public ICommand PaginaProximaCommand { get; }

        public async Task VerificarConfiguracaoAsync()
        {
            try
            {
                await MongoContext.InicializarAsync();
                PrecisaConfigurar = !await UsuarioRepositorio.ExisteCoordenacaoAsync();
                if (Mensagem.StartsWith("Sem conexão"))
                    Mensagem = string.Empty;
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(VerificarConfiguracaoAsync), ex);
                Mensagem = "Sem conexão com o banco de dados. Verifique se o MongoDB está em execução.";
            }
        }

        public async Task CarregarCursosAsync()
        {
            try
            {
                await MongoContext.InicializarAsync();
                var cursos = (await CursoRepositorio.ListarAsync()).Where(c => c.IsPublicado).ToList();
                Cursos = cursos.Select(c => new CursoCadastroItem { Curso = c }).ToList();
                _indicePagina = 0;
                AplicarPagina();

                // com um único curso (o mestrado), ele já vem marcado
                if (Cursos.Count == 1)
                    SelecionarCurso(Cursos[0]);
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(CarregarCursosAsync), ex);
                Mensagem = "Sem conexão com o banco de dados. Verifique se o MongoDB está em execução.";
            }
        }

        /// <summary>Chamado ao voltar da página de detalhes com "Escolher este curso".</summary>
        public void SelecionarCursoPorId(string cursoId)
        {
            var item = Cursos.FirstOrDefault(c => c.Curso.Id == cursoId);
            if (item == null)
                return;

            SelecionarCurso(item);
            _indicePagina = Cursos.IndexOf(item) / CursosPorPagina;
            AplicarPagina();
        }

        private void SelecionarCurso(CursoCadastroItem? item)
        {
            if (item == null)
                return;

            foreach (var curso in Cursos)
                curso.Selecionado = ReferenceEquals(curso, item);
            _cursoSelecionado = item;
            Mensagem = string.Empty;
        }

        private static async Task VerDetalhesAsync(CursoCadastroItem? item)
        {
            if (item != null)
                await Shell.Current.GoToAsync("cursoinfo", new Dictionary<string, object> { ["Curso"] = item.Curso });
        }

        private void MudarPagina(int delta)
        {
            var novo = _indicePagina + delta;
            if (novo < 0 || novo >= TotalPaginas)
                return;

            _indicePagina = novo;
            AplicarPagina();
        }

        private void AplicarPagina()
        {
            CursosPagina = Cursos.Skip(_indicePagina * CursosPorPagina).Take(CursosPorPagina).ToList();
            OnPropertyChanged(nameof(TotalPaginas));
            OnPropertyChanged(nameof(TemPaginacao));
            OnPropertyChanged(nameof(PodePaginaAnterior));
            OnPropertyChanged(nameof(PodePaginaProxima));
            OnPropertyChanged(nameof(IndicadorPagina));
        }

        // ---- Login ----

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                Mensagem = "Informe e-mail e senha.";
                return;
            }

            await ExecutarAsync(async () =>
            {
                await MongoContext.InicializarAsync();
                var usuario = await UsuarioRepositorio.PorEmailAsync(Email);

                if (usuario != null && SenhaService.Verificar(Senha, usuario.SenhaHash))
                {
                    Mensagem = string.Empty;
                    Senha = string.Empty;
                    await AcessoService.EntrarAsync(usuario);
                    return;
                }

                // ainda não é usuário: pode ser um candidato com solicitação em andamento
                var solicitacao = usuario == null ? await SolicitacaoRepositorio.PorEmailAsync(Email) : null;
                if (solicitacao != null && SenhaService.Verificar(Senha, solicitacao.SenhaHash))
                {
                    Mensagem = string.Empty;
                    Senha = string.Empty;
                    await AcessoService.EntrarCandidatoAsync(solicitacao);
                    return;
                }

                Mensagem = "E-mail ou senha inválidos.";
            });
        }

        // ---- Cadastro do aluno (vira solicitação; a conta só é criada quando a coordenação libera) ----

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

        private async Task EnviarSolicitacaoAsync()
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

                Mensagem = string.Empty;
                Senha = string.Empty;
                await AcessoService.EntrarCandidatoAsync(solicitacao);
            });
        }

        private bool DadosValidos()
        {
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                Mensagem = "Informe nome, e-mail e senha.";
                return false;
            }

            if (!EmailValido(Email))
            {
                Mensagem = "Informe um e-mail válido.";
                return false;
            }

            if (!SenhaService.EhValida(Senha, out var erroSenha))
            {
                Mensagem = erroSenha;
                return false;
            }

            Mensagem = string.Empty;
            return true;
        }

        // ---- Primeira conta da coordenação ----

        private async Task ConfigurarCoordenacaoAsync()
        {
            if (!DadosValidos())
                return;

            if (Senha != ConfirmacaoSenha)
            {
                Mensagem = "As senhas não conferem.";
                return;
            }

            await ExecutarAsync(async () =>
            {
                await MongoContext.InicializarAsync();

                // A conta inicial da coordenação só pode ser criada enquanto não houver nenhuma.
                if (await UsuarioRepositorio.ExisteCoordenacaoAsync())
                {
                    Mensagem = "A coordenação já foi configurada. Faça login.";
                    return;
                }

                var usuario = new Usuario
                {
                    Nome = Nome,
                    Email = Email,
                    SenhaHash = SenhaService.GerarHash(Senha),
                    Roles = Roles.Coordenador,
                    StatusAcesso = StatusAcesso.Liberado
                };

                if (!await UsuarioRepositorio.InserirAsync(usuario))
                {
                    Mensagem = "Este e-mail já está cadastrado.";
                    return;
                }

                Mensagem = string.Empty;
                Senha = string.Empty;
                ConfirmacaoSenha = string.Empty;
                await AcessoService.EntrarAsync(usuario);
            });
        }

        public static bool EmailValido(string email)
        {
            try
            {
                return new MailAddress(email.Trim()).Address == email.Trim();
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
