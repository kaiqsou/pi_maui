using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace LuminaAVA.Views;

public class TelaInicialAlunoPage : ThemedPage
{
    const string IconFont = "MaterialSymbolsOutlined";

    static Color SurfaceColor => LuminaTheme.Surface;
    static Color PrimaryColor => LuminaTheme.Primary;
    static Color OnSurfaceColor => LuminaTheme.OnSurface;
    static Color TextMuted => LuminaTheme.TextMuted;
    static Color TextFaint => LuminaTheme.TextFaint;
    static Color TextSoft => LuminaTheme.TextSoft;
    static Color TextVariant => LuminaTheme.TextVariant;
    static Color TrackColor => LuminaTheme.Track;
    static Color DividerColor => LuminaTheme.Divider;
    static Color CardStroke => LuminaTheme.CardStroke;
    static Color SuccessColor => LuminaTheme.Success;
    static Color AvatarBackground => LuminaTheme.AvatarBackground;

    public TelaInicialAlunoPage()
    {
        Title = "Início";
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        Content = BuildRoot();
    }

    protected override View BuildRoot()
    {
        BackgroundColor = SurfaceColor;

        var header = BuildHeader();
        var content = BuildContent();
        var bottomNav = LuminaNav.Build("inicio");

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

    static Border Card(View content, double cornerRadius = 16, Thickness? padding = null) => new Border
    {
        BackgroundColor = LuminaTheme.SurfaceCard,
        Stroke = CardStroke,
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = cornerRadius },
        Padding = padding ?? new Thickness(20),
        Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Color.FromArgb("#0A000000")),
            Offset = new Point(0, 2),
            Radius = 12,
            Opacity = 1,
        },
        Content = content,
    };

    View BuildHeader()
    {
        var brand = new HorizontalStackLayout
        {
            Spacing = 6,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Text("Lumina", 16, PrimaryColor, FontAttributes.Bold),
                Text("/", 12, TextFaint),
                Text("AVA", 12, TextMuted, FontAttributes.Bold),
            },
        };

        var notificationDot = new Border
        {
            BackgroundColor = PrimaryColor,
            StrokeShape = new RoundRectangle { CornerRadius = 3 },
            StrokeThickness = 0,
            WidthRequest = 6,
            HeightRequest = 6,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 7, 7, 0),
        };

        var notification = new Grid
        {
            WidthRequest = 36,
            HeightRequest = 36,
            Children =
            {
                Icon("notifications", 20, TextMuted),
                notificationDot,
            },
        };
        ((Image)notification.Children[0]).HorizontalOptions = LayoutOptions.Center;
        ((Image)notification.Children[0]).VerticalOptions = LayoutOptions.Center;

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
            Padding = new Thickness(20, 0),
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
                new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#0A000000") },
            },
        };
    }

    View BuildContent()
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 28,
            Padding = new Thickness(20, 24, 20, 32),
            Children =
            {
                BuildGreeting(),
                BuildContinueCard(),
                BuildSubjectsSection(),
            },
        };

        return new ScrollView
        {
            Content = stack,
            VerticalOptions = LayoutOptions.Fill,
        };
    }

    static View BuildGreeting() => new VerticalStackLayout
    {
        Spacing = 4,
        Children =
        {
            Text("Olá, Gabriel", 24, OnSurfaceColor, FontAttributes.Bold),
            Text("Engenharia de Software", 14, TextMuted),
        },
    };

    static View BuildContinueCard()
    {
        var headerRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children =
            {
                Text("CONTINUAR ESTUDANDO", 11, TextFaint, FontAttributes.Bold),
                Text("65%", 12, TextMuted),
            },
        };
        headerRow.SetColumn(headerRow.Children[0], 0);
        headerRow.SetColumn(headerRow.Children[1], 1);

        var titleBlock = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Text("Arquitetura de Microsserviços", 16, OnSurfaceColor, FontAttributes.Bold),
                Text("Aula 04 • Padrões e Comunicação Assíncrona", 12, TextMuted),
            },
        };

        var progress = new ProgressBar
        {
            Progress = 0.65,
            ProgressColor = PrimaryColor,
            BackgroundColor = TrackColor,
            HeightRequest = 4,
            VerticalOptions = LayoutOptions.Center,
        };

        var buttonLabel = Text("Continuar", 14, Colors.White, FontAttributes.Bold);
        var buttonIcon = Icon("arrow_forward", 18, Colors.White);

        var buttonContent = new HorizontalStackLayout
        {
            Spacing = 6,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children = { buttonLabel, buttonIcon },
        };

        var button = new Border
        {
            BackgroundColor = PrimaryColor,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            StrokeThickness = 0,
            HeightRequest = 44,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 4, 0, 0),
            Content = buttonContent,
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            buttonLabel.Text = "Abrindo...";
            buttonIcon.Source = IconSource("progress_activity", 18, Colors.White);
            await Task.Delay(900);
            buttonLabel.Text = "Continuar";
            buttonIcon.Source = IconSource("arrow_forward", 18, Colors.White);
        };
        button.GestureRecognizers.Add(tap);

        return Card(new VerticalStackLayout
        {
            Spacing = 16,
            Children = { headerRow, titleBlock, progress, button },
        });
    }

    static View BuildSubjectsSection()
    {
        var sectionHeader = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children =
            {
                Text("Minhas Matérias", 15, OnSurfaceColor, FontAttributes.Bold),
                Text("Semestre 4", 12, TextFaint),
            },
        };
        sectionHeader.SetColumn(sectionHeader.Children[0], 0);
        sectionHeader.SetColumn(sectionHeader.Children[1], 1);

        var list = new VerticalStackLayout();
        list.Children.Add(SubjectRow("Engenharia de Software", "Em dia", "84%", true));
        list.Children.Add(new BoxView { HeightRequest = 1, BackgroundColor = DividerColor });
        list.Children.Add(SubjectRow("Modelagem de Dados", "Em andamento", "62%", false));
        list.Children.Add(new BoxView { HeightRequest = 1, BackgroundColor = DividerColor });
        list.Children.Add(SubjectRow("Programação Orientada a Objetos", "Em andamento", "45%", false));

        return new VerticalStackLayout
        {
            Spacing = 12,
            Children = { sectionHeader, Card(list, padding: new Thickness(16, 6)) },
        };
    }

    static View SubjectRow(string name, string status, string percent, bool completed)
    {
        var nameLabel = Text(name, 14, OnSurfaceColor);
        nameLabel.LineBreakMode = LineBreakMode.TailTruncation;

        var info = new VerticalStackLayout
        {
            Spacing = 2,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                nameLabel,
                Text(status, 12, TextFaint),
            },
        };

        var trailingIcon = completed
            ? Icon("check_circle", 18, SuccessColor)
            : Icon("chevron_right", 18, Color.FromArgb("#D4D4D4"));

        var trailing = new HorizontalStackLayout
        {
            Spacing = 6,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Text(percent, 12, completed ? TextSoft : TextMuted,
                    completed ? FontAttributes.Bold : FontAttributes.None),
                trailingIcon,
            },
        };

        var row = new Grid
        {
            Padding = new Thickness(4, 12),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children = { info, trailing },
        };
        row.SetColumn(info, 0);
        row.SetColumn(trailing, 1);

        return row;
    }

}
