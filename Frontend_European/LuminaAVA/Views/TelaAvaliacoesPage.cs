using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace LuminaAVA.Views;

public class TelaAvaliacoesPage : ThemedPage
{
    const string IconFont = "MaterialSymbolsOutlined";

    static Color SurfaceColor => LuminaTheme.Surface;
    static Color PrimaryColor => LuminaTheme.Primary;
    static Color PrimaryContainer => LuminaTheme.PrimaryContainer;
    static Color OnSurfaceColor => LuminaTheme.OnSurface;
    static Color TextVariant => LuminaTheme.TextVariant;
    static Color OutlineColor => LuminaTheme.Outline;
    static Color SecondaryColor => LuminaTheme.Secondary;
    static Color BadgeBackground => LuminaTheme.BadgePrimaryBg;
    static Color GradeBackground => LuminaTheme.BadgeGradeBg;
    static Color CardStroke => LuminaTheme.CardStrokeStrong;
    static Color HeaderStroke => LuminaTheme.HeaderStrokeSoft;
    static Color RowDivider => LuminaTheme.RowDivider;
    static Color AvatarBackground => LuminaTheme.AvatarBackground;

    public TelaAvaliacoesPage()
    {
        Title = "Provas";
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        Content = BuildRoot();
    }

    protected override View BuildRoot()
    {
        BackgroundColor = SurfaceColor;

        var header = BuildHeader();
        var content = BuildContent();
        var bottomNav = LuminaNav.Build("provas");

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
                Text("Lumina AVA", 16, PrimaryColor, FontAttributes.Bold),
            },
        };

        var notification = new Grid
        {
            WidthRequest = 40,
            HeightRequest = 40,
            Children = { Icon("notifications", 22, TextVariant) },
        };
        ((Image)notification.Children[0]).HorizontalOptions = LayoutOptions.Center;

        var avatar = LuminaAvatar.Build();

        var actions = new HorizontalStackLayout
        {
            Spacing = 4,
            VerticalOptions = LayoutOptions.Center,
            Children = { notification, avatar },
        };

        var bar = new Grid
        {
            HeightRequest = 64,
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
                new BoxView { HeightRequest = 1, BackgroundColor = HeaderStroke },
            },
        };
    }

    View BuildContent()
    {
        var titleBlock = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Text("Avaliações", 26, OnSurfaceColor, FontAttributes.Bold),
                Text("1 prova pendente", 14, TextVariant),
            },
        };

        var stack = new VerticalStackLayout
        {
            Spacing = 24,
            Padding = new Thickness(20, 16, 20, 28),
            Children =
            {
                titleBlock,
                BuildPendingCard(),
                BuildCompletedSection(),
            },
        };

        return new ScrollView
        {
            Content = stack,
            VerticalOptions = LayoutOptions.Fill,
        };
    }

    View BuildPendingCard()
    {
        var badge = new Border
        {
            BackgroundColor = BadgeBackground,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            StrokeThickness = 0,
            Padding = new Thickness(10, 3),
            HorizontalOptions = LayoutOptions.Start,
            Content = Text("Pendente", 12, PrimaryColor, FontAttributes.Bold),
        };

        var headerRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children =
            {
                badge,
                Text("60 min", 13, TextVariant),
            },
        };
        headerRow.SetColumn(badge, 0);
        headerRow.SetColumn(headerRow.Children[1], 1);

        var deadline = new HorizontalStackLayout
        {
            Spacing = 6,
            Children =
            {
                Icon("event", 16, OutlineColor),
                Text("Até 25 de Outubro, 23:59", 13, TextVariant),
            },
        };

        var titleBlock = new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                Text("Prova Bimestral: Banco de Dados NoSQL", 18, OnSurfaceColor, FontAttributes.Bold),
                deadline,
            },
        };

        var button = new Border
        {
            BackgroundColor = PrimaryColor,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            StrokeThickness = 0,
            HeightRequest = 44,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 8, 0, 0),
            Content = new HorizontalStackLayout
            {
                Spacing = 6,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    Text("Iniciar Prova", 14, Colors.White, FontAttributes.Bold),
                    Icon("arrow_forward", 18, Colors.White),
                },
            },
        };

        return new Border
        {
            BackgroundColor = LuminaTheme.SurfaceCard,
            Stroke = CardStroke,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(20),
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#08131B2E")),
                Offset = new Point(0, 2),
                Radius = 12,
                Opacity = 1,
            },
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children = { headerRow, titleBlock, button },
            },
        };
    }

    View BuildCompletedSection()
    {
        var list = new VerticalStackLayout { Spacing = 0 };

        list.Children.Add(CompletedRow("Estrutura de Dados e Algoritmos", "14 de Outubro", "9.5"));
        list.Children.Add(new BoxView { HeightRequest = 1, BackgroundColor = RowDivider });
        list.Children.Add(CompletedRow("Engenharia de Software", "28 de Setembro", "8.8"));
        list.Children.Add(new BoxView { HeightRequest = 1, BackgroundColor = RowDivider });
        list.Children.Add(CompletedRow("Redes de Computadores", "12 de Setembro", "9.0"));

        var card = new Border
        {
            BackgroundColor = LuminaTheme.SurfaceCard,
            Stroke = CardStroke,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(0),
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#05131B2E")),
                Offset = new Point(0, 2),
                Radius = 12,
                Opacity = 1,
            },
            Content = list,
        };

        return new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                Text("Concluídas recentemente", 15, OnSurfaceColor, FontAttributes.Bold),
                card,
            },
        };
    }

    static View CompletedRow(string title, string date, string grade)
    {
        var nameLabel = Text(title, 14, OnSurfaceColor, FontAttributes.Bold);
        nameLabel.LineBreakMode = LineBreakMode.TailTruncation;

        var info = new VerticalStackLayout
        {
            Spacing = 2,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                nameLabel,
                Text(date, 13, TextVariant),
            },
        };

        var gradeBadge = new Border
        {
            BackgroundColor = GradeBackground,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            StrokeThickness = 0,
            Padding = new Thickness(10, 4),
            VerticalOptions = LayoutOptions.Center,
            Content = Text(grade, 14, SecondaryColor, FontAttributes.Bold),
        };

        var row = new Grid
        {
            Padding = new Thickness(16),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children = { info, gradeBadge },
        };
        row.SetColumn(info, 0);
        row.SetColumn(gradeBadge, 1);

        return row;
    }
}
