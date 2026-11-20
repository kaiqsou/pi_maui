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
                    fonts.AddFont("Lato-Regular.ttf", "Texto");
                    fonts.AddFont("PlayfairDisplay-Bold.ttf", "Titulo");
                    fonts.AddFont("Lato-Bold.ttf", "TextoNegrito");
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
            // O Windows mostra o Title do Picker como um rótulo acima da lista; aqui ele vira o texto de dica, como no celular.
            // (o MAUI reaplica o rótulo em mais de um mapeamento, então a correção vai em todos eles)
            static void TitleComoDica(Microsoft.Maui.Handlers.IPickerHandler handler, IPicker picker)
            {
                handler.PlatformView.Header = null;
                handler.PlatformView.HeaderTemplate = null;
                handler.PlatformView.PlaceholderText = picker.Title ?? string.Empty;
            }
            foreach (var mapeamento in new[] { nameof(Picker.Title), nameof(Picker.TitleColor), "PickerSemCabecalho" })
                Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping(mapeamento, TitleComoDica);

            // Texto selecionado: o Windows usa a cor de destaque do sistema (que pode ser vermelha, azul...).
            // Aqui vira o dourado escuro da marca, legível com o texto branco nos dois temas.
            var selecaoDourada = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x99, 0x6D, 0x1C));
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("SelecaoDaMarca",
                (handler, _) => handler.PlatformView.SelectionHighlightColor = selecaoDourada);
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("SelecaoDaMarca",
                (handler, _) => handler.PlatformView.SelectionHighlightColor = selecaoDourada);

            // O ToggleSwitch do Windows reserva ~154 px de largura (espaço do texto "On/Off"), espremendo o texto ao lado.
            Microsoft.Maui.Handlers.SwitchHandler.Mapper.AppendToMapping("SwitchCompacto", (handler, _) =>
            {
                handler.PlatformView.MinWidth = 0;
                handler.PlatformView.OnContent = null;
                handler.PlatformView.OffContent = null;
            });

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
