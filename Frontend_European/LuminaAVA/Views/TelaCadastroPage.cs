using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace LuminaAVA.Views;

public class TelaCadastroPage : ThemedPage
{
    const string IconFont = "MaterialSymbolsOutlined";

    static Color SurfaceColor => LuminaTheme.Surface;
    static Color PrimaryColor => LuminaTheme.Primary;
    static Color PrimaryContainer => LuminaTheme.PrimaryContainer;
    static Color OnSurfaceColor => LuminaTheme.OnSurface;
    static Color LabelColor => LuminaTheme.LabelStrong;
    static Color PlaceholderColor => LuminaTheme.Placeholder;
    static Color MutedColor => LuminaTheme.TextMuted;

    Entry? _passwordEntry;
    Image? _passwordToggleIcon;
    bool _passwordVisible;

    Entry? _confirmEntry;
    Image? _confirmToggleIcon;
    bool _confirmVisible;

    Entry? _matriculaEntry;
    Entry? _nomeEntry;
    Entry? _emailEntry;
    CheckBox? _terms;

    public TelaCadastroPage()
    {
        Title = "Cadastro";
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        Content = BuildRoot();
    }

    protected override View BuildRoot()
    {
        BackgroundColor = SurfaceColor;

        var content = new VerticalStackLayout
        {
            Spacing = 0,
            Padding = new Thickness(20, 24, 20, 20),
            Children =
            {
                BuildTopBar(),
                BuildHeader(),
                BuildCard(),
                BuildFooter(),
            },
        };

        return new ScrollView
        {
            Content = content,
            VerticalOptions = LayoutOptions.Fill,
        };
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

    View BuildTopBar()
    {
        var backContent = new HorizontalStackLayout
        {
            Spacing = 4,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Icon("arrow_back", 18, MutedColor),
                Text("Voltar ao login", 13, MutedColor, FontAttributes.Bold),
            },
        };

        var back = new Border
        {
            BackgroundColor = LuminaTheme.SurfaceSoft,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(12, 8),
            Content = backContent,
        };
        AddTap(back, GoToLogin);

        var yearBadge = new Border
        {
            BackgroundColor = LuminaTheme.BadgeGradeBg,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(10, 5),
            VerticalOptions = LayoutOptions.Center,
            Content = new HorizontalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    Icon("school", 14, LuminaTheme.Secondary),
                    Text("Ano Letivo 2025", 12, LuminaTheme.Secondary, FontAttributes.Bold),
                },
            },
        };

        var wrapper = new Grid
        {
            Margin = new Thickness(0, 0, 0, 24),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
            Children = { back, yearBadge },
        };
        wrapper.SetColumn(back, 0);
        wrapper.SetColumn(yearBadge, 1);
        back.HorizontalOptions = LayoutOptions.Start;
        yearBadge.HorizontalOptions = LayoutOptions.End;

        return wrapper;
    }

    View BuildHeader()
    {
        var iconBox = new Border
        {
            BackgroundColor = LuminaTheme.SurfaceSoft,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            WidthRequest = 56,
            HeightRequest = 56,
            Padding = new Thickness(0),
            HorizontalOptions = LayoutOptions.Center,
            Content = Icon("school", 30, PrimaryColor),
        };
        ((Image)iconBox.Content).HorizontalOptions = LayoutOptions.Center;

        var title = Text("Cadastro de Aluno", 26, OnSurfaceColor, FontAttributes.Bold);
        title.HorizontalTextAlignment = TextAlignment.Center;

        var subtitle = Text("Portal Acadêmico do Estudante • Lumina AVA", 13, MutedColor);
        subtitle.HorizontalTextAlignment = TextAlignment.Center;

        return new VerticalStackLayout
        {
            Spacing = 6,
            Margin = new Thickness(0, 0, 0, 24),
            Children = { iconBox, title, subtitle },
        };
    }

    View BuildCard()
    {
        _matriculaEntry = NewEntry("Ex: 2025.1.1234 ou 000.000.000-00", false);
        _nomeEntry = NewEntry("Ex: Gabriel Silva", false);
        _emailEntry = NewEntry("Ex: aluno@lumina.edu.br", false);

        _passwordEntry = NewEntry("Mínimo de 8 caracteres", true);
        _passwordToggleIcon = Icon("visibility", 19, PlaceholderColor);
        var passwordInput = PasswordInput(_passwordEntry, _passwordToggleIcon, () => _passwordVisible = Toggle(_passwordEntry, _passwordToggleIcon, _passwordVisible));

        _confirmEntry = NewEntry("Repita a senha criada", true);
        _confirmToggleIcon = Icon("visibility", 19, PlaceholderColor);
        var confirmInput = PasswordInput(_confirmEntry, _confirmToggleIcon, () => _confirmVisible = Toggle(_confirmEntry, _confirmToggleIcon, _confirmVisible));

        var termsLabel = new Label
        {
            VerticalOptions = LayoutOptions.Center,
            FormattedText = new FormattedString
            {
                Spans =
                {
                    new Span { Text = "Li e concordo com os ", TextColor = MutedColor, FontSize = 12 },
                    new Span { Text = "Termos de Uso", TextColor = PrimaryColor, FontSize = 12, FontAttributes = FontAttributes.Bold },
                    new Span { Text = " e a ", TextColor = MutedColor, FontSize = 12 },
                    new Span { Text = "Política de Privacidade", TextColor = PrimaryColor, FontSize = 12, FontAttributes = FontAttributes.Bold },
                    new Span { Text = ".", TextColor = MutedColor, FontSize = 12 },
                },
            },
        };

        _terms = new CheckBox { Color = PrimaryContainer };
        var termsRow = new Grid
        {
            ColumnSpacing = 4,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            Children = { _terms, termsLabel },
        };
        termsRow.SetColumn(_terms, 0);
        termsRow.SetColumn(termsLabel, 1);

        var registerButton = BuildRegisterButton();

        var loginRow = new HorizontalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                Text("Já possui cadastro?", 12, MutedColor),
            },
        };
        var entrar = Text("Entrar", 13, PrimaryColor, FontAttributes.Bold);
        AddTap(entrar, GoToLogin);
        loginRow.Children.Add(entrar);

        var fields = new VerticalStackLayout
        {
            Spacing = 16,
            Children =
            {
                BuildField("Matrícula ou CPF", "badge", _matriculaEntry),
                BuildField("Nome Completo", "person", _nomeEntry),
                BuildField("E-mail Institucional ou Pessoal", "mail", _emailEntry),
                BuildField("Criar Senha", "lock", passwordInput),
                BuildField("Confirmar Senha", "lock_reset", confirmInput),
                termsRow,
                registerButton,
                loginRow,
            },
        };

        return new Border
        {
            BackgroundColor = LuminaTheme.SurfaceCard,
            Stroke = LuminaTheme.CardStroke,
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

    View BuildRegisterButton()
    {
        var button = new Border
        {
            BackgroundColor = PrimaryContainer,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            StrokeThickness = 0,
            HeightRequest = 48,
            Margin = new Thickness(0, 4, 0, 0),
            Padding = new Thickness(0),
            Content = new HorizontalStackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    Text("Cadastrar Conta", 14, Colors.White, FontAttributes.Bold),
                    Icon("check_circle", 18, Colors.White),
                },
            },
        };

        AddTap(button, RegisterAsync);

        return button;
    }

    View BuildFooter()
    {
        var help = new HorizontalStackLayout
        {
            Spacing = 6,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                Icon("help_outline", 16, MutedColor),
                Text("Precisa de ajuda com a matrícula?", 13, MutedColor),
            },
        };

        var support = new HorizontalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                Text("Suporte ao Aluno", 13, PrimaryColor, FontAttributes.Bold),
                Icon("arrow_forward", 14, PrimaryColor),
            },
        };
        AddTap(support, async () => await DisplayAlertAsync("Suporte ao Aluno", "Nossa equipe pode ajudar com matrícula e acesso ao AVA.", "OK"));

        return new VerticalStackLayout
        {
            Spacing = 6,
            Margin = new Thickness(0, 24, 0, 0),
            Children = { help, support },
        };
    }

    Entry NewEntry(string placeholder, bool isPassword) => new Entry
    {
        Placeholder = placeholder,
        PlaceholderColor = PlaceholderColor,
        TextColor = OnSurfaceColor,
        FontSize = 14,
        BackgroundColor = Colors.Transparent,
        IsPassword = isPassword,
        VerticalOptions = LayoutOptions.Center,
    };

    static Border InputBorder(View content) => new Border
    {
        BackgroundColor = LuminaTheme.FieldBackground,
        Stroke = LuminaTheme.FieldStroke,
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = 12 },
        HeightRequest = 48,
        Padding = new Thickness(12, 0),
        Content = content,
    };

    View BuildField(string label, string glyph, View input)
    {
        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            Children = { Icon(glyph, 20, MutedColor), input },
        };
        row.SetColumn(row.Children[0], 0);
        row.SetColumn(input, 1);

        return new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                Text(label, 12, LabelColor, FontAttributes.Bold),
                InputBorder(row),
            },
        };
    }

    View PasswordInput(Entry entry, Image toggleIcon, Action onToggle)
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => onToggle();
        toggleIcon.GestureRecognizers.Add(tap);

        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children = { entry, toggleIcon },
        };
        row.SetColumn(entry, 0);
        row.SetColumn(toggleIcon, 1);
        toggleIcon.Margin = new Thickness(0, 0, 6, 0);

        return InputBorder(row);
    }

    bool Toggle(Entry entry, Image icon, bool visible)
    {
        var next = !visible;
        entry.IsPassword = !next;
        icon.Source = IconSource(next ? "visibility_off" : "visibility", 19, PlaceholderColor);
        return next;
    }

    static void AddTap(View view, Action action)
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => action();
        view.GestureRecognizers.Add(tap);
    }

    static void AddTap(View view, Func<Task> action)
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await action();
        view.GestureRecognizers.Add(tap);
    }

    async Task RegisterAsync()
    {
        if (Shell.Current is null)
            return;

        var matricula = _matriculaEntry?.Text?.Trim() ?? string.Empty;
        var nome = _nomeEntry?.Text?.Trim() ?? string.Empty;
        var email = _emailEntry?.Text?.Trim() ?? string.Empty;
        var senha = _passwordEntry?.Text ?? string.Empty;
        var confirmacao = _confirmEntry?.Text ?? string.Empty;

        if (matricula.Length == 0 || nome.Length == 0 || email.Length == 0 || senha.Length == 0 || confirmacao.Length == 0)
        {
            await DisplayAlertAsync("Campos obrigatórios", "Preencha todos os campos para concluir o cadastro.", "OK");
            return;
        }

        if (senha.Length < 8)
        {
            await DisplayAlertAsync("Senha inválida", "A senha deve ter no mínimo 8 caracteres.", "OK");
            return;
        }

        if (senha != confirmacao)
        {
            await DisplayAlertAsync("Senhas diferentes", "A confirmação de senha não confere com a senha criada.", "OK");
            return;
        }

        if (_terms is null || !_terms.IsChecked)
        {
            await DisplayAlertAsync("Termos de Uso", "É necessário aceitar os Termos de Uso e a Política de Privacidade.", "OK");
            return;
        }

        await DisplayAlertAsync("Cadastro concluído", $"Aluno {nome} cadastrado com sucesso! Faça login para acessar o seu AVA.", "Ir para o login");
        await GoToLogin();
    }

    static async Task GoToLogin()
    {
        if (Shell.Current is null)
            return;

        try
        {
            await Shell.Current.GoToAsync("//login");
        }
        catch
        {
        }
    }

}
