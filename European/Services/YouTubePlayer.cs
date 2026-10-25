using System.Globalization;
using System.Text.RegularExpressions;

namespace European.Services
{
    /// <summary>
    /// Monta a página do player do YouTube (IFrame API) exibida no WebView das aulas.
    /// A página é servida a partir de uma origem HTTPS fictícia para que o YouTube
    /// receba Referer/origin válidos e não bloqueie o vídeo (erro 153 / "vídeo indisponível").
    /// </summary>
    public static partial class YouTubePlayer
    {
        public const string Origem = "https://european.app";
        public const string VideoPadrao = "https://www.youtube.com/watch?v=M7lc1UVf-VE";

        [GeneratedRegex(@"^[A-Za-z0-9_-]{11}$")]
        private static partial Regex IdRegex();

        [GeneratedRegex(@"(?:youtu\.be/|youtube(?:-nocookie)?\.com/(?:watch\?(?:.*&)?v=|embed/|shorts/|live/|v/))([A-Za-z0-9_-]{11})")]
        private static partial Regex UrlRegex();

        /// <summary>Aceita uma URL do YouTube (watch, youtu.be, embed, shorts, live) ou o ID puro.</summary>
        public static string? ExtrairId(string? urlOuId)
        {
            if (string.IsNullOrWhiteSpace(urlOuId))
                return null;

            var valor = urlOuId.Trim();
            if (IdRegex().IsMatch(valor))
                return valor;

            var match = UrlRegex().Match(valor);
            return match.Success ? match.Groups[1].Value : null;
        }

        public static string UrlDoPlayer(string videoId, double inicioSegundos) =>
            $"{Origem}/player?v={videoId}&t={(int)Math.Max(0, inicioSegundos)}";

        /// <summary>Gera o HTML a partir de uma URL criada por <see cref="UrlDoPlayer"/>.</summary>
        public static string? GerarHtmlDaUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return null;

            string? id = null;
            var inicio = 0;

            foreach (var parte in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = parte.Split('=', 2);
                if (kv.Length != 2)
                    continue;

                if (kv[0] == "v")
                    id = ExtrairId(Uri.UnescapeDataString(kv[1]));
                else if (kv[0] == "t")
                    int.TryParse(kv[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out inicio);
            }

            return id == null ? null : GerarHtml(id, inicio);
        }

        public static string GerarHtml(string videoId, double inicioSegundos)
        {
            var inicio = ((int)Math.Max(0, inicioSegundos)).ToString(CultureInfo.InvariantCulture);

            return $$"""
                <!DOCTYPE html>
                <html>
                <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1" />
                <meta name="referrer" content="strict-origin-when-cross-origin" />
                <style>
                  html, body { margin: 0; padding: 0; height: 100%; background: #000; overflow: hidden; }
                  #player { position: absolute; inset: 0; width: 100%; height: 100%; }
                </style>
                </head>
                <body>
                <div id="player"></div>
                <script>
                  var player = null;
                  var pronto = false;

                  window.europeanPlayer = {
                    duracao: function () { return pronto ? String(player.getDuration() || '') : ''; },
                    posicao: function () { return pronto ? String(player.getCurrentTime() || 0) : ''; },
                    pausar: function () {
                      if (!pronto) return '0';
                      var t = player.getCurrentTime() || 0;
                      player.pauseVideo();
                      return String(t);
                    }
                  };

                  function onYouTubeIframeAPIReady() {
                    player = new YT.Player('player', {
                      width: '100%',
                      height: '100%',
                      videoId: '{{videoId}}',
                      playerVars: {
                        start: {{inicio}},
                        autoplay: 1,
                        playsinline: 1,
                        rel: 0,
                        modestbranding: 1,
                        origin: '{{Origem}}'
                      },
                      events: {
                        onReady: function () { pronto = true; }
                      }
                    });
                  }
                </script>
                <script src="https://www.youtube.com/iframe_api"></script>
                </body>
                </html>
                """;
        }
    }
}
