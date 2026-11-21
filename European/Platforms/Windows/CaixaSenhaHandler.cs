using European.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace European.WinUI
{
    /// <summary>
    /// Desenha a CaixaSenha com o PasswordBox do Windows, que guarda a senha sem nunca mostrar os caracteres
    /// digitados (a não ser que o olhinho do CampoSenha peça para revelar).
    /// </summary>
    public class CaixaSenhaHandler : ViewHandler<CaixaSenha, PasswordBox>
    {
        public static readonly IPropertyMapper<CaixaSenha, CaixaSenhaHandler> Mapper =
            new PropertyMapper<CaixaSenha, CaixaSenhaHandler>(ViewMapper)
            {
                [nameof(Entry.Text)] = MapText,
                [nameof(Entry.IsPassword)] = MapIsPassword,
                [nameof(Entry.MaxLength)] = (h, v) => h.PlatformView.MaxLength = v.MaxLength < 0 ? 0 : v.MaxLength,
                [nameof(Entry.Placeholder)] = (h, v) => h.PlatformView.PlaceholderText = v.Placeholder ?? string.Empty,
                [nameof(Entry.TextColor)] = MapTextColor,
                [nameof(Entry.PlaceholderColor)] = MapPlaceholderColor,
                [nameof(Entry.FontFamily)] = MapFont,
                [nameof(Entry.FontSize)] = MapFont,
                [nameof(Entry.FontAttributes)] = MapFont,
            };

        public CaixaSenhaHandler() : base(Mapper)
        {
        }

        protected override PasswordBox CreatePlatformView() => new()
        {
            PasswordRevealMode = PasswordRevealMode.Hidden,
            BorderThickness = new Microsoft.UI.Xaml.Thickness(0),
        };

        protected override void ConnectHandler(PasswordBox platformView)
        {
            base.ConnectHandler(platformView);
            platformView.PasswordChanged += AoMudarSenha;
            platformView.KeyUp += AoSoltarTecla;
            platformView.Loaded += AoCarregar;
            platformView.SizeChanged += AoMudarTamanho;
        }

        /// <summary>
        /// O Entry do MAUI centraliza o texto na altura do campo; o PasswordBox deixa no topo. Aqui as partes
        /// internas (texto e placeholder) são centralizadas, para alinhar com os outros campos. Roda ao carregar e
        /// também quando o campo ganha tamanho: um campo que começa escondido (ex.: CVV antes de escolher o cartão)
        /// ainda não tem as partes internas no momento em que carrega.
        /// </summary>
        private static void AoCarregar(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) => Centralizar((PasswordBox)sender);

        private static void AoMudarTamanho(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e) => Centralizar((PasswordBox)sender);

        private static void Centralizar(PasswordBox caixa)
        {
            caixa.ApplyTemplate();
            caixa.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
            foreach (var nome in new[] { "ContentElement", "PlaceholderTextContentPresenter" })
                if (Procurar(caixa, nome) is { } parte)
                    parte.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
        }

        private static Microsoft.UI.Xaml.FrameworkElement? Procurar(Microsoft.UI.Xaml.DependencyObject pai, string nome)
        {
            for (var i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(pai); i++)
            {
                var filho = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(pai, i);
                if (filho is Microsoft.UI.Xaml.FrameworkElement { Name: var n } elemento && n == nome)
                    return elemento;
                if (Procurar(filho, nome) is { } achado)
                    return achado;
            }
            return null;
        }

        protected override void DisconnectHandler(PasswordBox platformView)
        {
            platformView.PasswordChanged -= AoMudarSenha;
            platformView.KeyUp -= AoSoltarTecla;
            platformView.SizeChanged -= AoMudarTamanho;
            platformView.Loaded -= AoCarregar;
            base.DisconnectHandler(platformView);
        }

        private void AoMudarSenha(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (VirtualView != null && VirtualView.Text != PlatformView.Password)
                VirtualView.Text = PlatformView.Password;
        }

        private void AoSoltarTecla(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                VirtualView?.SendCompleted();
        }

        private static void MapText(CaixaSenhaHandler handler, CaixaSenha caixa)
        {
            var texto = caixa.Text ?? string.Empty;
            if (handler.PlatformView.Password != texto)
                handler.PlatformView.Password = texto;
        }

        private static void MapIsPassword(CaixaSenhaHandler handler, CaixaSenha caixa)
            => handler.PlatformView.PasswordRevealMode = caixa.IsPassword ? PasswordRevealMode.Hidden : PasswordRevealMode.Visible;

        // As cores vão também nos recursos do controle: o Windows troca o pincel ao focar/passar o mouse.
        private static void MapTextColor(CaixaSenhaHandler handler, CaixaSenha caixa)
        {
            if (caixa.TextColor == null)
                return;

            var pincel = new WBrush(caixa.TextColor.ToWindowsColor());
            handler.PlatformView.Foreground = pincel;
            foreach (var chave in new[] { "TextControlForeground", "TextControlForegroundPointerOver", "TextControlForegroundFocused" })
                handler.PlatformView.Resources[chave] = pincel;
        }

        private static void MapPlaceholderColor(CaixaSenhaHandler handler, CaixaSenha caixa)
        {
            if (caixa.PlaceholderColor == null)
                return;

            var pincel = new WBrush(caixa.PlaceholderColor.ToWindowsColor());
            foreach (var chave in new[] { "TextControlPlaceholderForeground", "TextControlPlaceholderForegroundPointerOver", "TextControlPlaceholderForegroundFocused" })
                handler.PlatformView.Resources[chave] = pincel;
        }

        private static void MapFont(CaixaSenhaHandler handler, CaixaSenha caixa)
        {
            var fontes = handler.MauiContext?.Services.GetService<IFontManager>();
            if (fontes == null)
                return;

            var fonte = Microsoft.Maui.Font.OfSize(caixa.FontFamily, caixa.FontSize)
                .WithAttributes(caixa.FontAttributes);
            handler.PlatformView.FontFamily = fontes.GetFontFamily(fonte);
            handler.PlatformView.FontSize = fontes.GetFontSize(fonte);
        }
    }
}
