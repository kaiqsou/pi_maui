using System.Globalization;
using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class AulaDetalhePage : ContentPage, IQueryAttributable
    {
        private readonly AulaViewModel _viewModel;
        private IDispatcherTimer? _timerPosicao;

        public AulaDetalhePage()
        {
            InitializeComponent();
            _viewModel = new AulaViewModel();
            BindingContext = _viewModel;
            VideoPlayer.Navigated += OnVideoNavigated;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var aula = query.TryGetValue("Aula", out var a) ? a as Aula : null;
            var matricula = query.TryGetValue("Matricula", out var m) ? m as Matricula : null;

            if (aula != null)
                await _viewModel.CarregarAsync(aula, matricula);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_viewModel.Aula != null)
                _viewModel.RecarregarVideo();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            PausarESalvarPosicao();
        }

        private async void OnVideoNavigated(object? sender, WebNavigatedEventArgs e)
        {
            await InicializarPosicaoAsync();
            IniciarTimerPosicao();
        }

        private async Task InicializarPosicaoAsync()
        {
            // o player do YouTube carrega de forma assíncrona; a posição inicial já vai no HTML (playerVars.start)
            for (var tentativa = 0; tentativa < 50; tentativa++)
            {
                var resultado = await AvaliarAsync(
                    "(function(){return window.europeanPlayer?window.europeanPlayer.duracao():'';})();");

                if (double.TryParse(resultado, NumberStyles.Float, CultureInfo.InvariantCulture, out var duracao) && duracao > 0)
                {
                    _viewModel.RegistrarDuracao(duracao);
                    break;
                }

                await Task.Delay(300);
            }
        }

        private void IniciarTimerPosicao()
        {
            PararTimerPosicao();

            _timerPosicao = Dispatcher.CreateTimer();
            _timerPosicao.Interval = TimeSpan.FromSeconds(2);
            _timerPosicao.Tick += (_, _) => SalvarPosicaoAtual();
            _timerPosicao.Start();
        }

        private void PararTimerPosicao()
        {
            if (_timerPosicao == null)
                return;

            _timerPosicao.Stop();
            _timerPosicao = null;
        }

        private async void SalvarPosicaoAtual()
        {
            var resultado = await AvaliarAsync(
                "(function(){return window.europeanPlayer?window.europeanPlayer.posicao():'';})();");

            if (double.TryParse(resultado, NumberStyles.Float, CultureInfo.InvariantCulture, out var posicao) && posicao > 0)
                _viewModel.RegistrarPosicao(posicao);
        }

        private async void PausarESalvarPosicao()
        {
            PararTimerPosicao();

            var resultado = await AvaliarAsync(
                "(function(){return window.europeanPlayer?window.europeanPlayer.pausar():'0';})();");

            if (double.TryParse(resultado, NumberStyles.Float, CultureInfo.InvariantCulture, out var posicao) && posicao > 0)
                _viewModel.RegistrarPosicao(posicao);
        }

        private async Task<string> AvaliarAsync(string script)
        {
            try
            {
                return await VideoPlayer.EvaluateJavaScriptAsync(script);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}