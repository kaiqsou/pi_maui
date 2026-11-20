using System.Windows.Input;

namespace European.Controls
{
    /// <summary>
    /// Campo de senha com o "olhinho" que mostra/oculta o que foi digitado.
    /// Use dentro de um Border com Style="Campo", como os outros campos: &lt;controls:CampoSenha Text="{Binding Senha}" /&gt;.
    /// </summary>
    public class CampoSenha : ContentView
    {
        private const string IconeMostrar = ""; // visibility: senha oculta, toque para ver
        private const string IconeOcultar = ""; // visibility_off: senha visível, toque para esconder

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(CampoSenha), string.Empty, BindingMode.TwoWay,
                propertyChanged: (b, _, novo) =>
                {
                    var campo = (CampoSenha)b;
                    if (campo._entrada.Text != (string?)novo)
                        campo._entrada.Text = (string?)novo;
                });

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(CampoSenha), string.Empty,
                propertyChanged: (b, _, novo) => ((CampoSenha)b)._entrada.Placeholder = (string?)novo);

        public static readonly BindableProperty ReturnCommandProperty =
            BindableProperty.Create(nameof(ReturnCommand), typeof(ICommand), typeof(CampoSenha),
                propertyChanged: (b, _, novo) => ((CampoSenha)b)._entrada.ReturnCommand = (ICommand?)novo);

        private readonly Entry _entrada = new() { IsPassword = true };
        private readonly Label _olho;

        public CampoSenha()
        {
            _entrada.TextChanged += (_, e) => Text = e.NewTextValue ?? string.Empty;

            _olho = Recursos.ComCor(new Label
            {
                Text = IconeMostrar,
                FontFamily = "MaterialIcons",
                FontSize = 20,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }, Label.TextColorProperty, "OnSurfaceVariant");

            // área de toque maior que o ícone
            var botao = new ContentView { Padding = new Thickness(8, 0, 0, 0), WidthRequest = 36, Content = _olho };
            botao.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(AlternarVisibilidade) });
            SemanticProperties.SetDescription(botao, "Mostrar senha");

            var grade = new Grid { ColumnDefinitions = { new(GridLength.Star), new(GridLength.Auto) } };
            grade.Add(_entrada, 0);
            grade.Add(botao, 1);
            Content = grade;
        }

        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
        public ICommand? ReturnCommand { get => (ICommand?)GetValue(ReturnCommandProperty); set => SetValue(ReturnCommandProperty, value); }

        /// <summary>
        /// Mostra/oculta a senha mantendo o cursor onde estava. Trocar IsPassword faz o controle nativo reescrever o
        /// texto e mandar o cursor para o início; a posição é devolvida na mesma hora, antes de a tela ser redesenhada,
        /// para o cursor não "pular".
        /// </summary>
        private void AlternarVisibilidade()
        {
            var posicao = PosicaoDoCursor();
            _entrada.IsPassword = !_entrada.IsPassword;
            _olho.Text = _entrada.IsPassword ? IconeMostrar : IconeOcultar;
            PorCursorEm(posicao);

            SemanticProperties.SetDescription((View)_olho.Parent, _entrada.IsPassword ? "Mostrar senha" : "Ocultar senha");
        }

        private int PosicaoDoCursor()
        {
#if WINDOWS
            if (_entrada.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBox caixa)
                return caixa.SelectionStart;
#endif
            return _entrada.CursorPosition;
        }

        private void PorCursorEm(int posicao)
        {
#if WINDOWS
            // direto na caixa de texto nativa: o MAUI não repassa a posição quando o valor dele "não mudou"
            if (_entrada.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBox caixa)
            {
                caixa.Select(Math.Min(posicao, caixa.Text.Length), 0);
                return;
            }
#endif
            // nas outras plataformas, muda o valor duas vezes para a posição ser reaplicada no controle nativo
            var destino = Math.Min(posicao, _entrada.Text?.Length ?? 0);
            _entrada.CursorPosition = destino == 0 ? 1 : 0;
            _entrada.CursorPosition = destino;
            _entrada.SelectionLength = 0;
        }
    }
}
