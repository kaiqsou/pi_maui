using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Campo de texto rotulado com prefixo/sufixo e feedback de erro/foco.</summary>
    public class InputView : Grid
    {
        private readonly Border _shell;
        private readonly GlyphLabel _prefix;
        private readonly Entry _entry;
        private readonly GlyphLabel _suffix;
        private readonly Label _message;

        public static readonly BindableProperty LabelProperty = BindableProperty.Create(
            nameof(Label), typeof(string), typeof(InputView), null,
            propertyChanged: (b, _, newValue) => ((InputView)b).UpdateLabel((string?)newValue));

        public static readonly BindableProperty TextProperty = BindableProperty.Create(
            nameof(Text), typeof(string), typeof(InputView), string.Empty,
            BindingMode.TwoWay,
            propertyChanged: (b, _, newValue) => ((InputView)b).UpdateText((string?)newValue));

        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
            nameof(Placeholder), typeof(string), typeof(InputView), null,
            propertyChanged: (b, _, newValue) => ((InputView)b).UpdatePlaceholder((string?)newValue));

        public static readonly BindableProperty ErrorTextProperty = BindableProperty.Create(
            nameof(ErrorText), typeof(string), typeof(InputView), null,
            propertyChanged: (b, _, newValue) => ((InputView)b).UpdateError((string?)newValue));

        public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(
            nameof(IsPassword), typeof(bool), typeof(InputView), false,
            propertyChanged: (b, _, newValue) => ((InputView)b).UpdatePassword((bool)newValue));

        public static readonly BindableProperty PrefixGlyphProperty = BindableProperty.Create(
            nameof(PrefixGlyph), typeof(string), typeof(InputView), null,
            propertyChanged: (b, _, newValue) => ((InputView)b).UpdatePrefix((string?)newValue));

        public static readonly BindableProperty SuffixGlyphProperty = BindableProperty.Create(
            nameof(SuffixGlyph), typeof(string), typeof(InputView), null);

        public InputView()
        {
            RowSpacing = 4;

            _shell = new Border
            {
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                BackgroundColor = ThemeColors.Get("Surface"),
                Padding = new Thickness(16, 0),
                HeightRequest = 48,
                VerticalOptions = LayoutOptions.Center,
            };

            _prefix = new GlyphLabel { FontSize = 18, TextColor = ThemeColors.Get("TextMuted"), VerticalTextAlignment = TextAlignment.Center };

            _entry = new Entry
            {
                FontSize = 15,
                TextColor = ThemeColors.Get("Text"),
                PlaceholderColor = ThemeColors.Get("TextMuted"),
                BackgroundColor = Colors.Transparent,
                VerticalTextAlignment = TextAlignment.Center,
            };

            _suffix = new GlyphLabel
            {
                FontSize = 18,
                TextColor = ThemeColors.Get("TextMuted"),
                VerticalTextAlignment = TextAlignment.Center,
                IsVisible = false,
            };

            var inner = new Grid { ColumnSpacing = 12 };
            inner.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            inner.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            inner.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            inner.AddAt(_prefix, 0, 0);
            inner.AddAt(_entry, 1, 0);
            inner.AddAt(_suffix, 2, 0);
            _shell.Content = inner;

            _message = new Label
            {
                FontSize = 12,
                TextColor = ThemeColors.Get("TextMuted"),
                IsVisible = false,
                LineBreakMode = LineBreakMode.WordWrap,
            };

            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var caption = new Label
            {
                Text = Label ?? "",
                FontSize = 12,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("TextMuted"),
                IsVisible = false,
            };
            this.AddAt(caption, 0, 0);
            this.AddAt(_shell, 0, 1);
            this.AddAt(_message, 0, 2);

            _entry.Focused += OnFocused;
            _entry.Unfocused += OnUnfocused;
            _entry.TextChanged += (_, e) =>
            {
                SetValue(TextProperty, e.NewTextValue);
                TextChanged?.Invoke(this, e);
            };

            var eyeTap = new TapGestureRecognizer();
            eyeTap.Tapped += (_, _) => _entry.IsPassword = !_entry.IsPassword;
            _suffix.GestureRecognizers.Add(eyeTap);

            if (ThemeSession.Instance != null)
                ThemeSession.Instance.ThemeChanged += OnThemeChanged;
            else
                AppServices.ServicesReady += () => ThemeSession.Instance!.ThemeChanged += OnThemeChanged;

            ApplyShellBorder();
        }

        public event EventHandler<TextChangedEventArgs>? TextChanged;

        public string? Label
        {
            get => (string?)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string? Placeholder { get; set; }

        public string? ErrorText
        {
            get => (string?)GetValue(ErrorTextProperty);
            set => SetValue(ErrorTextProperty, value);
        }

        public bool IsPassword
        {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }

        public string? PrefixGlyph { get; set; }

        public string? SuffixGlyph { get; set; }

        public Keyboard Keyboard
        {
            get => _entry.Keyboard;
            set => _entry.Keyboard = value;
        }

        private void UpdateLabel(string? text)
        {
            if (Children.Count > 0 && Children[0] is Label cap)
            {
                cap.Text = text ?? "";
                cap.IsVisible = !string.IsNullOrEmpty(text);
            }
        }

        private void UpdateText(string? text)
        {
            if (_entry != null && _entry.Text != text)
                _entry.Text = text;
        }

        private void UpdatePlaceholder(string? text) => _entry.Placeholder = text;

        private void UpdateError(string? error)
        {
            if (_message == null) return;
            _message.Text = error;
            _message.TextColor = ThemeColors.Get("Danger");
            _message.IsVisible = !string.IsNullOrEmpty(error);
            ApplyShellBorder();
        }

        private void UpdatePassword(bool isPassword)
        {
            if (_entry == null) return;
            _entry.IsPassword = isPassword;
            _suffix.IsVisible = isPassword;
            _suffix.Glyph = isPassword ? Icons.EyeOutline : Icons.EyeOffOutline;
        }

        private void UpdatePrefix(string? glyph)
        {
            if (_prefix == null) return;
            _prefix.Glyph = glyph ?? "";
            _prefix.IsVisible = !string.IsNullOrEmpty(glyph);
        }

        private void OnFocused(object? sender, FocusEventArgs e) => ApplyShellBorder();

        private void OnUnfocused(object? sender, FocusEventArgs e) => ApplyShellBorder();

        private void ApplyShellBorder()
        {
            if (_shell == null) return;
            bool hasError = !string.IsNullOrEmpty(ErrorText);
            if (hasError)
                _shell.Stroke = ThemeColors.Get("Danger");
            else if (_entry?.IsFocused == true)
                _shell.Stroke = ThemeColors.Get("Primary");
            else
                _shell.Stroke = ThemeColors.Get("Border");
        }

        private void OnThemeChanged() => Dispatcher.Dispatch(() =>
        {
            ApplyShellBorder();
            if (_entry != null)
            {
                _entry.TextColor = ThemeColors.Get("Text");
                _entry.PlaceholderColor = ThemeColors.Get("TextMuted");
            }
            _shell.BackgroundColor = ThemeColors.Get("Surface");
            _prefix.TextColor = ThemeColors.Get("TextMuted");
            _suffix.TextColor = ThemeColors.Get("TextMuted");
            if (Children.Count > 0 && Children[0] is Label cap)
            {
                cap.TextColor = ThemeColors.Get("TextMuted");
            }
            if (_message != null) _message.TextColor = ThemeColors.Get("Danger");
        });
    }
}