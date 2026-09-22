using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Barra lateral de desktop (equivalente ao Sidebar do front-end).</summary>
    public class SidebarView : Grid
    {
        private readonly NavigationService _navigation;
        private readonly VerticalStackLayout _items;

        public SidebarView()
        {
            _navigation = AppServices.Get<NavigationService>()!;

            WidthRequest = 240;
            RowDefinitions.Add(new RowDefinition(GridLength.Star));
            RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var header = new VerticalStackLayout { Spacing = 2, Padding = new Thickness(20, 20, 20, 20) };
            header.Children.Add(new Label
            {
                Text = AppText.Common.AppName,
                FontFamily = "OpenSansBold",
                FontSize = 24,
                TextColor = ThemeColors.Get("PrimaryDark"),
            });
            header.Children.Add(new Label
            {
                Text = AppText.Common.InstitutionName,
                FontSize = 12,
                LineBreakMode = LineBreakMode.TailTruncation,
                TextColor = ThemeColors.Get("TextMuted"),
            });

            _items = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(8, 0) };

            var themeRow = new Grid
            {
                Padding = new Thickness(8, 8, 8, 16),
                Children = { new ThemeToggleView { Variant = "sidebar" } },
            };

            this.AddAt(header, 0, 0);
            this.AddAt(_items, 0, 1);
            this.AddAt(themeRow, 0, 2);

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
            if (_items == null) return;

            var bg = ThemeColors.Get("Surface");
            var border = ThemeColors.Get("Border");
            var (activeColor, idleColor) = NavTint.Resolve();

            BackgroundColor = bg;

            _items.Children.Clear();
            foreach (var route in NavRoutes.SidebarMain)
                _items.Children.Add(CreateItem(route, activeColor, idleColor));

            _items.Children.Add(new BoxView { HeightRequest = 1, Color = border, Margin = new Thickness(0, 8) });

            foreach (var route in NavRoutes.SidebarSecondary)
                _items.Children.Add(CreateItem(route, activeColor, idleColor));
        }

        private View CreateItem(NavRoute route, Color activeColor, Color idleColor)
        {
            var active = _navigation.CurrentKey == route.Key;

            var icon = new GlyphLabel
            {
                Glyph = active ? route.ActiveIcon : route.Icon,
                FontSize = 20,
                TextColor = active ? activeColor : idleColor,
                VerticalTextAlignment = TextAlignment.Center,
            };
            var text = new Label
            {
                Text = NavRoutes.LabelOf(route),
                FontSize = 14,
                FontFamily = active ? "OpenSansSemibold" : "OpenSansRegular",
                TextColor = active ? activeColor : ThemeColors.Get("Text"),
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
            };

            var grid = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 12,
            };
            grid.AddAt(icon, 0, 0);
            grid.AddAt(text, 1, 0);

            var border = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = active ? ThemeColors.Get("PrimaryLight") : Colors.Transparent,
                Padding = new Thickness(12, 12),
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = grid,
            };

            var row = new PressableScale { Content = border };
            row.Command = new RelayCommand(() => _navigation.Navigate(route.Key));
            return row;
        }
    }
}