using EuropeanMaui.Controls;
using EuropeanMaui.Services;
using EuropeanMaui.ViewModels;

namespace EuropeanMaui.Views
{
    /// <summary>Menu "Mais" com atalhos (equivalente ao MoreScreen).</summary>
    public class MaisView : Grid
    {
        public MaisView(MaisViewModel vm)
        {
            BackgroundColor = Colors.Transparent;

            var header = new HeaderView { Title = vm.Title };

            var grid = new Grid { ColumnSpacing = 12, RowSpacing = 12 };
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            int col = 0, row = 0;
            foreach (var card in vm.Cards)
            {
                var item = NavCard(card.Icon, card.Label, card.Command);
                grid.Add(item, col, row);
                col++;
                if (col == 2) { col = 0; row++; }
            }

            var content = new VerticalStackLayout { Spacing = 16, HorizontalOptions = LayoutOptions.Center, MaximumWidthRequest = 1200 };
            content.Children.Add(header);
            content.Children.Add(grid);

            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            RowDefinitions.Add(new RowDefinition(GridLength.Star));
            this.AddAt(content, 0, 0);
        }

        private static PressableScale NavCard(string icon, string label, System.Windows.Input.ICommand command)
        {
            var iconBox = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                BackgroundColor = ThemeColors.Get("PrimaryLight"),
                WidthRequest = 44,
                HeightRequest = 44,
                Content = new GlyphLabel
                {
                    Glyph = icon,
                    FontSize = 22,
                    TextColor = ThemeColors.Get("PrimaryDark"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
            };
            var text = new Label
            {
                Text = label,
                FontSize = 15,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("Text"),
                VerticalOptions = LayoutOptions.Center,
            };
            var chevron = new GlyphLabel
            {
                Glyph = Icons.ChevronForward,
                FontSize = 20,
                TextColor = ThemeColors.Get("TextMuted"),
                VerticalOptions = LayoutOptions.Center,
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 12,
            };
            grid.AddAt(iconBox, 0, 0);
            grid.AddAt(text, 1, 0);
            grid.AddAt(chevron, 2, 0);

            var card = new Border
            {
                StrokeThickness = 1,
                Stroke = ThemeColors.Get("Border"),
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                BackgroundColor = ThemeColors.Get("Surface"),
                Padding = new Thickness(16, 14),
                Content = grid,
            };

            var pressable = new PressableScale { Content = card, Command = command };
            return pressable;
        }
    }
}