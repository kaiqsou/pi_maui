using System.Windows.Input;
using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    public enum AppButtonVariant
    {
        Primary,
        Secondary,
        Ghost,
        Danger,
        Outline
    }

    public enum AppButtonSize
    {
        Small,
        Medium,
        Large
    }

    /// <summary>Botão estilizado (equivalente ao Button do front-end).</summary>
    public class ButtonView : Grid
    {
        private readonly PressableScale _pressable;
        private readonly Border _fill;
        private readonly ActivityIndicator _spinner;
        private readonly GlyphLabel _leftIcon;
        private readonly Label _label;
        private readonly GlyphLabel _rightIcon;

        public static readonly BindableProperty TextProperty = BindableProperty.Create(
            nameof(Text), typeof(string), typeof(ButtonView), string.Empty,
            propertyChanged: (b, _, newValue) => ((ButtonView)b).UpdateText((string?)newValue));

        public static readonly BindableProperty VariantProperty = BindableProperty.Create(
            nameof(Variant), typeof(AppButtonVariant), typeof(ButtonView), AppButtonVariant.Primary,
            propertyChanged: (b, _, _) => ((ButtonView)b).ApplyVisual());

        public static readonly BindableProperty ShowLoadingProperty = BindableProperty.Create(
            nameof(ShowLoading), typeof(bool), typeof(ButtonView), false,
            propertyChanged: (b, _, newValue) => ((ButtonView)b).ApplyLoading((bool)newValue));

        public static readonly BindableProperty IsFullWidthProperty = BindableProperty.Create(
            nameof(IsFullWidth), typeof(bool), typeof(ButtonView), false,
            propertyChanged: (b, _, newValue) => ((ButtonView)b).ApplyFullWidth((bool)newValue));

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
            nameof(Command), typeof(ICommand), typeof(ButtonView), null,
            propertyChanged: (b, _, newValue) => ((ButtonView)b).SyncCommand((ICommand?)newValue));

        public static readonly BindableProperty LeftGlyphProperty = BindableProperty.Create(
            nameof(LeftGlyph), typeof(string), typeof(ButtonView), null,
            propertyChanged: (b, _, newValue) => ((ButtonView)b).ApplyLeftGlyph((string?)newValue));

        public static readonly BindableProperty RightGlyphProperty = BindableProperty.Create(
            nameof(RightGlyph), typeof(string), typeof(ButtonView), null,
            propertyChanged: (b, _, newValue) => ((ButtonView)b).ApplyRightGlyph((string?)newValue));

        public ButtonView()
        {
            HorizontalOptions = LayoutOptions.Fill;

            _leftIcon = new GlyphLabel { FontSize = 16, IsVisible = false };
            _spinner = new ActivityIndicator { HeightRequest = 18, WidthRequest = 18, IsRunning = false, IsVisible = false };
            _label = new Label
            {
                FontFamily = "OpenSansSemibold",
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            _rightIcon = new GlyphLabel { FontSize = 16, IsVisible = false, HorizontalOptions = LayoutOptions.End };

            var inner = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 8,
            };
            inner.AddAt(_leftIcon, 0, 0);
            inner.AddAt(_spinner, 1, 0);
            inner.AddAt(_label, 2, 0);
            inner.AddAt(_rightIcon, 3, 0);

            _fill = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(18, 14),
                Content = inner,
                HorizontalOptions = LayoutOptions.Fill,
            };

            _pressable = new PressableScale { Content = _fill, HorizontalOptions = LayoutOptions.Fill };

            Children.Add(_pressable);

            if (ThemeSession.Instance != null)
                ThemeSession.Instance.ThemeChanged += OnThemeChanged;
            else
                AppServices.ServicesReady += () => ThemeSession.Instance!.ThemeChanged += OnThemeChanged;

            ApplyVisual();
            ApplyLoading(ShowLoading);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public AppButtonVariant Variant
        {
            get => (AppButtonVariant)GetValue(VariantProperty);
            set => SetValue(VariantProperty, value);
        }

        public bool ShowLoading
        {
            get => (bool)GetValue(ShowLoadingProperty);
            set => SetValue(ShowLoadingProperty, value);
        }

        public bool IsFullWidth
        {
            get => (bool)GetValue(IsFullWidthProperty);
            set => SetValue(IsFullWidthProperty, value);
        }

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public string? LeftGlyph
        {
            get => (string?)GetValue(LeftGlyphProperty);
            set => SetValue(LeftGlyphProperty, value);
        }

        public string? RightGlyph
        {
            get => (string?)GetValue(RightGlyphProperty);
            set => SetValue(RightGlyphProperty, value);
        }

        public AppButtonSize ButtonSize { get; set; } = AppButtonSize.Medium;

        private void UpdateText(string? text) => _label.Text = text;

        private void SyncCommand(ICommand? command)
        {
            if (_pressable != null) _pressable.Command = command;
        }

        private void OnThemeChanged() => Dispatcher.Dispatch(ApplyVisual);

        private void ApplyFullWidth(bool fullWidth)
        {
            if (_pressable != null)
                _pressable.HorizontalOptions = fullWidth ? LayoutOptions.Fill : LayoutOptions.Center;
        }

        private void ApplyLoading(bool loading)
        {
            if (_spinner != null)
            {
                _spinner.IsVisible = loading;
                _spinner.IsRunning = loading;
                _spinner.Color = ForegroundColor();
            }
            _pressable.Enabled = !loading;
        }

        private void ApplyLeftGlyph(string? glyph)
        {
            if (_leftIcon == null) return;
            _leftIcon.Glyph = glyph ?? "";
            _leftIcon.IsVisible = !string.IsNullOrEmpty(glyph);
        }

        private void ApplyRightGlyph(string? glyph)
        {
            if (_rightIcon == null) return;
            _rightIcon.Glyph = glyph ?? "";
            _rightIcon.IsVisible = !string.IsNullOrEmpty(glyph);
        }

        private Color ForegroundColor() => Variant switch
        {
            AppButtonVariant.Primary => ThemeColors.Get("Surface"),
            AppButtonVariant.Danger => ThemeColors.Get("Surface"),
            AppButtonVariant.Secondary => ThemeColors.Get("PrimaryDark"),
            AppButtonVariant.Ghost => ThemeColors.Get("TextMuted"),
            AppButtonVariant.Outline => ThemeColors.Get("Text"),
            _ => ThemeColors.Get("Surface"),
        };

        public void ApplyVisual()
        {
            if (_fill == null) return;
            switch (Variant)
            {
                case AppButtonVariant.Primary:
                    _fill.BackgroundColor = ThemeColors.Get("Primary");
                    _fill.Stroke = Colors.Transparent;
                    break;
                case AppButtonVariant.Secondary:
                    _fill.BackgroundColor = ThemeColors.Get("PrimaryLight");
                    _fill.Stroke = Colors.Transparent;
                    break;
                case AppButtonVariant.Ghost:
                    _fill.BackgroundColor = Colors.Transparent;
                    _fill.Stroke = Colors.Transparent;
                    break;
                case AppButtonVariant.Danger:
                    _fill.BackgroundColor = ThemeColors.Get("Danger");
                    _fill.Stroke = Colors.Transparent;
                    break;
                case AppButtonVariant.Outline:
                    _fill.BackgroundColor = Colors.Transparent;
                    _fill.Stroke = ThemeColors.Get("BorderStrong");
                    _fill.StrokeThickness = 1.5;
                    break;
            }
            var fg = ForegroundColor();
            _label.TextColor = fg;
            _leftIcon.TextColor = fg;
            _rightIcon.TextColor = fg;
            _spinner.Color = fg;
        }
    }
}