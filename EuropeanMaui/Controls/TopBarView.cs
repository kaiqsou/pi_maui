using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    /// <summary>Barra superior (equivalente ao TopBar do front-end).</summary>
    public class TopBarView : Grid
    {
        public event Action? ProfileMenuRequested;

        private readonly NavigationService _navigation;
        private readonly LayoutService _layout;
        private readonly AuthSession _auth;
        private readonly Grid _left;
        private readonly Grid _right;
        private int _notificationCount = 3;

        public TopBarView()
        {
            _navigation = AppServices.Get<NavigationService>()!;
            _layout = AppServices.Get<LayoutService>()!;
            _auth = AppServices.Get<AuthSession>()!;

            Padding = new Thickness(20, 8);
            BackgroundColor = ThemeColors.Get("Surface");
            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            _left = new Grid { ColumnSpacing = 8 };
            _right = new Grid { ColumnSpacing = 8 };
            this.AddAt(_left, 0, 0);
            this.AddAt(_right, 1, 0);

            if (ThemeSession.Instance != null)
                ThemeSession.Instance.ThemeChanged += ApplyTheme;
            else
                AppServices.ServicesReady += () => ThemeSession.Instance!.ThemeChanged += ApplyTheme;
            _layout.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(LayoutService.IsDesktop))
                    RebuildLeft();
            };

            RebuildLeft();
            RebuildRight();
        }

        public int NotificationCount
        {
            get => _notificationCount;
            set
            {
                _notificationCount = value;
                RebuildRight();
            }
        }

        private void RebuildLeft()
        {
            _left.Children.Clear();
            _left.ColumnDefinitions.Clear();

            if (_layout.IsDesktop)
            {
                _left.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                var search = CreateSearchBox();
                search.HorizontalOptions = LayoutOptions.Fill;
                search.MaximumWidthRequest = 420;
                _left.AddAt(search, 0, 0);
            }
            else
            {
                _left.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                var icon = new GlyphLabel
                {
                    Glyph = Icons.School,
                    FontSize = 22,
                    TextColor = ThemeColors.Get("PrimaryDark"),
                    VerticalTextAlignment = TextAlignment.Center,
                };
                var brand = new Label
                {
                    Text = AppText.Common.AppName,
                    FontSize = 15,
                    FontFamily = "OpenSansBold",
                    TextColor = ThemeColors.Get("PrimaryDark"),
                    VerticalTextAlignment = TextAlignment.Center,
                };
                var stack = new HorizontalStackLayout { Spacing = 8 };
                stack.Children.Add(icon);
                stack.Children.Add(brand);
                _left.AddAt(stack, 0, 0);
            }
        }

        private View CreateSearchBox()
        {
            var icon = new GlyphLabel
            {
                Glyph = Icons.Search,
                FontSize = 18,
                TextColor = ThemeColors.Get("TextMuted"),
                VerticalTextAlignment = TextAlignment.Center,
            };
            var entry = new Entry
            {
                Placeholder = AppText.Navigation.SearchPlaceholder,
                PlaceholderColor = ThemeColors.Get("TextMuted"),
                TextColor = ThemeColors.Get("Text"),
                FontSize = 14,
                BackgroundColor = Colors.Transparent,
            };

            var grid = new Grid { Padding = new Thickness(12, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.AddAt(icon, 0, 0);
            grid.AddAt(entry, 1, 0);

            var shell = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                StrokeThickness = 1,
                Stroke = ThemeColors.Get("Border"),
                BackgroundColor = ThemeColors.Get("Surface"),
                Content = grid,
                HeightRequest = 44,
                VerticalOptions = LayoutOptions.Center,
            };
            return shell;
        }

        private void RebuildRight()
        {
            _right.Children.Clear();
            _right.ColumnDefinitions.Clear();

            // Sino de notificaÃ§Ãµes com badge.
            var bell = new GlyphLabel
            {
                Glyph = Icons.NotificationsOutline,
                FontSize = 24,
                TextColor = ThemeColors.Get("Text"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                WidthRequest = 40,
                HeightRequest = 40,
            };
            var badge = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = ThemeColors.Get("Danger"),
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = new Thickness(4, 1),
                Content = new Label
                {
                    Text = _notificationCount > 9 ? "9+" : _notificationCount.ToString(),
                    FontSize = 10,
                    FontFamily = "OpenSansBold",
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                },
                WidthRequest = 18,
                HeightRequest = 18,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 2, 2, 0),
            };
            var bellHolder = new Grid { WidthRequest = 44, HeightRequest = 44 };
            bellHolder.Children.Add(bell);
            bellHolder.Children.Add(badge);

            _right.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _right.AddAt(bellHolder, 0, 0);

            // Avatar (com borda accent) + chevron (desktop).
            var avatarBorder = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                StrokeThickness = 2,
                Stroke = ThemeColors.Get("Accent"),
                Padding = new Thickness(1),
                Content = new AvatarView { Name = _auth.User?.Name, Size = 34 },
            };

            var profile = new PressableScale();
            var profileGrid = new Grid { ColumnSpacing = 6 };
            profileGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            profileGrid.AddAt(avatarBorder, 0, 0);
            if (_layout.IsDesktop)
            {
                profileGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                profileGrid.Add(new GlyphLabel
                {
                    Glyph = Icons.ChevronDown,
                    FontSize = 16,
                    TextColor = ThemeColors.Get("TextMuted"),
                    VerticalTextAlignment = TextAlignment.Center,
                }, 1, 0);
            }
            profile.Content = profileGrid;
            profile.Command = new RelayCommand(() => ProfileMenuRequested?.Invoke());

            _right.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _right.AddAt(profile, 1, 0);
        }

        private void ApplyTheme()
        {
            Dispatcher.Dispatch(() =>
            {
                BackgroundColor = ThemeColors.Get("Surface");
                RebuildLeft();
                RebuildRight();
            });
        }
    }
}