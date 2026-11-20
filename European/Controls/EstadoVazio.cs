namespace European.Controls
{
    /// <summary>Lista vazia: ícone grande, título e um texto opcional de orientação (use no EmptyView das listas).</summary>
    public class EstadoVazio : ContentView
    {
        public static readonly BindableProperty IconeProperty =
            BindableProperty.Create(nameof(Icone), typeof(string), typeof(EstadoVazio), string.Empty,
                propertyChanged: (b, _, v) => ((EstadoVazio)b)._icone.Text = v as string);

        public static readonly BindableProperty TituloProperty =
            BindableProperty.Create(nameof(Titulo), typeof(string), typeof(EstadoVazio), string.Empty,
                propertyChanged: (b, _, v) => ((EstadoVazio)b)._titulo.Text = v as string);

        public static readonly BindableProperty TextoProperty =
            BindableProperty.Create(nameof(Texto), typeof(string), typeof(EstadoVazio), string.Empty,
                propertyChanged: (b, _, v) =>
                {
                    var estado = (EstadoVazio)b;
                    estado._texto.Text = v as string;
                    estado._texto.IsVisible = !string.IsNullOrWhiteSpace(v as string);
                });

        private readonly Label _icone;
        private readonly Label _titulo;
        private readonly Label _texto;

        public EstadoVazio()
        {
            _icone = Recursos.ComCor(new Label { Style = Recursos.Estilo("Icon"), FontSize = 48, HorizontalOptions = LayoutOptions.Center }, Label.TextColorProperty, "Outline");
            _titulo = new Label { Style = Recursos.Estilo("TitleSm"), HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center };
            _texto = new Label { Style = Recursos.Estilo("Muted"), HorizontalTextAlignment = TextAlignment.Center, IsVisible = false };

            Padding = new Thickness(20, 40);
            Content = new VerticalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center, Children = { _icone, _titulo, _texto } };
        }

        public string Icone { get => (string)GetValue(IconeProperty); set => SetValue(IconeProperty, value); }
        public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }
        public string Texto { get => (string)GetValue(TextoProperty); set => SetValue(TextoProperty, value); }
    }
}
