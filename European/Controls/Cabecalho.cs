using European.Services;

namespace European.Controls
{
    /// <summary>
    /// Cabeçalho padrão das telas internas: botão voltar + título (+ subtítulo opcional)
    /// e, à direita, uma ação opcional (ex.: botão de opções).
    /// </summary>
    [ContentProperty(nameof(Acao))]
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

        public static readonly BindableProperty AcaoProperty =
            BindableProperty.Create(nameof(Acao), typeof(View), typeof(Cabecalho),
                propertyChanged: (b, antiga, nova) => ((Cabecalho)b).TrocarAcao(antiga as View, nova as View));

        private readonly Grid _grade;
        private readonly Label _titulo;
        private readonly Label _subtitulo;

        public Cabecalho()
        {
            var voltar = new Border
            {
                Style = Recursos.Estilo("BackTile"),
                Content = new Label
                {
                    Text = "",
                    Style = Recursos.Estilo("Icon"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
            voltar.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Navegacao.VoltarAsync()) });

            _titulo = new Label { Style = Recursos.Estilo("TitleMd"), LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 };
            _subtitulo = new Label { Style = Recursos.Estilo("BodySm"), IsVisible = false };

            _grade = new Grid
            {
                ColumnDefinitions = { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) },
                ColumnSpacing = 12
            };
            _grade.Add(voltar, 0);
            _grade.Add(new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, Children = { _titulo, _subtitulo } }, 1);
            Content = _grade;
        }

        public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }
        public string Subtitulo { get => (string)GetValue(SubtituloProperty); set => SetValue(SubtituloProperty, value); }

        /// <summary>Conteúdo opcional à direita do título.</summary>
        public View? Acao { get => (View?)GetValue(AcaoProperty); set => SetValue(AcaoProperty, value); }

        private void TrocarAcao(View? antiga, View? nova)
        {
            if (antiga != null)
                _grade.Remove(antiga);
            if (nova != null)
                _grade.Add(nova, 2);
        }
    }
}
