using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace LuminaAVA.Views;

public class TelaCertificadosPage : ThemedPage
{
    const string IconFont = "MaterialSymbolsOutlined";

    static Color SurfaceColor => LuminaTheme.Surface;
    static Color PrimaryColor => LuminaTheme.Primary;
    static Color OnPrimaryColor => Colors.White;
    static Color OnSurfaceColor => LuminaTheme.OnSurface;
    static Color TextVariant => LuminaTheme.TextVariant;
    static Color SurfaceContainer => LuminaTheme.SurfaceContainer;
    static Color SurfaceCard => LuminaTheme.SurfaceCard;

    readonly (string Glyph, string Titulo, string Emitido)[] _certificados =
    {
        ("database", "Banco de Dados Moderno", "Emitido em 18 de Outubro de 2024"),
        ("all_inclusive", "DevOps & CI/CD", "Emitido em 25 de Agosto de 2024"),
    };

    Entry? _codigoEntry;

    public TelaCertificadosPage()
    {
        Title = "Certificados";
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        Content = BuildRoot();
    }

    protected override View BuildRoot()
    {
        BackgroundColor = SurfaceColor;

        var header = BuildHeader();
        var content = BuildContent();
        var bottomNav = LuminaNav.Build("certificados");

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
                Text("Certificados", 12, TextVariant),
            },
        };

        var brand = new HorizontalStackLayout
        {
            Spacing = 8,
            VerticalOptions = LayoutOptions.Center,
            Children = { Icon("school", 22, PrimaryColor), brandText },
        };

        var notificationIcon = Icon("notifications", 22, TextVariant);
        notificationIcon.HorizontalOptions = LayoutOptions.Center;
        notificationIcon.VerticalOptions = LayoutOptions.Center;

        var dot = new Border
        {
            BackgroundColor = LuminaTheme.Error,
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
                new BoxView { HeightRequest = 1, BackgroundColor = LuminaTheme.HeaderStroke },
            },
        };
    }

    View BuildContent()
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 20,
            Padding = new Thickness(20, 16, 20, 28),
            Children =
            {
                BuildTitle(),
                BuildValidationField(),
                BuildCertificateList(),
            },
        };

        return new ScrollView
        {
            Content = stack,
            VerticalOptions = LayoutOptions.Fill,
        };
    }

    View BuildTitle()
    {
        return new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Text("Certificados", 26, OnSurfaceColor, FontAttributes.Bold),
                Text($"{_certificados.Length} certificados emitidos", 14, TextVariant),
            },
        };
    }

    View BuildValidationField()
    {
        _codigoEntry = new Entry
        {
            Placeholder = "Validar por código autenticador...",
            TextColor = OnSurfaceColor,
            PlaceholderColor = LuminaTheme.Placeholder,
            BackgroundColor = Colors.Transparent,
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center,
        };

        var validateLabel = Text("Validar", 13, PrimaryColor, FontAttributes.Bold);
        var validate = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Padding = new Thickness(8, 4),
            VerticalOptions = LayoutOptions.Center,
            Content = validateLabel,
        };

        var validateTap = new TapGestureRecognizer();
        validateTap.Tapped += async (_, _) =>
        {
            var codigo = _codigoEntry?.Text?.Trim();
            if (string.IsNullOrEmpty(codigo))
            {
                await DisplayAlertAsync("Validação de Certificado", "Informe o código autenticador para validar.", "OK");
                return;
            }

            await DisplayAlertAsync("Certificado Validado", $"O código {codigo} é válido e corresponde a um certificado emitido pela Lumina AVA.", "OK");
        };
        validate.GestureRecognizers.Add(validateTap);

        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children = { Icon("search", 20, LuminaTheme.Outline), _codigoEntry, validate },
        };
        row.SetColumn(row.Children[0], 0);
        row.SetColumn(_codigoEntry, 1);
        row.SetColumn(validate, 2);

        return new Border
        {
            BackgroundColor = SurfaceCard,
            Stroke = LuminaTheme.FieldStroke,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(12, 2),
            Content = row,
        };
    }

    View BuildCertificateList()
    {
        var list = new VerticalStackLayout { Spacing = 12 };

        foreach (var certificado in _certificados)
            list.Children.Add(BuildCertificateCard(certificado));

        return list;
    }

    View BuildCertificateCard((string Glyph, string Titulo, string Emitido) certificado)
    {
        var iconBox = new Border
        {
            BackgroundColor = SurfaceContainer,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            WidthRequest = 40,
            HeightRequest = 40,
            Padding = new Thickness(0),
            Content = Icon(certificado.Glyph, 20, PrimaryColor),
        };
        ((Image)iconBox.Content).HorizontalOptions = LayoutOptions.Center;
        ((Image)iconBox.Content).VerticalOptions = LayoutOptions.Center;

        var info = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Text(certificado.Titulo, 15, OnSurfaceColor, FontAttributes.Bold),
                Text(certificado.Emitido, 13, TextVariant),
            },
        };

        var top = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            Children = { iconBox, info },
        };
        top.SetColumn(iconBox, 0);
        top.SetColumn(info, 1);

        var actions = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
            Children =
            {
                BuildActionButton("visibility", "Visualizar", false, certificado.Titulo, true),
                BuildActionButton("download", "Baixar PDF", true, certificado.Titulo, false),
            },
        };
        actions.SetColumn(actions.Children[0], 0);
        actions.SetColumn(actions.Children[1], 1);

        return new Border
        {
            BackgroundColor = SurfaceCard,
            Stroke = LuminaTheme.CardStrokeStrong,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(16),
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children = { top, actions },
            },
        };
    }

    View BuildActionButton(string glyph, string label, bool primary, string titulo, bool visualizar)
    {
        var foreground = primary ? OnPrimaryColor : OnSurfaceColor;

        var button = new Border
        {
            BackgroundColor = primary ? PrimaryColor : SurfaceContainer,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            HeightRequest = 36,
            Padding = new Thickness(0),
            Content = new HorizontalStackLayout
            {
                Spacing = 6,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = { Icon(glyph, 18, foreground), Text(label, 13, foreground, FontAttributes.Bold) },
            },
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            if (visualizar)
                await DisplayAlertAsync("Certificado", $"Abrindo a visualização do certificado “{titulo}”.", "OK");
            else
                await DisplayAlertAsync("Download", $"O PDF do certificado “{titulo}” será salvo no seu dispositivo.", "OK");
        };
        button.GestureRecognizers.Add(tap);

        return button;
    }
}
