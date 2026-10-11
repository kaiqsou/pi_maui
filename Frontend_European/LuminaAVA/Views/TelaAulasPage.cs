using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace LuminaAVA.Views;

public class TelaAulasPage : ThemedPage
{
    const string IconFont = "MaterialSymbolsOutlined";

    const string ThumbnailUrl =
        "https://lh3.googleusercontent.com/aida-public/AB6AXuA_u3bulgbc0dC3THllXGbPR74JPwUu4sioguuX9KdDkSDKks9vw8f7w1uQWrcwkevCiRo8cEufRVPyUyrH5_r87vsnKERKxoE-QBh7Ju4AeEz3_pjyKtFaODPyF2IdM39V5OyRjJfwzTJYUaDHQlZeGBnXWuzvVx8eWNaWrBq2TttBsLTeeHG3owg-0qUOPFv6zRrxgNutgtkSL626ZHwCE-shEOyJxBMnNIx8r08";

    static Color SurfaceColor => LuminaTheme.Surface;
    static Color PrimaryColor => LuminaTheme.Primary;
    static Color OnSurfaceColor => LuminaTheme.OnSurface;
    static Color TextVariant => LuminaTheme.TextVariant;
    static Color TextMuted => LuminaTheme.TextMuted;
    static Color TextFaint => LuminaTheme.TextFaint;
    static Color OutlineColor => LuminaTheme.Outline;
    static Color SecondaryColor => LuminaTheme.Secondary;
    static Color SurfaceContainer => LuminaTheme.SurfaceContainer;
    static Color DividerColor => LuminaTheme.DividerSoft;
    static Color AvatarBackground => LuminaTheme.AvatarBackground;
    static Color CardStroke => LuminaTheme.CardStroke;

    readonly string[] _speeds = { "1.0x", "1.25x", "1.5x", "2.0x" };

    Image? _centerPlayIcon;
    Image? _miniPlayIcon;
    Label? _speedLabel;
    bool _isPlaying;
    int _speedIndex = 2;

    VerticalStackLayout? _lessonsTab;
    VerticalStackLayout? _materialsTab;
    Label? _lessonsLabel;
    Label? _materialsLabel;
    BoxView? _lessonsUnderline;
    BoxView? _materialsUnderline;

    public TelaAulasPage()
    {
        Title = "Aulas";
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        Content = BuildRoot();
    }

    protected override View BuildRoot()
    {
        BackgroundColor = SurfaceColor;

        var header = BuildHeader();
        var content = BuildContent();
        var bottomNav = LuminaNav.Build("aulas");

        var root = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto },
            },
        };

        root.SetRow(header, 0);
        root.SetRow(content, 1);
        root.SetRow(bottomNav, 2);

        root.Add(header);
        root.Add(content);
        root.Add(bottomNav);

        return root;
    }

    static ImageSource IconSource(string glyph, double size, Color color) => new FontImageSource
    {
        FontFamily = IconFont,
        Glyph = glyph,
        Size = size,
        Color = color,
    };

    static Image Icon(string glyph, double size, Color color) => new Image
    {
        Source = IconSource(glyph, size, color),
        WidthRequest = size,
        HeightRequest = size,
        VerticalOptions = LayoutOptions.Center,
    };

    static Label Text(string value, double size, Color color, FontAttributes attributes = FontAttributes.None) => new Label
    {
        Text = value,
        FontSize = size,
        TextColor = color,
        FontAttributes = attributes,
        VerticalOptions = LayoutOptions.Center,
    };

    View BuildHeader()
    {
        var brand = new HorizontalStackLayout
        {
            Spacing = 8,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Icon("school", 22, PrimaryColor),
                Text("Lumina AVA", 16, OnSurfaceColor, FontAttributes.Bold),
            },
        };

        var notification = new Grid
        {
            WidthRequest = 36,
            HeightRequest = 36,
            Children = { Icon("notifications", 20, TextVariant) },
        };
        ((Image)notification.Children[0]).HorizontalOptions = LayoutOptions.Center;

        var avatar = LuminaAvatar.Build();

        var actions = new HorizontalStackLayout
        {
            Spacing = 8,
            VerticalOptions = LayoutOptions.Center,
            Children = { notification, avatar },
        };

        var bar = new Grid
        {
            HeightRequest = 56,
            Padding = new Thickness(16, 0),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children = { brand, actions },
        };
        bar.SetColumn(brand, 0);
        bar.SetColumn(actions, 1);

        return new Grid
        {
            BackgroundColor = SurfaceColor,
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            Children =
            {
                bar,
                new BoxView { HeightRequest = 1, BackgroundColor = LuminaTheme.DividerSoft },
            },
        };
    }

    View BuildContent()
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 0,
            Children =
            {
                BuildVideoPlayer(),
                BuildLessonHeader(),
                BuildTabSwitcher(),
            },
        };

        return new ScrollView
        {
            Content = stack,
            VerticalOptions = LayoutOptions.Fill,
        };
    }

    View BuildVideoPlayer()
    {
        var thumbnail = new Image
        {
            Source = ImageSource.FromUri(new Uri(ThumbnailUrl)),
            Aspect = Aspect.AspectFill,
            Opacity = 0.8,
        };

        var overlay = new BoxView { Color = Color.FromArgb("#33000000") };

        _centerPlayIcon = Icon("play_arrow", 26, OnSurfaceColor);

        var playButton = new Border
        {
            BackgroundColor = Color.FromArgb("#E6FFFFFF"),
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            StrokeThickness = 0,
            WidthRequest = 48,
            HeightRequest = 48,
            Padding = new Thickness(0),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = _centerPlayIcon,
        };
        ((Image)playButton.Content).HorizontalOptions = LayoutOptions.Center;

        var playTap = new TapGestureRecognizer();
        playTap.Tapped += (_, _) => TogglePlay();
        playButton.GestureRecognizers.Add(playTap);

        var bottomBar = BuildPlayerBar();

        var player = new Grid
        {
            HeightRequest = 220,
            BackgroundColor = LuminaTheme.PlayerBackground,
            Children = { thumbnail, overlay, playButton, bottomBar },
        };
        thumbnail.HorizontalOptions = LayoutOptions.Fill;
        thumbnail.VerticalOptions = LayoutOptions.Fill;
        overlay.HorizontalOptions = LayoutOptions.Fill;
        overlay.VerticalOptions = LayoutOptions.Fill;
        bottomBar.VerticalOptions = LayoutOptions.End;

        player.SizeChanged += (_, _) =>
        {
            if (player.Width <= 0)
                return;

            var target = player.Width * 9.0 / 16.0;
            if (Math.Abs(player.HeightRequest - target) > 0.5)
                player.HeightRequest = target;
        };

        return player;
    }

    View BuildPlayerBar()
    {
        var trackFill = new Border
        {
            BackgroundColor = PrimaryColor,
            StrokeShape = new RoundRectangle { CornerRadius = 2 },
            StrokeThickness = 0,
            HeightRequest = 3,
            VerticalOptions = LayoutOptions.Center,
        };

        var progressGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(new GridLength(0.58, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(0.42, GridUnitType.Star)),
            },
            Children = { trackFill },
        };
        progressGrid.SetColumn(trackFill, 0);

        var timeline = new Border
        {
            BackgroundColor = Color.FromArgb("#4DFFFFFF"),
            StrokeShape = new RoundRectangle { CornerRadius = 2 },
            StrokeThickness = 0,
            HeightRequest = 3,
            Padding = new Thickness(0),
            Content = progressGrid,
        };

        _miniPlayIcon = Icon("play_arrow", 18, Colors.White);
        var miniPlay = new Grid { WidthRequest = 24, HeightRequest = 24, Children = { _miniPlayIcon } };
        ((Image)miniPlay.Children[0]).HorizontalOptions = LayoutOptions.Center;
        ((Image)miniPlay.Children[0]).VerticalOptions = LayoutOptions.Center;

        var miniTap = new TapGestureRecognizer();
        miniTap.Tapped += (_, _) => TogglePlay();
        miniPlay.GestureRecognizers.Add(miniTap);

        var timeLabel = Text("18:42 / 32:15", 11, Colors.White);
        timeLabel.FontAttributes = FontAttributes.Bold;

        var left = new HorizontalStackLayout
        {
            Spacing = 8,
            VerticalOptions = LayoutOptions.Center,
            Children = { miniPlay, timeLabel },
        };

        _speedLabel = Text(_speeds[_speedIndex], 11, Colors.White);
        _speedLabel.FontAttributes = FontAttributes.Bold;

        var speedTap = new TapGestureRecognizer();
        speedTap.Tapped += (_, _) =>
        {
            _speedIndex = (_speedIndex + 1) % _speeds.Length;
            if (_speedLabel is not null)
                _speedLabel.Text = _speeds[_speedIndex];
        };
        _speedLabel.GestureRecognizers.Add(speedTap);

        var fullscreen = Icon("fullscreen", 18, Colors.White);

        var right = new HorizontalStackLayout
        {
            Spacing = 12,
            VerticalOptions = LayoutOptions.Center,
            Children = { _speedLabel, fullscreen },
        };

        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children = { left, right },
        };
        row.SetColumn(left, 0);
        row.SetColumn(right, 1);

        return new Grid
        {
            BackgroundColor = Color.FromArgb("#B3000000"),
            Padding = new Thickness(16, 14, 16, 10),
            RowSpacing = 8,
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            Children = { timeline, row },
        };
    }

    void TogglePlay()
    {
        _isPlaying = !_isPlaying;
        var glyph = _isPlaying ? "pause" : "play_arrow";

        if (_centerPlayIcon is not null)
            _centerPlayIcon.Source = IconSource(glyph, 26, OnSurfaceColor);
        if (_miniPlayIcon is not null)
            _miniPlayIcon.Source = IconSource(glyph, 18, Colors.White);
    }

    View BuildLessonHeader()
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 4,
            Padding = new Thickness(16, 16, 16, 12),
            Children =
            {
                Text("Aula 04: Microsserviços e Event-Driven", 20, OnSurfaceColor, FontAttributes.Bold),
                Text("Prof. Carlos Eduardo Mendes", 13, TextVariant),
            },
        };

        var container = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            Children =
            {
                stack,
                new BoxView { HeightRequest = 1, BackgroundColor = DividerColor },
            },
        };
        container.SetRow(stack, 0);
        container.SetRow(container.Children[1], 1);

        return container;
    }

    View BuildTabSwitcher()
    {
        _lessonsLabel = Text("Aulas", 15, PrimaryColor, FontAttributes.Bold);
        _materialsLabel = Text("Materiais", 15, TextVariant);

        _lessonsUnderline = new BoxView { HeightRequest = 2, BackgroundColor = PrimaryColor };
        _materialsUnderline = new BoxView { HeightRequest = 2, BackgroundColor = Colors.Transparent, IsVisible = false };

        var lessonsButton = new VerticalStackLayout
        {
            Spacing = 8,
            Children = { _lessonsLabel, _lessonsUnderline },
        };

        var materialsButton = new VerticalStackLayout
        {
            Spacing = 8,
            Children = { _materialsLabel, _materialsUnderline },
        };

        var lessonsTap = new TapGestureRecognizer();
        lessonsTap.Tapped += (_, _) => SelectTab(true);
        lessonsButton.GestureRecognizers.Add(lessonsTap);

        var materialsTap = new TapGestureRecognizer();
        materialsTap.Tapped += (_, _) => SelectTab(false);
        materialsButton.GestureRecognizers.Add(materialsTap);

        var tabs = new HorizontalStackLayout
        {
            Spacing = 24,
            Padding = new Thickness(16, 0, 16, 0),
            Children = { lessonsButton, materialsButton },
        };

        var container = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            Children =
            {
                tabs,
                new BoxView { HeightRequest = 1, BackgroundColor = DividerColor },
                BuildTabContent(),
            },
        };
        container.SetRow(tabs, 0);
        container.SetRow(container.Children[1], 1);
        container.SetRow(container.Children[2], 2);

        return container;
    }

    View BuildTabContent()
    {
        _lessonsTab = new VerticalStackLayout { Spacing = 0, Children = { BuildLessonsList() } };
        _materialsTab = new VerticalStackLayout { Spacing = 0, IsVisible = false, Children = { BuildMaterialsList() } };

        return new Grid { Children = { _lessonsTab, _materialsTab } };
    }

    void SelectTab(bool lessons)
    {
        if (_lessonsTab is null || _materialsTab is null)
            return;

        _lessonsTab.IsVisible = lessons;
        _materialsTab.IsVisible = !lessons;

        if (_lessonsLabel is not null)
        {
            _lessonsLabel.TextColor = lessons ? PrimaryColor : TextVariant;
            _lessonsLabel.FontAttributes = lessons ? FontAttributes.Bold : FontAttributes.None;
        }

        if (_materialsLabel is not null)
        {
            _materialsLabel.TextColor = lessons ? TextVariant : PrimaryColor;
            _materialsLabel.FontAttributes = lessons ? FontAttributes.None : FontAttributes.Bold;
        }

        if (_lessonsUnderline is not null)
            _lessonsUnderline.IsVisible = lessons;
        if (_materialsUnderline is not null)
            _materialsUnderline.IsVisible = !lessons;
    }

    View BuildLessonsList()
    {
        var list = new VerticalStackLayout { Spacing = 0 };

        list.Children.Add(LessonRow("check_circle", SecondaryColor, "Aula 01: Introdução a Arquiteturas Distribuídas",
            "24 min", TextVariant, false, null, TextVariant, false, 1));
        list.Children.Add(Divider());

        list.Children.Add(LessonRow("check_circle", SecondaryColor, "Aula 02: Monólitos vs. Microsserviços",
            "19 min", TextVariant, false, null, TextVariant, false, 1));
        list.Children.Add(Divider());

        list.Children.Add(LessonRow("check_circle", SecondaryColor, "Aula 03: Comunicação Síncrona e APIs",
            "28 min", TextVariant, false, null, TextVariant, false, 1));
        list.Children.Add(Divider());

        list.Children.Add(LessonRow("play_circle", PrimaryColor, "Aula 04: Microsserviços e Event-Driven",
            "32 min", PrimaryColor, true, "Em reprodução", PrimaryColor, true, 1));
        list.Children.Add(Divider());

        list.Children.Add(LessonRow("radio_button_unchecked", OutlineColor, "Aula 05: Mensageria com RabbitMQ",
            "26 min", OnSurfaceColor, false, null, TextVariant, false, 0.75));
        list.Children.Add(Divider());

        list.Children.Add(LessonRow("lock", OutlineColor, "Aula 06: Tratamento de Falhas e Resiliência",
            "22 min", TextVariant, false, null, TextVariant, false, 0.6));

        return list;
    }

    static View LessonRow(string glyph, Color iconColor, string title, string duration, Color titleColor,
        bool bold, string? subtitle, Color durationColor, bool active, double opacity)
    {
        var icon = Icon(glyph, 18, iconColor);

        var info = new VerticalStackLayout
        {
            Spacing = 2,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            Children = { Text(title, 14, titleColor, bold ? FontAttributes.Bold : FontAttributes.None) },
        };
        ((Label)info.Children[0]).LineBreakMode = LineBreakMode.TailTruncation;
        ((Label)info.Children[0]).HorizontalOptions = LayoutOptions.Fill;

        if (!string.IsNullOrEmpty(subtitle))
        {
            var sub = Text(subtitle!, 11, Color.FromArgb("#B33525CD"));
            info.Children.Add(sub);
        }

        var leading = new Grid
        {
            ColumnSpacing = 12,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            Children = { icon, info },
        };
        leading.SetColumn(icon, 0);
        leading.SetColumn(info, 1);

        var durationLabel = Text(duration, 13, durationColor, bold ? FontAttributes.Bold : FontAttributes.None);

        var content = new Grid
        {
            Padding = new Thickness(16, 14),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children = { leading, durationLabel },
        };
        content.SetColumn(leading, 0);
        content.SetColumn(durationLabel, 1);

        var bar = new BoxView
        {
            WidthRequest = 2,
            BackgroundColor = active ? PrimaryColor : Colors.Transparent,
        };

        var row = new Grid
        {
            BackgroundColor = active ? SurfaceContainer : Colors.Transparent,
            Opacity = opacity,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            Children = { bar, content },
        };
        row.SetColumn(bar, 0);
        row.SetColumn(content, 1);

        return row;
    }

    View BuildMaterialsList()
    {
        var list = new VerticalStackLayout { Spacing = 0 };

        list.Children.Add(MaterialRow("description", "Slides da Aula.pdf", "4.2 MB", "download"));
        list.Children.Add(Divider());
        list.Children.Add(MaterialRow("folder_zip", "Código do Exemplo (RabbitMQ).zip", "1.8 MB", "download"));
        list.Children.Add(Divider());
        list.Children.Add(MaterialRow("link", "Artigo: Arquiteturas Orientadas a Eventos", "Leitura complementar", "open_in_new"));

        return list;
    }

    static View MaterialRow(string glyph, string title, string subtitle, string actionGlyph)
    {
        var icon = Icon(glyph, 20, TextVariant);

        var info = new VerticalStackLayout
        {
            Spacing = 2,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Text(title, 14, OnSurfaceColor),
                Text(subtitle, 13, TextVariant),
            },
        };
        ((Label)info.Children[0]).LineBreakMode = LineBreakMode.TailTruncation;
        ((Label)info.Children[0]).HorizontalOptions = LayoutOptions.Fill;

        var leading = new Grid
        {
            ColumnSpacing = 12,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            Children = { icon, info },
        };
        leading.SetColumn(icon, 0);
        leading.SetColumn(info, 1);

        var action = Icon(actionGlyph, 18, TextVariant);

        var row = new Grid
        {
            Padding = new Thickness(16, 14),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children = { leading, action },
        };
        row.SetColumn(leading, 0);
        row.SetColumn(action, 1);

        var tap = new TapGestureRecognizer();
        row.GestureRecognizers.Add(tap);

        return row;
    }

    static BoxView Divider() => new BoxView { HeightRequest = 1, BackgroundColor = LuminaTheme.DividerSoft };
}
