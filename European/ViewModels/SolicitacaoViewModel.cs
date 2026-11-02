using System.Windows.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>
    /// Coordenação: analisa uma solicitação de matrícula. "Liberar" cria a conta do aluno e a matrícula
    /// (só então o e-mail passa a existir como usuário); "Não liberar" guarda a pendência para o candidato ver.
    /// </summary>
    public class SolicitacaoViewModel : BaseViewModel
    {
        private SolicitacaoMatricula? _solicitacao;
        private Curso? _curso;

        public SolicitacaoViewModel()
        {
            Title = "Solicitação de matrícula";
            LiberarCommand = new Command(async () => await LiberarAsync());
            NaoLiberarCommand = new Command(async () => await NaoLiberarAsync());
        }

        public SolicitacaoMatricula? Solicitacao
        {
            get => _solicitacao;
            private set
            {
                if (SetProperty(ref _solicitacao, value))
                {
                    OnPropertyChanged(nameof(Nome));
                    OnPropertyChanged(nameof(Email));
                    OnPropertyChanged(nameof(Iniciais));
                    OnPropertyChanged(nameof(CriadaEmTexto));
                    OnPropertyChanged(nameof(Pago));
                    OnPropertyChanged(nameof(PagamentoTexto));
                    OnPropertyChanged(nameof(TemPendencia));
                    OnPropertyChanged(nameof(PendenciaTexto));
                    OnPropertyChanged(nameof(StatusTexto));
                    OnPropertyChanged(nameof(PodeLiberar));
                }
            }
        }

        public string Nome => Solicitacao?.Nome ?? string.Empty;
        public string Email => Solicitacao?.Email ?? string.Empty;
        public string Iniciais => Solicitacao?.Iniciais ?? "?";
        public string CriadaEmTexto => Solicitacao == null ? string.Empty : $"Solicitada em {Solicitacao.CriadaEm:dd/MM/yyyy HH:mm}";

        public string CursoNome => _curso?.Nome ?? "Curso não encontrado";

        public bool Pago => Solicitacao?.Pago == true;
        public string PagamentoTexto => Solicitacao is { PagamentoEm: { } quando, FormaPagamento: { } forma }
            ? $"Pago via {PagamentoService.Nome(forma)} em {quando:dd/MM/yyyy HH:mm}"
            : "Pagamento ainda não realizado";

        public bool TemPendencia => Solicitacao?.TemPendencia == true;
        public string PendenciaTexto => Solicitacao is { TemPendencia: true } s
            ? $"Não liberada em {s.PendenciaEm:dd/MM/yyyy}: {s.PendenciaMotivo}"
            : string.Empty;

        public string StatusTexto => Solicitacao switch
        {
            null => string.Empty,
            { TemPendencia: true } => "Com pendência",
            { Pago: false } => "Aguardando pagamento",
            _ => "Aguardando liberação"
        };

        /// <summary>Só libera com o pagamento feito e o curso ainda existente.</summary>
        public bool PodeLiberar => Pago && _curso != null;

        public ICommand LiberarCommand { get; }
        public ICommand NaoLiberarCommand { get; }

        public Task CarregarAsync(string solicitacaoId) => ExecutarAsync(async () =>
        {
            var solicitacao = await SolicitacaoRepositorio.PorIdAsync(solicitacaoId);
            _curso = solicitacao == null ? null : await CursoRepositorio.PorIdAsync(solicitacao.CursoId);
            Solicitacao = solicitacao;
            OnPropertyChanged(nameof(CursoNome));
            OnPropertyChanged(nameof(PodeLiberar));

            if (solicitacao == null)
            {
                await AvisarAsync("Solicitação", "Esta solicitação não existe mais.");
                await VoltarAsync();
            }
        });

        private async Task LiberarAsync()
        {
            var solicitacao = Solicitacao;
            var curso = _curso;
            if (solicitacao == null || curso == null)
                return;

            if (!await ConfirmarAsync($"Liberar a matrícula de {solicitacao.Nome} em {curso.Nome}? A conta do aluno será criada agora.", "Liberar"))
                return;

            var criada = false;
            var ok = await ExecutarAsync(async () =>
            {
                var aluno = new Usuario
                {
                    Nome = solicitacao.Nome,
                    Email = solicitacao.Email,
                    SenhaHash = solicitacao.SenhaHash,
                    Roles = Roles.Aluno,
                    StatusAcesso = StatusAcesso.Liberado,
                    FormaPagamento = solicitacao.FormaPagamento,
                    PagamentoEm = solicitacao.PagamentoEm
                };

                // só aqui o e-mail passa a existir como usuário
                if (!await UsuarioRepositorio.InserirAsync(aluno))
                    return;

                await MatriculaRepositorio.InserirAsync(new Matricula
                {
                    AlunoId = aluno.Id,
                    CursoId = curso.Id,
                    Status = StatusMatricula.Ativa,
                    LiberadaPorId = SessionService.UsuarioAtual?.Id,
                    DataMatricula = DateTime.Now
                });

                await SolicitacaoRepositorio.ExcluirAsync(solicitacao.Id);
                criada = true;
            });

            if (!ok)
                return;

            if (!criada)
            {
                await AvisarAsync("Liberar matrícula", "Já existe uma conta com este e-mail. Abra o aluno pela lista para liberar o curso.");
                return;
            }

            await AvisarAsync("Matrícula liberada", $"{solicitacao.Nome} já pode entrar e acessar {curso.Nome}.");
            await VoltarAsync();
        }

        private async Task NaoLiberarAsync()
        {
            var solicitacao = Solicitacao;
            if (solicitacao == null)
                return;

            var motivo = await Shell.Current.DisplayPromptAsync(
                "Não liberar",
                $"Qual pendência {solicitacao.Nome} precisa resolver? O candidato verá esta mensagem ao entrar no app.",
                "Registrar", "Cancelar",
                placeholder: "Ex.: pagamento não identificado",
                maxLength: 200,
                initialValue: solicitacao.PendenciaMotivo ?? string.Empty);

            if (motivo == null)
                return;

            motivo = motivo.Trim();
            if (motivo.Length == 0)
            {
                await AvisarAsync("Não liberar", "Informe o motivo para o candidato saber o que resolver.");
                return;
            }

            if (await ExecutarAsync(() => SolicitacaoRepositorio.RegistrarPendenciaAsync(solicitacao.Id, motivo)))
            {
                await AvisarAsync("Matrícula não liberada", $"{solicitacao.Nome} verá a pendência ao entrar no app. Nenhuma conta foi criada.");
                await CarregarAsync(solicitacao.Id);
            }
        }
    }
}
