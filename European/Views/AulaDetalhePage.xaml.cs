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

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var aula = query.TryGetValue("Aula", out var a) ? a as Aula : null;
            var matricula = query.TryGetValue("Matricula", out var m) ? m as Matricula : null;

            if (aula != null)
                _viewModel.Carregar(aula, matricula);
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
            var duracao = 0.0;

            for (var tentativa = 0; tentativa < 20; tentativa++)
            {
                var resultado = await AvaliarAsync(
                    "(function(){var v=document.querySelector('video');" +
                    "if(!v||!isFinite(v.duration))return '';" +
                    "return String(v.duration);})();");

                if (double.TryParse(resultado, NumberStyles.Float, CultureInfo.InvariantCulture, out duracao) && duracao > 0)
                    break;

                await Task.Delay(200);
            }

            if (duracao > 0)
            {
                _viewModel.RegistrarDuracao(duracao);

                var resume = _viewModel.SegundosParaRetomar.ToString("0.###", CultureInfo.InvariantCulture);
                await AvaliarAsync(
                    "(function(){var v=document.querySelector('video');" +
                    "if(!v)return;" +
                    $"var r={resume};" +
                    "if(r>0&&isFinite(v.duration)&&r<v.duration){v.currentTime=r;}" +
                    "if(v.paused){var p=v.play();p&&p.catch&&p.catch(function(){});}})();");
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
                "(function(){var v=document.querySelector('video');" +
                "if(!v)return '';" +
                "return String(v.currentTime);})();");

            if (double.TryParse(resultado, NumberStyles.Float, CultureInfo.InvariantCulture, out var posicao) && posicao > 0)
                _viewModel.RegistrarPosicao(posicao);
        }

        private async void PausarESalvarPosicao()
        {
            PararTimerPosicao();

            var resultado = await AvaliarAsync(
                "(function(){var v=document.querySelector('video');" +
                "if(!v)return '0';" +
                "var t=v.currentTime;" +
                "v.pause();" +
                "return String(t);})();");

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