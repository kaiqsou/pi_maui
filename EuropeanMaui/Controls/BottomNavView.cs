using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Barra de navegação inferior para mobile (equivalente ao BottomNav do front-end).</summary>
    public class BottomNavView : Grid
    {
        private readonly NavigationService _navigation;

        public BottomNavView()
        {
            _navigation = AppServices.Get<NavigationService>()!;

            Padding = new Thickness(8, 6);
            ColumnSpacing = 4;
            BackgroundColor = ThemeColors.Get("Surface");

            _navigation.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(NavigationService.CurrentKey))
                    Rebuild();
            };
            if (ThemeSession.Instance != null)
                ThemeSession.Instance.ThemeChanged += Rebuild;
            else
                AppServices.ServicesReady += () => ThemeSession.Instance!.ThemeChanged += Rebuild;

            Rebuild();
        }

        private void Rebuild()
        {
            Children.Clear();
            ColumnDefinitions.Clear();

            var (activeColor, idleColor) = NavTint.Resolve();
            BackgroundColor = ThemeColors.Get("Surface");

            foreach (var route in NavRoutes.BottomNav)
            {
                ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                this.AddAt(CreateItem(route, activeColor, idleColor), ColumnDefinitions.Count - 1, 0);
            }
        }

        private View CreateItem(NavRoute route, Color activeColor, Color idleColor)
        {
            var active = _navigation.CurrentKey == route.Key;
            var color = active ? activeColor : idleColor;

            var icon = new GlyphLabel
            {
                Glyph = active ? route.ActiveIcon : route.Icon,
                FontSize = 22,
                TextColor = color,
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Center,
            };
            var label = new Label
            {
                Text = NavRoutes.LabelOf(route),
                FontSize = 12,
                FontFamily = active ? "OpenSansSemibold" : "OpenSansRegular",
                TextColor = color,
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
            };

            var inner = new VerticalStackLayout { Spacing = 2, Padding = new Thickness(0, 6) };
            inner.Children.Add(icon);
            inner.Children.Add(label);

            var row = new PressableScale { Content = inner, HorizontalOptions = LayoutOptions.Fill };
            row.Command = new RelayCommand(() => _navigation.Navigate(route.Key));
            return row;
        }
    }
}