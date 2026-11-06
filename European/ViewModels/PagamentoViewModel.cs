using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class ParcelaOpcao
    {
        public int Quantidade { get; init; }
        public string Texto { get; init; } = string.Empty;
    }

    /// <summary>
    /// Pagamento da matrícula — apenas simulado: nada é cobrado e os dados do cartão não são
    /// gravados; só a forma de pagamento e a data ficam na solicitação para a coordenação conferir.
    /// </summary>
    public partial class PagamentoViewModel : BaseViewModel
    {
        private const int MaxParcelas = 12;

        public PagamentoViewModel()
        {
            Title = "Pagamento";
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CursoNome), nameof(CargaTexto), nameof(PrecoTexto), nameof(BotaoTexto))]
        public partial Curso? Curso { get; private set; }

        public string CursoNome => Curso?.Nome ?? string.Empty;
        public string CargaTexto => Curso?.CargaHorariaHoras > 0 ? $"{Curso.CargaHorariaHoras} horas · Certificado de conclusão" : "Certificado de conclusão";
        public decimal Valor => Curso?.Preco ?? 0;
        public string PrecoTexto => Formatos.Moeda(Valor);

        // ---- Forma de pagamento ----

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPix), nameof(IsCredito), nameof(IsDebito), nameof(IsCartao), nameof(BotaoTexto))]
        public partial FormaPagamento Forma { get; private set; } = FormaPagamento.Pix;

        public bool IsPix => Forma == FormaPagamento.Pix;
        public bool IsCredito => Forma == FormaPagamento.CartaoCredito;
        public bool IsDebito => Forma == FormaPagamento.CartaoDebito;
        public bool IsCartao => !IsPix;

        public string BotaoTexto => IsPix ? "Já fiz o Pix" : $"Pagar {PrecoTexto}";

        partial void OnFormaChanged(FormaPagamento value) => Mensagem = string.Empty;

        // ---- Pix e cartão ----

        [ObservableProperty]
        public partial string CodigoPix { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial string NumeroCartao { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string NomeCartao { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Validade { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Cvv { get; set; } = string.Empty;

        [ObservableProperty]
        public partial List<ParcelaOpcao> Parcelas { get; private set; } = new();

        [ObservableProperty]
        public partial ParcelaOpcao? ParcelaSelecionada { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemMensagem))]
        public partial string Mensagem { get; private set; } = string.Empty;

        public bool TemMensagem => !string.IsNullOrEmpty(Mensagem);

        public override Task AoAparecerAsync() => ExecutarAsync(async () =>
        {
            var solicitacao = SessionService.SolicitacaoAtual;
            Curso = solicitacao == null ? null : await CursoRepositorio.PorIdAsync(solicitacao.CursoId);

            Parcelas = Enumerable.Range(1, MaxParcelas)
                .Select(n => new ParcelaOpcao
                {
                    Quantidade = n,
                    Texto = n == 1 ? $"À vista · {PrecoTexto}" : $"{n}x de {Formatos.Moeda(Valor / n)} sem juros"
                })
                .ToList();
            ParcelaSelecionada = Parcelas.FirstOrDefault();

            // código "copia e cola" apenas ilustrativo
            CodigoPix = $"00020126580014BR.GOV.BCB.PIX0136european-{solicitacao?.Id[^8..]}5204000053039865406{Valor.ToString("0.00", CultureInfo.InvariantCulture)}5802BR5908EUROPEAN6304ABCD";
        });

        [RelayCommand]
        private void SelecionarForma(string forma) => Forma = forma switch
        {
            "credito" => FormaPagamento.CartaoCredito,
            "debito" => FormaPagamento.CartaoDebito,
            _ => FormaPagamento.Pix
        };

        [RelayCommand]
        private async Task CopiarPixAsync()
        {
            await Clipboard.Default.SetTextAsync(CodigoPix);
            await AvisarAsync("Pix", "Código copiado.");
        }

        [RelayCommand]
        private Task Sair() => AcessoService.SairAsync();

        [RelayCommand]
        private async Task PagarAsync()
        {
            var solicitacao = SessionService.SolicitacaoAtual;
            if (solicitacao == null)
                return;

            if (IsCartao && ValidarCartao() is { } erro)
            {
                Mensagem = erro;
                return;
            }

            Mensagem = string.Empty;
            var ok = await ExecutarAsync(async () =>
            {
                await Task.Delay(1200); // simula o processamento
                var agora = DateTime.Now;
                await SolicitacaoRepositorio.RegistrarPagamentoAsync(solicitacao.Id, Forma, agora);
                solicitacao.FormaPagamento = Forma;
                solicitacao.PagamentoEm = agora;
                solicitacao.PendenciaMotivo = null;
                solicitacao.PendenciaEm = null;
            });

            if (!ok)
                return;

            NumeroCartao = NomeCartao = Validade = Cvv = string.Empty;
            await AvisarAsync("Pagamento confirmado", "Agora é só aguardar a coordenação liberar o seu acesso ao curso.");
            await Shell.Current.GoToAsync(Navegacao.Aguardando);
        }

        /// <summary>Confere o formato dos dados do cartão (nada é enviado a operadora). Retorna o erro, ou null se estiver ok.</summary>
        private string? ValidarCartao()
        {
            var digitos = new string(NumeroCartao.Where(char.IsDigit).ToArray());
            if (digitos.Length is < 13 or > 19)
                return "Informe o número do cartão.";

            if (string.IsNullOrWhiteSpace(NomeCartao))
                return "Informe o nome impresso no cartão.";

            var partes = Validade.Split('/');
            if (partes.Length != 2 || !int.TryParse(partes[0], out var mes) || !int.TryParse(partes[1], out var ano) || mes is < 1 or > 12)
                return "Informe a validade no formato MM/AA.";

            if (new DateTime(2000 + ano % 100, mes, 1).AddMonths(1) <= DateTime.Today)
                return "Este cartão está vencido.";

            if (Cvv.Length is < 3 or > 4 || !Cvv.All(char.IsDigit))
                return "Informe o código de segurança (CVV).";

            return null;
        }
    }
}
