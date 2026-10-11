using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace LuminaAVA.Views;

public static class LuminaNav
{
    const string IconFont = "MaterialSymbolsOutlined";

    static Color PrimaryColor => LuminaTheme.Primary;
    static Color InactiveColor => LuminaTheme.TextVariant;
    static Color CardStroke => LuminaTheme.CardStroke;

    static readonly (string Glyph, string Title, string Route)[] Items =
    {
        ("dashboard", "Início", "inicio"),
        ("play_circle", "Aulas", "aulas"),
        ("assignment_turned_in", "Provas", "provas"),
        ("bar_chart", "Desempenho", "desempenho"),
        ("school", "Certificados", "certificados"),
    };

    public static View Build(string activeRoute)
    {
        var nav = new Grid
        {
            Padding = new Thickness(8, 8, 8, 10),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
        };

        for (var i = 0; i < Items.Length; i++)
        {
            var item = Items[i];
            var view = NavItem(item.Glyph, item.Title, item.Route == activeRoute, item.Route);
            nav.Add(view);
            nav.SetColumn(view, i);
        }

        return new Border
        {
            BackgroundColor = LuminaTheme.SurfaceCard,
            Stroke = CardStroke,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            Padding = new Thickness(0),
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#0F131B2E")),
                Offset = new Point(0, -4),
                Radius = 20,
                Opacity = 1,
            },
            Content = nav,
        };
    }

    static View NavItem(string glyph, string title, bool active, string route)
    {
        var color = active ? PrimaryColor : InactiveColor;

        var view = new VerticalStackLayout
        {
            Spacing = 2,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Image
                {
                    Source = new FontImageSource
                    {
                        FontFamily = IconFont,
                        Glyph = glyph,
                        Size = 24,
                        Color = color,
                    },
                    WidthRequest = 24,
                    HeightRequest = 24,
                },
                new Label
                {
                    Text = title,
                    FontSize = 11,
                    TextColor = color,
                    FontAttributes = active ? FontAttributes.Bold : FontAttributes.None,
                    HorizontalTextAlignment = TextAlignment.Center,
                },
            },
        };

        if (!active)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) =>
            {
                if (Shell.Current is null)
                    return;

                try
                {
                    await Shell.Current.GoToAsync($"//{route}");
                }
                catch
                {
                }
            };
            view.GestureRecognizers.Add(tap);
        }

        return view;
    }
}
