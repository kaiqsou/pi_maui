using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Avatar circular com iniciais do nome (equivalente ao Avatar do front-end).</summary>
    public class AvatarView : Grid
    {
        private readonly Label _label;

        public static readonly BindableProperty NameProperty = BindableProperty.Create(
            nameof(Name), typeof(string), typeof(AvatarView), null,
            propertyChanged: (b, _, newValue) => ((AvatarView)b).Update((string?)newValue));

        public static readonly BindableProperty SizeProperty = BindableProperty.Create(
            nameof(Size), typeof(double), typeof(AvatarView), 72d,
            propertyChanged: (b, _, newValue) => ((AvatarView)b).ApplySize((double)newValue));

        public AvatarView()
        {
            _label = new Label
            {
                FontFamily = "OpenSansBold",
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            };
            var circle = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 999 },
                BackgroundColor = Colors.Transparent,
                Content = _label,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
            Children.Add(circle);

            ApplySize(Size);
            Update(Name);

            if (ThemeSession.Instance != null)
                ThemeSession.Instance.ThemeChanged += OnThemeChanged;
            else
                AppServices.ServicesReady += OnServicesReady;
            RefreshTheme();
        }

        public string? Name
        {
            get => (string?)GetValue(NameProperty);
            set => SetValue(NameProperty, value);
        }

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public void RefreshTheme()
        {
            if (Children.Count > 0 && Children[0] is Border circle && circle.Content is Label)
            {
                circle.BackgroundColor = ThemeColors.Get("PrimaryLight");
                _label.TextColor = ThemeColors.Get("PrimaryDark");
            }
        }

        private void ApplySize(double size)
        {
            if (Children.Count == 0 || Children[0] is not Border circle) return;
            circle.WidthRequest = size;
            circle.HeightRequest = size;
            _label.FontSize = size * 0.42;
        }

        private void Update(string? name)
        {
            var initials = string.Concat(
                (name ?? string.Empty)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Take(2)
                    .Select(p => p[0]))
                .ToUpperInvariant();
            _label.Text = string.IsNullOrEmpty(initials) ? "?" : initials;
        }

        private void OnThemeChanged() => Dispatcher.Dispatch(RefreshTheme);

        private void OnServicesReady() => OnThemeChanged();
    }
}