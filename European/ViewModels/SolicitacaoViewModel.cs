using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>
    /// Coordenação: analisa uma solicitação de matrícula. "Liberar" cria a conta do aluno e a matrícula
    /// (só então o e-mail passa a existir como usuário); "Não liberar" guarda a pendência para o candidato ver.
    /// </summary>
    public partial class SolicitacaoViewModel : BaseViewModel
    {
        private Curso? _curso;

        public SolicitacaoViewModel()
        {
            Title = "Solicitação de matrícula";
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Nome), nameof(Email), nameof(Iniciais), nameof(CriadaEmTexto), nameof(Pago),
            nameof(PagamentoTexto), nameof(TemPendencia), nameof(PendenciaTexto), nameof(StatusTexto), nameof(PodeLiberar), nameof(CursoNome))]
        public partial SolicitacaoMatricula? Solicitacao { get; private set; }

        public string Nome => Solicitacao?.Nome ?? string.Empty;
        public string Email => Solicitacao?.Email ?? string.Empty;
        public string Iniciais => Solicitacao?.Iniciais ?? "?";
        public string CriadaEmTexto => Solicitacao == null ? string.Empty : $"Solicitada em {Solicitacao.CriadaEm:dd/MM/yyyy HH:mm}";
        public string CursoNome => _curso?.Nome ?? "Curso não encontrado";

        public bool Pago => Solicitacao?.Pago == true;
        public string PagamentoTexto => Pago
            ? $"Pago {Formatos.Pagamento(Solicitacao!.FormaPagamento, Solicitacao.PagamentoEm)}"
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

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
            => parametros.Obter<SolicitacaoMatricula>("Solicitacao") is { } solicitacao
                ? CarregarAsync(solicitacao.Id)
                : Task.CompletedTask;

        private Task CarregarAsync(string solicitacaoId) => ExecutarAsync(async () =>
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

        [RelayCommand]
        private async Task LiberarAsync()
        {
            var solicitacao = Solicitacao;
            var curso = _curso;
            if (solicitacao == null || curso == null)
                return;

            if (!await ConfirmarAsync($"Liberar a matrícula de {solicitacao.Nome} em {curso.Nome}? A conta do aluno será criada agora.", "Liberar"))
                return;

            var criada = false;
            if (!await ExecutarAsync(async () => criada = await ContaService.LiberarSolicitacaoAsync(solicitacao)))
                return;

            if (!criada)
            {
                await AvisarAsync("Liberar matrícula", "Já existe uma conta com este e-mail. Abra o aluno pela lista para liberar o curso.");
                return;
            }

            await AvisarAsync("Matrícula liberada", $"{solicitacao.Nome} já pode entrar e acessar {curso.Nome}.");
            await VoltarAsync();
        }

        [RelayCommand]
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
