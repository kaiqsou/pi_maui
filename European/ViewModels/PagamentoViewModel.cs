using System.Globalization;
using System.Windows.Input;
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
    public class PagamentoViewModel : BaseViewModel
    {
        public static readonly CultureInfo PtBr = new("pt-BR");
        private const int MaxParcelas = 12;

        private Curso? _curso;
        private FormaPagamento _forma = FormaPagamento.Pix;
        private string _numeroCartao = string.Empty;
        private string _nomeCartao = string.Empty;
        private string _validade = string.Empty;
        private string _cvv = string.Empty;
        private List<ParcelaOpcao> _parcelas = new();
        private ParcelaOpcao? _parcelaSelecionada;
        private string _mensagem = string.Empty;
        private string _codigoPix = string.Empty;

        public PagamentoViewModel()
        {
            Title = "Pagamento";
            SelecionarFormaCommand = new Command<string>(SelecionarForma);
            CopiarPixCommand = new Command(async () => await CopiarPixAsync());
            PagarCommand = new Command(async () => await PagarAsync());
            SairCommand = new Command(async () => await AcessoService.SairAsync());
        }

        public Curso? Curso
        {
            get => _curso;
            private set
            {
                if (SetProperty(ref _curso, value))
                {
                    OnPropertyChanged(nameof(CursoNome));
                    OnPropertyChanged(nameof(CargaTexto));
                    OnPropertyChanged(nameof(PrecoTexto));
                    OnPropertyChanged(nameof(BotaoTexto));
                }
            }
        }

        public string CursoNome => Curso?.Nome ?? string.Empty;
        public string CargaTexto => Curso?.CargaHorariaHoras > 0 ? $"{Curso.CargaHorariaHoras} horas · Certificado de conclusão" : "Certificado de conclusão";
        public decimal Valor => Curso?.Preco ?? 0;
        public string PrecoTexto => Valor.ToString("C", PtBr);

        // ---- Forma de pagamento ----

        public FormaPagamento Forma
        {
            get => _forma;
            private set
            {
                if (SetProperty(ref _forma, value))
                {
                    OnPropertyChanged(nameof(IsPix));
                    OnPropertyChanged(nameof(IsCredito));
                    OnPropertyChanged(nameof(IsDebito));
                    OnPropertyChanged(nameof(IsCartao));
                    OnPropertyChanged(nameof(BotaoTexto));
                    Mensagem = string.Empty;
                }
            }
        }

        public bool IsPix => Forma == FormaPagamento.Pix;
        public bool IsCredito => Forma == FormaPagamento.CartaoCredito;
        public bool IsDebito => Forma == FormaPagamento.CartaoDebito;
        public bool IsCartao => !IsPix;

        public string BotaoTexto => IsPix ? "Já fiz o Pix" : $"Pagar {PrecoTexto}";

        // ---- Pix ----

        public string CodigoPix
        {
            get => _codigoPix;
            private set => SetProperty(ref _codigoPix, value);
        }

        // ---- Cartão ----

        public string NumeroCartao { get => _numeroCartao; set => SetProperty(ref _numeroCartao, value); }
        public string NomeCartao { get => _nomeCartao; set => SetProperty(ref _nomeCartao, value); }
        public string Validade { get => _validade; set => SetProperty(ref _validade, value); }
        public string Cvv { get => _cvv; set => SetProperty(ref _cvv, value); }

        public List<ParcelaOpcao> Parcelas
        {
            get => _parcelas;
            private set => SetProperty(ref _parcelas, value);
        }

        public ParcelaOpcao? ParcelaSelecionada
        {
            get => _parcelaSelecionada;
            set => SetProperty(ref _parcelaSelecionada, value);
        }

        public string Mensagem
        {
            get => _mensagem;
            private set
            {
                if (SetProperty(ref _mensagem, value))
                    OnPropertyChanged(nameof(TemMensagem));
            }
        }

        public bool TemMensagem => !string.IsNullOrEmpty(Mensagem);

        public ICommand SelecionarFormaCommand { get; }
        public ICommand CopiarPixCommand { get; }
        public ICommand PagarCommand { get; }
        public ICommand SairCommand { get; }

        public Task CarregarAsync() => ExecutarAsync(async () =>
        {
            var solicitacao = SessionService.SolicitacaoAtual;
            Curso = solicitacao == null ? null : await CursoRepositorio.PorIdAsync(solicitacao.CursoId);

            Parcelas = Enumerable.Range(1, MaxParcelas)
                .Select(n => new ParcelaOpcao
                {
                    Quantidade = n,
                    Texto = n == 1 ? $"À vista · {PrecoTexto}" : $"{n}x de {(Valor / n).ToString("C", PtBr)} sem juros"
                })
                .ToList();
            ParcelaSelecionada = Parcelas.FirstOrDefault();

            // código "copia e cola" apenas ilustrativo
            CodigoPix = $"00020126580014BR.GOV.BCB.PIX0136european-{solicitacao?.Id[^8..]}5204000053039865406{Valor.ToString("0.00", CultureInfo.InvariantCulture)}5802BR5908EUROPEAN6304ABCD";
        });

        private void SelecionarForma(string forma) => Forma = forma switch
        {
            "credito" => FormaPagamento.CartaoCredito,
            "debito" => FormaPagamento.CartaoDebito,
            _ => FormaPagamento.Pix
        };

        private async Task CopiarPixAsync()
        {
            await Clipboard.Default.SetTextAsync(CodigoPix);
            await AvisarAsync("Pix", "Código copiado.");
        }

        private async Task PagarAsync()
        {
            var solicitacao = SessionService.SolicitacaoAtual;
            if (solicitacao == null)
                return;

            if (IsCartao && !CartaoValido(out var erro))
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

            LimparCartao();
            await AvisarAsync("Pagamento confirmado", "Agora é só aguardar a coordenação liberar o seu acesso ao curso.");
            await Shell.Current.GoToAsync(Navegacao.Aguardando);
        }

        private bool CartaoValido(out string erro)
        {
            var digitos = new string(NumeroCartao.Where(char.IsDigit).ToArray());
            if (digitos.Length is < 13 or > 19)
            {
                erro = "Informe o número do cartão.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(NomeCartao))
            {
                erro = "Informe o nome impresso no cartão.";
                return false;
            }

            var partes = Validade.Split('/');
            if (partes.Length != 2 || !int.TryParse(partes[0], out var mes) || !int.TryParse(partes[1], out var ano) || mes is < 1 or > 12)
            {
                erro = "Informe a validade no formato MM/AA.";
                return false;
            }

            var fimDoMes = new DateTime(2000 + ano % 100, mes, 1).AddMonths(1);
            if (fimDoMes <= DateTime.Today)
            {
                erro = "Este cartão está vencido.";
                return false;
            }

            if (Cvv.Length is < 3 or > 4 || !Cvv.All(char.IsDigit))
            {
                erro = "Informe o código de segurança (CVV).";
                return false;
            }

            erro = string.Empty;
            return true;
        }

        private void LimparCartao()
        {
            NumeroCartao = string.Empty;
            NomeCartao = string.Empty;
            Validade = string.Empty;
            Cvv = string.Empty;
        }
    }
}
