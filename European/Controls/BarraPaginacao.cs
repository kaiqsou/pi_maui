using European.ViewModels;

namespace European.Controls
{
    /// <summary>Barra "‹ 1 / 3 ›". Use com BindingContext="{Binding Paginas}" (um <see cref="Paginador"/>); some quando há só uma página.</summary>
    public class BarraPaginacao : ContentView
    {
        public BarraPaginacao()
        {
            var anterior = Botao("", nameof(Paginador.AnteriorCommand));
            var proxima = Botao("", nameof(Paginador.ProximaCommand));

            var indicador = new Label
            {
                Style = Recursos.Estilo("BodySm"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            indicador.SetBinding(Label.TextProperty, nameof(Paginador.Indicador));

            var grid = new Grid
            {
                ColumnDefinitions = { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) },
                ColumnSpacing = 12
            };
            grid.Add(anterior, 0);
            grid.Add(indicador, 1);
            grid.Add(proxima, 2);
            Content = grid;

            this.SetBinding(IsVisibleProperty, nameof(Paginador.TemPaginacao));
        }

        private static Button Botao(string icone, string comando)
        {
            var botao = new Button { Text = icone, Style = Recursos.Estilo("BotaoPaginacao") };
            botao.SetBinding(Button.CommandProperty, comando);
            return botao;
        }
    }
}
