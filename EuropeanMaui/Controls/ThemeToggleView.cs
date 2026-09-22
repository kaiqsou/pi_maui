using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Alternador claro/escuro (equivalente ao ThemeToggle do front-end).</summary>
    public class ThemeToggleView : Grid, IDisposable
    {
        private readonly PressableScale _pressable;
        private readonly Border _pill;
        private readonly GlyphLabel _icon;
        private readonly Label _label;
        private ThemeSession? _theme;

        public static readonly BindableProperty VariantProperty = BindableProperty.Create(
            nameof(Variant), typeof(string), typeof(ThemeToggleView), "inline",
            propertyChanged: (b, _, _) => ((ThemeToggleView)b).ApplyVisual());

        public ThemeToggleView()
        {
            _icon = new GlyphLabel { FontSize = 20, HorizontalOptions = LayoutOptions.Center };
            _label = new Label { FontSize = 14, FontFamily = "OpenSansSemibold", VerticalTextAlignment = TextAlignment.Center };

            var grid = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 12,
            };
            grid.AddAt(_icon, 0, 0);
            grid.AddAt(_label, 1, 0);

            _pill = new Border
            {
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(12, 10),
                Content = grid,
            };

            _pressable = new PressableScale { Content = _pill };
            Children.Add(_pressable);

            _theme = ThemeSession.Instance ?? AppServices.Get<ThemeSession>();
            if (_theme == null)
                AppServices.ServicesReady += OnReady;
            else
                _theme.ThemeChanged += OnThemeChanged;

            _pressable.Command = new RelayCommand(Toggle);
            ApplyVisual();
        }

        public string Variant
        {
            get => (string)GetValue(VariantProperty);
            set => SetValue(VariantProperty, value);
        }

        private void OnReady()
        {
            _theme = ThemeSession.Instance;
            if (_theme != null)
                _theme.ThemeChanged += OnThemeChanged;
            ApplyVisual();
        }

        private void Toggle() => _theme?.Toggle();

        private void OnThemeChanged() => Dispatcher.Dispatch(ApplyVisual);

        private void ApplyVisual()
        {
            if (_pill == null) return;
            bool sidebar = Variant == "sidebar";
            _pill.BackgroundColor = sidebar ? Colors.Transparent : ThemeColors.Get("Surface");
            _pill.Stroke = sidebar ? Colors.Transparent : ThemeColors.Get("Border");
            bool isDark = _theme?.IsDark ?? false;
            _icon.Glyph = isDark ? Icons.SunnyOutline : Icons.MoonOutline;
            _icon.TextColor = ThemeColors.Get("TextMuted");
            _label.Text = isDark ? AppText.Perfil.ThemeLight : AppText.Perfil.ThemeDark;
            _label.TextColor = ThemeColors.Get("Text");
        }

        public void Dispose()
        {
            if (_theme != null)
                _theme.ThemeChanged -= OnThemeChanged;
        }
    }
}