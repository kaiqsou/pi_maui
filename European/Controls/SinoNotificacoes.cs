using European.ViewModels;

namespace European.Controls
{
    /// <summary>Botão de sino com o número de notificações não lidas. Use com BindingContext="{Binding Sino}".</summary>
    public class SinoNotificacoes : ContentView
    {
        public SinoNotificacoes()
        {
            var botao = new Border
            {
                Style = Recursos.Estilo("BackTile"),
                WidthRequest = 44,
                HeightRequest = 44,
                Content = new Label
                {
                    Text = "", // notifications
                    Style = Recursos.Estilo("IconPrimary"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };

            var contador = new Label
            {
                FontSize = 10,
                FontFamily = "OpenSansSemibold",
                TextColor = Recursos.Cor("BrandOnPrimaryContainer"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            contador.SetBinding(Label.TextProperty, nameof(Sino.Contador));

            var marcador = new Border
            {
                BackgroundColor = Recursos.Cor("Danger"),
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 9 },
                MinimumWidthRequest = 18,
                HeightRequest = 18,
                Padding = new Thickness(4, 0),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, -4, -4, 0),
                Content = contador
            };
            marcador.SetBinding(IsVisibleProperty, nameof(Sino.TemNaoLidas));

            var grade = new Grid { Children = { botao, marcador } };
            grade.GestureRecognizers.Add(CriarToque());
            Content = grade;
            SemanticProperties.SetDescription(this, "Notificações");
        }

        private static TapGestureRecognizer CriarToque()
        {
            var toque = new TapGestureRecognizer();
            toque.SetBinding(TapGestureRecognizer.CommandProperty, nameof(Sino.AbrirCommand));
            return toque;
        }
    }
}
