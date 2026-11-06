using System.Globalization;
using System.Windows.Input;

namespace European.Controls
{
    /// <summary>
    /// WebView que toca o player do YouTube (página gerada por Services/YouTubePlayer) e informa,
    /// pelos comandos, a duração real do vídeo e a posição em que o aluno está (a cada 2 segundos).
    /// </summary>
    public class PlayerYouTube : WebView
    {
        public static readonly BindableProperty DuracaoCommandProperty =
            BindableProperty.Create(nameof(DuracaoCommand), typeof(ICommand), typeof(PlayerYouTube));

        public static readonly BindableProperty PosicaoCommandProperty =
            BindableProperty.Create(nameof(PosicaoCommand), typeof(ICommand), typeof(PlayerYouTube));

        private IDispatcherTimer? _timer;

        public PlayerYouTube()
        {
            Navigated += async (_, _) =>
            {
                await DetectarDuracaoAsync();
                IniciarTimer();
            };
        }

        /// <summary>Recebe a duração do vídeo em segundos (double).</summary>
        public ICommand? DuracaoCommand { get => (ICommand?)GetValue(DuracaoCommandProperty); set => SetValue(DuracaoCommandProperty, value); }

        /// <summary>Recebe a posição atual do vídeo em segundos (double).</summary>
        public ICommand? PosicaoCommand { get => (ICommand?)GetValue(PosicaoCommandProperty); set => SetValue(PosicaoCommandProperty, value); }

        /// <summary>Pausa o vídeo e informa a posição em que parou (chamar quando a tela some).</summary>
        public async void Pausar()
        {
            PararTimer();
            Informar(PosicaoCommand, await AvaliarAsync("window.europeanPlayer?window.europeanPlayer.pausar():'0'"));
        }

        private async Task DetectarDuracaoAsync()
        {
            // o player carrega de forma assíncrona; a posição inicial já vai no HTML (playerVars.start)
            for (var tentativa = 0; tentativa < 50; tentativa++)
            {
                if (Informar(DuracaoCommand, await AvaliarAsync("window.europeanPlayer?window.europeanPlayer.duracao():''")))
                    return;

                await Task.Delay(300);
            }
        }

        private void IniciarTimer()
        {
            PararTimer();
            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += async (_, _) =>
                Informar(PosicaoCommand, await AvaliarAsync("window.europeanPlayer?window.europeanPlayer.posicao():''"));
            _timer.Start();
        }

        private void PararTimer()
        {
            _timer?.Stop();
            _timer = null;
        }

        /// <summary>Envia o número (em segundos) ao comando; retorna false se o player ainda não respondeu.</summary>
        private static bool Informar(ICommand? comando, string resultado)
        {
            if (!double.TryParse(resultado, NumberStyles.Float, CultureInfo.InvariantCulture, out var segundos) || segundos <= 0)
                return false;

            if (comando?.CanExecute(segundos) == true)
                comando.Execute(segundos);
            return true;
        }

        private async Task<string> AvaliarAsync(string expressao)
        {
            try
            {
                return await EvaluateJavaScriptAsync($"(function(){{return {expressao};}})();") ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
