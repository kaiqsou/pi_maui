using Microsoft.Extensions.Logging;

namespace European
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

#if WINDOWS
            ConfigurarPlayerYouTubeWindows();
#endif

            return builder.Build();
        }

#if WINDOWS
        // O WebView2 não envia origem ao carregar HTML por string; por isso a página do player é
        // entregue numa URL HTTPS (YouTubePlayer.Origem) interceptada aqui, sem acessar a rede.
        private static void ConfigurarPlayerYouTubeWindows()
        {
            Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("YouTubePlayer", (handler, _) =>
            {
                var webView = handler.PlatformView;

                void Configurar(Microsoft.Web.WebView2.Core.CoreWebView2 core)
                {
                    core.AddWebResourceRequestedFilter($"{Services.YouTubePlayer.Origem}/*",
                        Microsoft.Web.WebView2.Core.CoreWebView2WebResourceContext.All);

                    core.WebResourceRequested += (_, args) =>
                    {
                        var html = Services.YouTubePlayer.GerarHtmlDaUrl(args.Request.Uri);
                        if (html == null)
                            return;

                        var bytes = System.Text.Encoding.UTF8.GetBytes(html);
                        var stream = new MemoryStream(bytes).AsRandomAccessStream();
                        args.Response = core.Environment.CreateWebResourceResponse(
                            stream, 200, "OK", "Content-Type: text/html; charset=utf-8");
                    };
                }

                if (webView.CoreWebView2 != null)
                    Configurar(webView.CoreWebView2);
                else
                    webView.CoreWebView2Initialized += (sender, _) => Configurar(sender.CoreWebView2);
            });
        }
#endif
    }
}
