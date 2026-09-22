namespace EuropeanMaui.Controls
{
    /// <summary>Label que renderiza um glyph da fonte Ionicons (equivalente ao <Icon/> do front-end).</summary>
    public class GlyphLabel : Label
    {
        public GlyphLabel()
        {
            FontFamily = "Ionicons";
        }

        public static readonly BindableProperty GlyphProperty = BindableProperty.Create(
            nameof(Glyph),
            typeof(string),
            typeof(GlyphLabel),
            string.Empty,
            propertyChanged: (bindable, _, newValue) => ((GlyphLabel)bindable).Text = (string?)newValue);

        public string Glyph
        {
            get => (string)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }
    }
}