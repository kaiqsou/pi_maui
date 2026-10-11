using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace LuminaAVA.Views;

public class TelaDesempenhoPage : ThemedPage
{
    const string IconFont = "MaterialSymbolsOutlined";

    static Color SurfaceColor => LuminaTheme.Surface;
    static Color PrimaryColor => LuminaTheme.Primary;
    static Color OnSurfaceColor => LuminaTheme.OnSurface;
    static Color TextVariant => LuminaTheme.TextVariant;
    static Color OutlineColor => LuminaTheme.Outline;
    static Color CardStroke => LuminaTheme.CardStroke;
    static Color HeaderStroke => LuminaTheme.HeaderStroke;

    public TelaDesempenhoPage()
    {
        Title = "Desempenho";
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        Content = BuildRoot();
    }

    protected override View BuildRoot()
    {
        BackgroundColor = SurfaceColor;

        var header = BuildHeader();
        var content = BuildContent();
        var bottomNav = LuminaNav.Build("desempenho");

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
        var brandText = new VerticalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Text("Lumina AVA", 16, PrimaryColor, FontAttributes.Bold),
                Text("Desempenho", 12, TextVariant),
            },
        };

        var brand = new HorizontalStackLayout
        {
            Spacing = 8,
            VerticalOptions = LayoutOptions.Center,
            Children = { Icon("bar_chart", 22, PrimaryColor), brandText },
        };

        var notificationIcon = Icon("notifications", 22, TextVariant);
        notificationIcon.HorizontalOptions = LayoutOptions.Center;
        notificationIcon.VerticalOptions = LayoutOptions.Center;

        var dot = new Border
        {
            BackgroundColor = PrimaryColor,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 4 },
            WidthRequest = 8,
            HeightRequest = 8,
            Margin = new Thickness(0, 8, 8, 0),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
        };

        var notification = new Grid
        {
            WidthRequest = 40,
            HeightRequest = 40,
            Children = { notificationIcon, dot },
        };

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
        var stack = new VerticalStackLayout
        {
            Spacing = 24,
            Padding = new Thickness(20, 20, 20, 28),
            Children =
            {
                BuildGpaCard(),
                BuildSubjectsSection(),
                BuildTranscriptAction(),
            },
        };

        return new ScrollView
        {
            Content = stack,
            VerticalOptions = LayoutOptions.Fill,
        };
    }

    static View BuildGpaCard()
    {
        var valueRow = new HorizontalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                Text("9.2", 52, OnSurfaceColor, FontAttributes.Bold),
                new Label
                {
                    Text = "/ 10",
                    FontSize = 16,
                    TextColor = TextVariant,
                    VerticalOptions = LayoutOptions.End,
                    Margin = new Thickness(0, 0, 0, 10),
                },
            },
        };

        return new Border
        {
            BackgroundColor = LuminaTheme.SurfaceCard,
            Stroke = CardStroke,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(24, 32),
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#05131B2E")),
                Offset = new Point(0, 2),
                Radius = 12,
                Opacity = 1,
            },
            Content = new VerticalStackLayout
            {
                Spacing = 4,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    Text("MÉDIA GERAL DO CURSO", 11, TextVariant, FontAttributes.Bold),
                    valueRow,
                    Text($"{AlunoData.SemestreTexto} • Rendimento Regular Concluído", 13, TextVariant),
                },
            },
        };
    }

    View BuildSubjectsSection()
    {
        var header = new Grid
        {
            Padding = new Thickness(4, 0),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children =
            {
                Text("Disciplinas Ativas", 16, OnSurfaceColor, FontAttributes.Bold),
                Text($"{AlunoData.Disciplinas.Length} matérias", 13, TextVariant),
            },
        };
        header.SetColumn(header.Children[0], 0);
        header.SetColumn(header.Children[1], 1);

        var list = new VerticalStackLayout { Spacing = 10 };

        foreach (var disciplina in AlunoData.Disciplinas)
            list.Children.Add(BuildSubjectRow(disciplina));

        return new VerticalStackLayout
        {
            Spacing = 12,
            Children = { header, list },
        };
    }

    static View BuildSubjectRow(Disciplina disciplina)
    {
        var info = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = disciplina.Nome,
                    FontSize = 15,
                    TextColor = OnSurfaceColor,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    MaxLines = 1,
                },
                Text($"Presença: {disciplina.Presenca}%", 13, TextVariant),
            },
        };

        var grade = new VerticalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = disciplina.Media.ToString("0.0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")),
                    FontSize = 17,
                    TextColor = OnSurfaceColor,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.End,
                },
                new Label
                {
                    Text = "média",
                    FontSize = 11,
                    TextColor = OutlineColor,
                    HorizontalTextAlignment = TextAlignment.End,
                },
            },
        };

        var row = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children = { info, grade },
        };
        row.SetColumn(info, 0);
        row.SetColumn(grade, 1);

        return new Border
        {
            BackgroundColor = LuminaTheme.SurfaceCard,
            Stroke = CardStroke,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(16),
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#05131B2E")),
                Offset = new Point(0, 1),
                Radius = 4,
                Opacity = 1,
            },
            Content = row,
        };
    }

    View BuildTranscriptAction()
    {
        var buttonContent = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Icon("download", 18, TextVariant),
                Text("Baixar Histórico em PDF", 14, OnSurfaceColor, FontAttributes.Bold),
            },
        };

        var button = new Border
        {
            BackgroundColor = LuminaTheme.SurfaceCard,
            Stroke = HeaderStroke,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(0, 12),
            Content = buttonContent,
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await DisplayAlertAsync(
            "Histórico Acadêmico",
            "Seu histórico consolidado será gerado em PDF com todas as notas do semestre.",
            "OK");
        button.GestureRecognizers.Add(tap);

        var caption = Text("Atualizado automaticamente após cada fechamento de nota", 11, TextVariant);
        caption.HorizontalTextAlignment = TextAlignment.Center;

        return new VerticalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(0, 4, 0, 0),
            Children = { button, caption },
        };
    }
}
