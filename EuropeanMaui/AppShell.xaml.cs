using System.ComponentModel;
using EuropeanMaui.Controls;
using EuropeanMaui.Services;

namespace EuropeanMaui
{
    /// <summary>
    /// Host responsivo da aplicação: TopBar + Sidebar (desktop) ou BottomNav (mobile),
    /// gate de autenticação (LoginView) e troca da página atual (equivalente ao
    /// AppShell + expo-router do front-end).
    /// </summary>
    public partial class AppShell : ContentPage
    {
        private readonly NavigationService _navigation;
        private readonly LayoutService _layout;
        private readonly AuthSession _auth;

        private readonly Grid _root;
        private readonly Grid _main;
        private readonly Grid _menuOverlay;
        private readonly ColumnDefinition _sidebarColumn;
        private readonly TopBarView _topBar;
        private readonly SidebarView _sidebar;
        private readonly ScrollView _pageScroll;
        private readonly ContentView _pageHost;
        private readonly BottomNavView _bottomNav;

        public AppShell(NavigationService navigation, ThemeSession theme)
        {
            _navigation = navigation;
            _layout = AppServices.Get<LayoutService>()!;
            _auth = AppServices.Get<AuthSession>()!;

            SetDynamicResource(BackgroundColorProperty, "Background");

            _root = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star),
                },
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star) },
            };

            _topBar = new TopBarView();
            _topBar.ProfileMenuRequested += OpenProfileMenu;

            _sidebar = new SidebarView();

            _main = new Grid
            {
                ColumnDefinitions =
                {
                    (_sidebarColumn = new ColumnDefinition(new GridLength(0))),
                    new ColumnDefinition(GridLength.Star),
                },
            };

            _pageHost = new ContentView
            {
                Padding = new Thickness(24, 24, 24, 96),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
            };
            _pageScroll = new ScrollView
            {
                Content = _pageHost,
                VerticalScrollBarVisibility = ScrollBarVisibility.Default,
            };

            _main.AddAt(_sidebar, 0, 0);
            _main.AddAt(_pageScroll, 1, 0);

            _bottomNav = new BottomNavView { IsVisible = false };

            _root.AddAt(_topBar, 0, 0);
            _root.AddAt(_main, 0, 1);
            _root.AddAt(_bottomNav, 0, 1);
            _root.Add((_menuOverlay = BuildMenuOverlay()), 0, 0);
            Grid.SetRowSpan(_menuOverlay, 2);

            Content = _root;

            _navigation.PropertyChanged += OnNavigationChanged;
            _layout.PropertyChanged += OnLayoutChanged;

            ReflectLayout();
            _navigation.GoToLogin();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (_layout != null && width > 0 && height > 0)
            {
                _layout.Update(width, height);
                _pageHost.MinimumHeightRequest = Math.Max(0, height - 80);
            }
        }

        private void OnNavigationChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NavigationService.CurrentView))
                Dispatcher.Dispatch(() => _pageHost.Content = _navigation.CurrentView);
        }

        private void OnLayoutChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(LayoutService.IsDesktop) or nameof(LayoutService.IsMobile))
                Dispatcher.Dispatch(ReflectLayout);
        }

        private void ReflectLayout()
        {
            bool desktop = _layout.IsDesktop;
            _sidebarColumn.Width = desktop ? new GridLength(240) : new GridLength(0);
            _sidebar.IsVisible = desktop;
            _bottomNav.IsVisible = !desktop;
            _pageHost.Padding = desktop
                ? new Thickness(24, 24, 24, 48)
                : new Thickness(24, 24, 24, 96);
        }

        private Grid BuildMenuOverlay()
        {
            var overlay = new Grid { IsVisible = false, BackgroundColor = ThemeColors.Get("Overlay"), ZIndex = 100 };

            var dismiss = new Grid();
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => CloseMenu();
            dismiss.GestureRecognizers.Add(tap);
            overlay.Children.Add(dismiss);

            return overlay;
        }

        private Border BuildMenuCard()
        {
            var card = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                StrokeThickness = 1,
                Stroke = ThemeColors.Get("Border"),
                BackgroundColor = ThemeColors.Get("Surface"),
                Padding = new Thickness(4),
                WidthRequest = 216,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 62, 16, 0),
                ZIndex = 101,
            };

            var stack = new VerticalStackLayout { Spacing = 0 };

            var header = new VerticalStackLayout
            {
                Spacing = 1,
                Padding = new Thickness(16, 12),
            };
            header.Children.Add(new Label
            {
                Text = _auth.User?.Name ?? "",
                FontSize = 14,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("Text"),
                LineBreakMode = LineBreakMode.TailTruncation,
            });
            header.Children.Add(new Label
            {
                Text = _auth.User?.Email ?? "",
                FontSize = 12,
                TextColor = ThemeColors.Get("TextMuted"),
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
            });

            var separator = new BoxView { HeightRequest = 1, Color = ThemeColors.Get("Border"), Margin = new Thickness(0, 4) };

            stack.Children.Add(header);
            stack.Children.Add(separator);
            stack.Children.Add(MenuItem(Icons.PersonOutline, AppText.Navigation.MenuEditProfile, false, () => { CloseMenu(); _navigation.Navigate(NavigationKey.Perfil); }));
            stack.Children.Add(MenuItem(Icons.SettingsOutline, AppText.Navigation.MenuSettings, false, () => { CloseMenu(); _navigation.Navigate(NavigationKey.Configuracoes); }));
            stack.Children.Add(MenuItem(Icons.HelpCircleOutline, AppText.Navigation.MenuHelp, false, CloseMenu));
            stack.Children.Add(MenuItem(Icons.LogOutOutline, AppText.Navigation.MenuLogout, true, () =>
            {
                CloseMenu();
                _auth.SignOut();
                _navigation.GoToLogin();
            }));

            card.Content = stack;
            return card;
        }

        private View MenuItem(string icon, string label, bool danger, Action onTap)
        {
            var color = danger ? ThemeColors.Get("Danger") : ThemeColors.Get("TextMuted");

            var iconLabel = new GlyphLabel
            {
                Glyph = icon,
                FontSize = 18,
                TextColor = color,
                VerticalTextAlignment = TextAlignment.Center,
            };
            var textLabel = new Label
            {
                Text = label,
                FontSize = 14,
                TextColor = danger ? ThemeColors.Get("Danger") : ThemeColors.Get("Text"),
                VerticalTextAlignment = TextAlignment.Center,
            };

            var grid = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 12,
                Padding = new Thickness(16, 12),
            };
            grid.AddAt(iconLabel, 0, 0);
            grid.AddAt(textLabel, 1, 0);

            var row = new PressableScale { Content = grid };
            row.Command = new RelayCommand(onTap);
            return row;
        }

        private void OpenProfileMenu()
        {
            if (_menuOverlay == null) return;
            _menuOverlay.Children.Clear();
            var dismiss = new Grid();
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => CloseMenu();
            dismiss.GestureRecognizers.Add(tap);
            _menuOverlay.Children.Add(dismiss);
            _menuOverlay.Children.Add(BuildMenuCard());
            _menuOverlay.BackgroundColor = ThemeColors.Get("Overlay");
            _menuOverlay.IsVisible = true;
        }

        private void CloseMenu()
        {
            if (_menuOverlay != null)
                _menuOverlay.IsVisible = false;
        }
    }
}