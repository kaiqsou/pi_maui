using System.Collections.ObjectModel;

namespace European.Controls
{
    /// <summary>Barra inferior de abas: uma linha divisória e os itens (AbaBotao) lado a lado, com a mesma largura.</summary>
    [ContentProperty(nameof(Abas))]
    public class BarraAbas : ContentView
    {
        private readonly Grid _grade = new() { Padding = new Thickness(4, 10, 4, 12) };

        public BarraAbas()
        {
            Abas.CollectionChanged += (_, _) => Montar();
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new BoxView { HeightRequest = 1, Color = Recursos.Cor("Outline") },
                    _grade
                }
            };
        }

        public ObservableCollection<View> Abas { get; } = new();

        private void Montar()
        {
            _grade.Children.Clear();
            _grade.ColumnDefinitions.Clear();
            for (var i = 0; i < Abas.Count; i++)
            {
                _grade.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                _grade.Add(Abas[i], i);
            }
        }
    }
}
