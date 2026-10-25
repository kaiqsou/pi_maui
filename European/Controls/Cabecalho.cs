namespace European.Controls
{
    /// <summary>Cabeçalho padrão das telas internas: botão voltar + título (+ subtítulo opcional).</summary>
    public class Cabecalho : ContentView
    {
        public static readonly BindableProperty TituloProperty =
            BindableProperty.Create(nameof(Titulo), typeof(string), typeof(Cabecalho), string.Empty,
                propertyChanged: (b, _, v) => ((Cabecalho)b)._titulo.Text = v as string);

        public static readonly BindableProperty SubtituloProperty =
            BindableProperty.Create(nameof(Subtitulo), typeof(string), typeof(Cabecalho), string.Empty,
                propertyChanged: (b, _, v) =>
                {
                    var cabecalho = (Cabecalho)b;
                    cabecalho._subtitulo.Text = v as string;
                    cabecalho._subtitulo.IsVisible = !string.IsNullOrWhiteSpace(v as string);
                });

        private readonly Label _titulo;
        private readonly Label _subtitulo;

        public Cabecalho()
        {
            var voltar = new Border
            {
                Style = Estilo("BackTile"),
                Content = new Label
                {
                    Text = "",
                    Style = Estilo("Icon"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
            voltar.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await Shell.Current.GoToAsync(".."))
            });

            _titulo = new Label { Style = Estilo("TitleMd"), LineBreakMode = LineBreakMode.TailTruncation };
            _subtitulo = new Label { Style = Estilo("BodySm"), IsVisible = false };

            var grid = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 12
            };
            grid.Add(voltar, 0);
            grid.Add(new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, Children = { _titulo, _subtitulo } }, 1);
            Content = grid;
        }

        public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }
        public string Subtitulo { get => (string)GetValue(SubtituloProperty); set => SetValue(SubtituloProperty, value); }

        private static Style? Estilo(string chave)
            => Application.Current?.Resources.TryGetValue(chave, out var valor) == true ? valor as Style : null;
    }
}
