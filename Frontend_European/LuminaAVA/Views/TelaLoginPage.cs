using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace LuminaAVA.Views;

public class TelaLoginPage : ThemedPage
{
    const string IconFont = "MaterialSymbolsOutlined";

    static Color SurfaceColor => LuminaTheme.Surface;
    static Color PrimaryContainer => LuminaTheme.PrimaryContainer;
    static Color OnSurfaceColor => LuminaTheme.OnSurface;
    static Color FieldBackground => LuminaTheme.FieldBackground;
    static Color FieldStroke => LuminaTheme.FieldStroke;
    static Color LabelColor => LuminaTheme.LabelStrong;
    static Color PlaceholderColor => LuminaTheme.Placeholder;
    static Color MutedColor => LuminaTheme.TextMuted;
    static Color SubtleText => LuminaTheme.SubtleText;
    static Color FaintText => LuminaTheme.TextFaint;
    static Color CardStroke => LuminaTheme.CardStroke;
    static Color SecondaryStroke => LuminaTheme.SecondaryStroke;

    Entry? _passwordEntry;
    Image? _passwordToggleIcon;
    bool _passwordVisible;

    public TelaLoginPage()
    {
        Title = "Acesso";
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        Content = BuildRoot();
    }

    protected override View BuildRoot()
    {
        BackgroundColor = SurfaceColor;

        var main = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto },
            },
            Padding = new Thickness(20, 0),
        };

        var body = BuildBody();
        var footer = BuildFooter();

        main.SetRow(body, 0);
        main.SetRow(footer, 1);

        main.Add(body);
        main.Add(footer);

        return main;
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

    View BuildBody()
    {
        var logo = new Border
        {
            BackgroundColor = LuminaTheme.SurfaceCard,
            Stroke = CardStroke,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            WidthRequest = 48,
            HeightRequest = 48,
            Padding = new Thickness(0),
            HorizontalOptions = LayoutOptions.Center,
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#08000000")),
                Offset = new Point(0, 2),
                Radius = 10,
                Opacity = 1,
            },
            Content = Icon("school", 26, PrimaryContainer),
        };
        ((Image)logo.Content).HorizontalOptions = LayoutOptions.Center;

        var brand = new VerticalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                logo,
                new Label
                {
                    Text = "Lumina AVA",
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = OnSurfaceColor,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 0),
                },
                new Label
                {
                    Text = "Portal do Aluno",
                    FontSize = 12,
                    TextColor = MutedColor,
                    HorizontalTextAlignment = TextAlignment.Center,
                },
            },
        };

        var card = BuildCard();
        card.Margin = new Thickness(0, 28, 0, 0);

        return new VerticalStackLayout
        {
            Spacing = 0,
            Padding = new Thickness(0, 32, 0, 8),
            Children = { brand, card },
        };
    }

    View BuildCard()
    {
        var identifier = BuildInput("Ex: 2024.1.0892 ou aluno@lumina.edu.br", false);

        _passwordEntry = new Entry
        {
            Placeholder = "••••••••••••",
            PlaceholderColor = PlaceholderColor,
            TextColor = OnSurfaceColor,
            FontSize = 14,
            BackgroundColor = Colors.Transparent,
            IsPassword = true,
            VerticalOptions = LayoutOptions.Center,
        };

        _passwordToggleIcon = Icon("visibility", 19, PlaceholderColor);
        var toggleTap = new TapGestureRecognizer();
        toggleTap.Tapped += (_, _) => TogglePassword();
        _passwordToggleIcon.GestureRecognizers.Add(toggleTap);

        var passwordField = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children = { _passwordEntry, _passwordToggleIcon },
        };
        passwordField.SetColumn(_passwordEntry, 0);
        passwordField.SetColumn(_passwordToggleIcon, 1);
        _passwordToggleIcon.Margin = new Thickness(0, 0, 6, 0);

        var passwordInput = InputBorder(passwordField);

        var forgot = Text("Esqueci minha senha", 12, PrimaryContainer, FontAttributes.Bold);

        var passwordHeader = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children =
            {
                Text("Senha", 12, LabelColor, FontAttributes.Bold),
                forgot,
            },
        };
        passwordHeader.SetColumn(passwordHeader.Children[0], 0);
        passwordHeader.SetColumn(forgot, 1);

        var remember = new HorizontalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new CheckBox { IsChecked = true, Color = PrimaryContainer },
                Text("Lembrar no dispositivo", 12, SubtleText),
            },
        };

        var primaryButton = PrimaryButton();

        var biometric = new Border
        {
            BackgroundColor = Colors.Transparent,
            Stroke = SecondaryStroke,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            HeightRequest = 40,
            Padding = new Thickness(0),
            Content = new HorizontalStackLayout
            {
                Spacing = 6,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    Icon("fingerprint", 18, MutedColor),
                    Text("Acessar com Biometria", 12, LabelColor, FontAttributes.Bold),
                },
            },
        };
        var biometricTap = new TapGestureRecognizer();
        biometricTap.Tapped += async (_, _) => await NavigateHome();
        biometric.GestureRecognizers.Add(biometricTap);

        primaryButton.Margin = new Thickness(0, 4, 0, 0);

        var registerLabel = Text("Não tem cadastro no AVA? ", 12, MutedColor);
        var registerLink = Text("Criar conta", 13, PrimaryContainer, FontAttributes.Bold);
        var registerTap = new TapGestureRecognizer();
        registerTap.Tapped += async (_, _) => await NavigateTo("//cadastro");
        registerLink.GestureRecognizers.Add(registerTap);

        var registerRow = new HorizontalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            Children = { registerLabel, registerLink },
        };

        var fields = new VerticalStackLayout
        {
            Spacing = 16,
            Children =
            {
                BuildField("Matrícula ou E-mail", identifier),
                new VerticalStackLayout { Spacing = 6, Children = { passwordHeader, passwordInput } },
                remember,
                primaryButton,
                biometric,
                registerRow,
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
                Brush = new SolidColorBrush(Color.FromArgb("#0A000000")),
                Offset = new Point(0, 2),
                Radius = 12,
                Opacity = 1,
            },
            Content = fields,
        };
    }

    View BuildField(string label, View input)
    {
        return new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                Text(label, 12, LabelColor, FontAttributes.Bold),
                input,
            },
        };
    }

    View BuildInput(string placeholder, bool isPassword)
    {
        var entry = new Entry
        {
            Placeholder = placeholder,
            PlaceholderColor = PlaceholderColor,
            TextColor = OnSurfaceColor,
            FontSize = 14,
            BackgroundColor = Colors.Transparent,
            IsPassword = isPassword,
            VerticalOptions = LayoutOptions.Center,
        };

        return InputBorder(entry);
    }

    static Border InputBorder(View content) => new Border
    {
        BackgroundColor = FieldBackground,
        Stroke = FieldStroke,
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = 12 },
        HeightRequest = 44,
        Padding = new Thickness(12, 0, 4, 0),
        Content = content,
    };

    View PrimaryButton()
    {
        var button = new Border
        {
            BackgroundColor = PrimaryContainer,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            StrokeThickness = 0,
            HeightRequest = 44,
            Padding = new Thickness(0),
            Content = new HorizontalStackLayout
            {
                Spacing = 6,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    Text("Entrar no AVA", 14, Colors.White, FontAttributes.Bold),
                    Icon("arrow_forward", 18, Colors.White),
                },
            },
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await NavigateHome();
        button.GestureRecognizers.Add(tap);

        return button;
    }

    View BuildFooter()
    {
        var footer = new HorizontalStackLayout
        {
            Spacing = 12,
            HorizontalOptions = LayoutOptions.Center,
            Padding = new Thickness(0, 8, 0, 20),
            Children =
            {
                Text("Ajuda e Suporte", 12, FaintText),
                Text("•", 12, FaintText),
                Text("Políticas de Acesso", 12, FaintText),
            },
        };

        return footer;
    }

    void TogglePassword()
    {
        if (_passwordEntry is null || _passwordToggleIcon is null)
            return;

        _passwordVisible = !_passwordVisible;
        _passwordEntry.IsPassword = !_passwordVisible;
        _passwordToggleIcon.Source = IconSource(_passwordVisible ? "visibility_off" : "visibility", 19, PlaceholderColor);
    }

    static async Task NavigateHome() => await NavigateTo("//inicio");

    static async Task NavigateTo(string route)
    {
        if (Shell.Current is null)
            return;

        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch
        {
        }
    }
}
