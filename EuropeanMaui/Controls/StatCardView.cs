using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Card de estatística resumida (equivalente ao StatCard do front-end).</summary>
    public class StatCardView : Border
    {
        public StatCardView(string glyph, double value, string label, double maxValue = 100)
        {
            StrokeThickness = 1;
            Stroke = ThemeColors.Get("Border");
            StrokeShape = new RoundRectangle { CornerRadius = 16 };
            BackgroundColor = Colors.Transparent;
            Padding = new Thickness(20);

            var iconBox = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                BackgroundColor = ThemeColors.Get("SurfaceAlt"),
                WidthRequest = 36,
                HeightRequest = 36,
                Content = new GlyphLabel
                {
                    Glyph = glyph,
                    FontSize = 18,
                    TextColor = ThemeColors.Get("Accent"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
            };

            var colorKey = maxValue > 0 && value >= maxValue ? "Success" : "Accent";
            var valueLabel = new Label
            {
                FontSize = 24,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get(colorKey),
            };

            var caption = new Label
            {
                Text = label,
                FontSize = 12,
                TextColor = ThemeColors.Get("TextMuted"),
            };

            var stack = new VerticalStackLayout { Spacing = 4 };
            stack.Children.Add(iconBox);
            stack.Children.Add(valueLabel);
            stack.Children.Add(caption);

            Content = stack;
            Value = valueLabel;
        }

        public Label Value { get; }
    }
}