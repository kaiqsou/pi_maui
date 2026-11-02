using System.Collections.ObjectModel;
using System.Windows.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class MatriculaItem
    {
        public Matricula Matricula { get; init; } = new();
        public string CursoNome { get; init; } = string.Empty;
        public string Status => Matricula.Status switch
        {
            StatusMatricula.Ativa => "Ativa",
            StatusMatricula.Concluida => "Concluída",
            StatusMatricula.Cancelada => "Encerrada",
            StatusMatricula.Trancada => "Trancada",
            _ => "Pendente"
        };
        public string Detalhe => $"{Matricula.ProgressoPercentual:0}% concluído · desde {Matricula.DataMatricula:dd/MM/yyyy}";
        public bool Ativa => Matricula.Status == StatusMatricula.Ativa;
        public bool Encerrada => Matricula.Status == StatusMatricula.Cancelada;
    }

    /// <summary>Coordenação: liberar/remover o acesso de um aluno e gerenciar as matrículas dele.</summary>
    public class AlunoAcessoViewModel : BaseViewModel
    {
        private Usuario? _aluno;
        private List<CursoOpcao> _cursosDisponiveis = new();
        private CursoOpcao? _cursoSelecionado;
        private string _statusTexto = string.Empty;
        private string _statusDescricao = string.Empty;

        public AlunoAcessoViewModel()
        {
            LiberarCommand = new Command(async () => await LiberarAsync());
            EncerrarCommand = new Command<MatriculaItem>(async m => await AlterarMatriculaAsync(m, StatusMatricula.Cancelada));
            ReativarCommand = new Command<MatriculaItem>(async m => await AlterarMatriculaAsync(m, StatusMatricula.Ativa));
            RemoverAcessoCommand = new Command(async () => await RemoverAcessoAsync());
            RedefinirSenhaCommand = new Command(async () => await RedefinirSenhaAsync());
        }

        public Usuario? Aluno
        {
            get => _aluno;
            private set
            {
                if (SetProperty(ref _aluno, value))
                {
                    OnPropertyChanged(nameof(Nome));
                    OnPropertyChanged(nameof(Email));
                    OnPropertyChanged(nameof(Iniciais));
                    OnPropertyChanged(nameof(PodeRemover));
                    OnPropertyChanged(nameof(Pago));
                    OnPropertyChanged(nameof(PagamentoTexto));
                }
            }
        }

        public string Nome => Aluno?.Nome ?? string.Empty;
        public string Email => Aluno?.Email ?? string.Empty;
        public string Iniciais => Aluno?.Iniciais ?? "?";
        public bool PodeRemover => Aluno?.StatusAcesso == StatusAcesso.Liberado;

        /// <summary>Pagamento feito na solicitação de matrícula (contas criadas pela coordenação não têm).</summary>
        public bool Pago => Aluno?.PagamentoEm != null;
        public string PagamentoTexto => Aluno is { PagamentoEm: { } quando, FormaPagamento: { } forma }
            ? $"Pago via {PagamentoService.Nome(forma)} em {quando:dd/MM/yyyy HH:mm}"
            : string.Empty;

        public string StatusTexto { get => _statusTexto; private set => SetProperty(ref _statusTexto, value); }
        public string StatusDescricao { get => _statusDescricao; private set => SetProperty(ref _statusDescricao, value); }

        public ObservableCollection<MatriculaItem> Matriculas { get; } = new();
        public List<CursoOpcao> CursosDisponiveis { get => _cursosDisponiveis; private set => SetProperty(ref _cursosDisponiveis, value); }
        public CursoOpcao? CursoSelecionado { get => _cursoSelecionado; set => SetProperty(ref _cursoSelecionado, value); }
        public bool TemCursosDisponiveis => CursosDisponiveis.Count > 0;

        public ICommand LiberarCommand { get; }
        public ICommand EncerrarCommand { get; }
        public ICommand ReativarCommand { get; }
        public ICommand RemoverAcessoCommand { get; }
        public ICommand RedefinirSenhaCommand { get; }

        public Task CarregarAsync(string alunoId) => ExecutarAsync(async () =>
        {
            Aluno = await UsuarioRepositorio.PorIdAsync(alunoId);
            if (Aluno == null)
                return;

            Title = Aluno.Nome;
            var matriculas = await MatriculaRepositorio.DoAlunoAsync(Aluno.Id);
            var cursos = await CursoRepositorio.ListarAsync();

            Matriculas.Clear();
            foreach (var matricula in matriculas)
                Matriculas.Add(new MatriculaItem { Matricula = matricula, CursoNome = matricula.Curso?.Nome ?? string.Empty });

            var ocupados = matriculas.Where(m => m.Status is StatusMatricula.Ativa or StatusMatricula.Concluida).Select(m => m.CursoId).ToHashSet();
            CursosDisponiveis = cursos.Where(c => c.IsPublicado && !ocupados.Contains(c.Id))
                .Select(c => new CursoOpcao { Valor = c, Texto = c.Nome })
                .ToList();
            CursoSelecionado = CursosDisponiveis.FirstOrDefault();
            OnPropertyChanged(nameof(TemCursosDisponiveis));
            OnPropertyChanged(nameof(PodeRemover));

            var ativas = matriculas.Count(m => m.Status == StatusMatricula.Ativa);
            (StatusTexto, StatusDescricao) = Aluno.StatusAcesso switch
            {
                StatusAcesso.Pendente =>("Aguardando liberação", "O aluno criou a conta e ainda não tem acesso. Escolha o curso abaixo e libere."),
                StatusAcesso.Removido => (matriculas.Count > 0 ? "Ex-aluno" : "Sem acesso", "O acesso foi removido. Se já foi aluno, ele vê apenas o próprio certificado."),
                _ when ativas > 0 => ("Acesso liberado", $"Acessa aulas e provas de {ativas} curso(s)."),
                _ => ("Sem curso ativo", "Conta liberada, mas sem matrícula ativa: o aluno vê apenas o certificado.")
            };
        });

        private async Task LiberarAsync()
        {
            var curso = CursoSelecionado?.Valor;
            var coordenador = SessionService.UsuarioAtual;
            if (Aluno == null || curso == null)
                return;

            var ok = await ExecutarAsync(async () =>
            {
                var existente = await MatriculaRepositorio.PorAlunoECursoAsync(Aluno.Id, curso.Id);
                if (existente != null)
                {
                    existente.Status = StatusMatricula.Ativa;
                    existente.LiberadaPorId = coordenador?.Id;
                    await MatriculaRepositorio.AtualizarAsync(existente);
                }
                else
                {
                    await MatriculaRepositorio.InserirAsync(new Matricula
                    {
                        AlunoId = Aluno.Id,
                        CursoId = curso.Id,
                        Status = StatusMatricula.Ativa,
                        LiberadaPorId = coordenador?.Id,
                        DataMatricula = DateTime.Now
                    });
                }

                Aluno.StatusAcesso = StatusAcesso.Liberado;
                await UsuarioRepositorio.AtualizarAsync(Aluno);
            });

            if (ok)
            {
                await AvisarAsync("Acesso liberado", $"{Aluno.Nome} já pode acessar {curso.Nome}.");
                await CarregarAsync(Aluno.Id);
            }
        }

        private async Task AlterarMatriculaAsync(MatriculaItem? item, StatusMatricula status)
        {
            if (item == null || Aluno == null)
                return;

            if (status == StatusMatricula.Cancelada &&
                !await ConfirmarAsync($"Encerrar o acesso de {Aluno.Nome} a {item.CursoNome}? O progresso fica guardado.", "Encerrar"))
                return;

            await ExecutarAsync(async () =>
            {
                item.Matricula.Status = status;
                await MatriculaRepositorio.AtualizarAsync(item.Matricula);

                if (status == StatusMatricula.Ativa && Aluno.StatusAcesso != StatusAcesso.Liberado)
                {
                    Aluno.StatusAcesso = StatusAcesso.Liberado;
                    await UsuarioRepositorio.AtualizarAsync(Aluno);
                }
            });
            await CarregarAsync(Aluno.Id);
        }

        private async Task RemoverAcessoAsync()
        {
            if (Aluno == null || !await ConfirmarAsync($"Remover o acesso de {Aluno.Nome} ao aplicativo? Todas as matrículas ativas serão encerradas.", "Remover acesso"))
                return;

            await ExecutarAsync(async () =>
            {
                foreach (var item in Matriculas.Where(m => m.Ativa))
                {
                    item.Matricula.Status = StatusMatricula.Cancelada;
                    await MatriculaRepositorio.AtualizarAsync(item.Matricula);
                }

                Aluno.StatusAcesso = StatusAcesso.Removido;
                await UsuarioRepositorio.AtualizarAsync(Aluno);
            });
            await CarregarAsync(Aluno.Id);
        }

        private async Task RedefinirSenhaAsync()
        {
            if (Aluno == null)
                return;

            var senha = await Shell.Current.DisplayPromptAsync("Nova senha", $"Defina uma senha provisória para {Aluno.Nome}.", "Salvar", "Cancelar");
            if (string.IsNullOrEmpty(senha))
                return;

            if (!SenhaService.EhValida(senha, out var erro))
            {
                await AvisarAsync("Senha", erro);
                return;
            }

            var hash = SenhaService.GerarHash(senha);
            if (await ExecutarAsync(() => UsuarioRepositorio.AtualizarSenhaAsync(Aluno.Id, hash, provisoria: true)))
                await AvisarAsync("Senha", "Senha alterada. Informe a nova senha ao aluno.");
        }
    }
}
