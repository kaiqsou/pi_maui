using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Tone de cor de um BadgeView.</summary>
    public enum BadgeTone
    {
        Primary,
        Accent,
        Success,
        Warning,
        Danger,
        Neutral
    }

    /// <summary>Centraliza a busca de cores do tema atual.</summary>
    public static class ThemeColors
    {
        public static Color Get(string key)
            => Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color c
                ? c
                : Colors.Gray;
    }

    /// <summary>Badge (pílula) com cores do tema. Uso: definir Label e Tone.</summary>
    public class BadgeView : Grid
    {
        private readonly Border _pill;
        private readonly Label _label;

        public static readonly BindableProperty LabelProperty = BindableProperty.Create(
            nameof(Label), typeof(string), typeof(BadgeView), string.Empty,
            propertyChanged: (b, _, newValue) => ((BadgeView)b).UpdateLabel((string?)newValue));

        public static readonly BindableProperty ToneProperty = BindableProperty.Create(
            nameof(Tone), typeof(BadgeTone), typeof(BadgeView), BadgeTone.Primary,
            propertyChanged: (b, _, _) => ((BadgeView)b).RefreshTheme());

        public BadgeView()
        {
            _label = new Label
            {
                FontSize = 12,
                FontFamily = "OpenSansSemibold",
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.Center,
            };
            _pill = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 999 },
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(10, 4),
                Content = _label,
                HorizontalOptions = LayoutOptions.Start,
            };
            Children.Add(_pill);
            HorizontalOptions = LayoutOptions.Start;

            if (ThemeSession.Instance != null)
                ThemeSession.Instance.ThemeChanged += OnThemeChanged;
            else
                AppServices.ServicesReady += OnServicesReady;

            RefreshTheme();
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public BadgeTone Tone
        {
            get => (BadgeTone)GetValue(ToneProperty);
            set => SetValue(ToneProperty, value);
        }

        private (string bg, string fg) ToneColors => Tone switch
        {
            BadgeTone.Accent => ("AccentLight", "Accent"),
            BadgeTone.Success => ("SuccessLight", "Success"),
            BadgeTone.Warning => ("WarningLight", "Warning"),
            BadgeTone.Danger => ("DangerLight", "Danger"),
            BadgeTone.Neutral => ("SurfaceAlt", "TextMuted"),
            _ => ("PrimaryLight", "PrimaryDark")
        };

        public void RefreshTheme()
        {
            var (bg, fg) = ToneColors;
            _pill.BackgroundColor = ThemeColors.Get(bg);
            _label.TextColor = ThemeColors.Get(fg);
        }

        private void UpdateLabel(string? text) => _label.Text = text;

        private void OnThemeChanged() => Dispatcher.Dispatch(RefreshTheme);

        private void OnServicesReady() => OnThemeChanged();
    }
}