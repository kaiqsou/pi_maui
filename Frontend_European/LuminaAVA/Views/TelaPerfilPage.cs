using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace LuminaAVA.Views;

public class TelaPerfilPage : ThemedPage
{
    const string IconFont = "MaterialSymbolsOutlined";

    static Color SurfaceColor => LuminaTheme.Surface;
    static Color PrimaryColor => LuminaTheme.Primary;
    static Color OnSurfaceColor => LuminaTheme.OnSurface;
    static Color TextVariant => LuminaTheme.TextVariant;
    static Color TextFaint => LuminaTheme.TextFaint;
    static Color OutlineColor => LuminaTheme.Outline;
    static Color SecondaryColor => LuminaTheme.Secondary;
    static Color OnSecondaryContainer => LuminaTheme.OnSecondaryContainer;
    static Color ErrorColor => LuminaTheme.Error;
    static Color ActivePillBackground => LuminaTheme.ActivePillBg;
    static Color SoftSurface => LuminaTheme.SurfaceSoft;
    static Color IconBoxBackground => LuminaTheme.IconBoxBackground;
    static Color CardStroke => LuminaTheme.CardStroke;
    static Color HeaderStroke => LuminaTheme.HeaderStroke;
    static Color AvatarBackground => LuminaTheme.AvatarBackground;

    Border? _themeLight;
    Border? _themeDark;
    Label? _themeLightLabel;
    Label? _themeDarkLabel;

    Border? _fontSmall;
    Border? _fontMedium;
    Border? _fontLarge;
    Label? _fontSmallLabel;
    Label? _fontMediumLabel;
    Label? _fontLargeLabel;

    public TelaPerfilPage()
    {
        Title = "Perfil";
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        Content = BuildRoot();
    }

    protected override View BuildRoot()
    {
        BackgroundColor = SurfaceColor;

        var header = BuildHeader();
        var content = BuildContent();
        var bottomNav = LuminaNav.Build("perfil");

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
        FontSize = size * LuminaTheme.FontFactor,
        TextColor = color,
        FontAttributes = attributes,
        VerticalOptions = LayoutOptions.Center,
    };

    static Border Card(View content, double radius = 12, Thickness? padding = null) => new Border
    {
        BackgroundColor = LuminaTheme.SurfaceCard,
        StrokeShape = new RoundRectangle { CornerRadius = radius },
        StrokeThickness = 0,
        Padding = padding ?? new Thickness(16),
        Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Color.FromArgb("#0A131B2E")),
            Offset = new Point(0, 1),
            Radius = 6,
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
                Text("AVA", 12, TextVariant, FontAttributes.Bold),
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

        var notifIcon = Icon("notifications", 20, TextVariant);
        notifIcon.HorizontalOptions = LayoutOptions.Center;
        notifIcon.VerticalOptions = LayoutOptions.Center;

        var notification = new Grid
        {
            WidthRequest = 36,
            HeightRequest = 36,
            Children = { notifIcon, notificationDot },
        };

        var actions = new HorizontalStackLayout
        {
            Spacing = 8,
            VerticalOptions = LayoutOptions.Center,
            Children = { notification, LuminaAvatar.Build() },
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
                new BoxView { HeightRequest = 1, BackgroundColor = HeaderStroke },
            },
        };
    }

    View BuildContent()
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 20,
            Padding = new Thickness(16, 16, 16, 28),
            Children =
            {
                BuildProfileCard(),
                BuildAppearanceSection(),
                BuildCourseSection(),
                BuildNotificationsSection(),
                BuildSecuritySection(),
            },
        };

        return new ScrollView
        {
            Content = stack,
            VerticalOptions = LayoutOptions.Fill,
        };
    }

    View BuildProfileCard()
    {
        var avatar = LuminaAvatar.Build(64);

        var camera = new Border
        {
            BackgroundColor = PrimaryColor,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            StrokeThickness = 0,
            WidthRequest = 24,
            HeightRequest = 24,
            Padding = new Thickness(0),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 1, 1),
            Content = Icon("photo_camera", 14, Colors.White),
        };
        ((Image)camera.Content).HorizontalOptions = LayoutOptions.Center;

        var avatarStack = new Grid
        {
            WidthRequest = 68,
            HeightRequest = 68,
            Children = { avatar, camera },
        };
        avatarStack.HorizontalOptions = LayoutOptions.Start;
        avatarStack.VerticalOptions = LayoutOptions.Start;
        Grid.SetRow(avatar, 0);
        Grid.SetRow(camera, 0);

        var dot = new Border
        {
            BackgroundColor = SecondaryColor,
            StrokeShape = new RoundRectangle { CornerRadius = 3 },
            StrokeThickness = 0,
            WidthRequest = 6,
            HeightRequest = 6,
            VerticalOptions = LayoutOptions.Center,
        };

        var statusPill = new Border
        {
            BackgroundColor = ActivePillBackground,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            StrokeThickness = 0,
            Padding = new Thickness(8, 3),
            HorizontalOptions = LayoutOptions.End,
            Content = new HorizontalStackLayout
            {
                Spacing = 4,
                Children = { dot, Text("Ativo", 11, OnSecondaryContainer, FontAttributes.Bold) },
            },
        };

        var nameLabel = Text(AlunoData.Nome, 20, OnSurfaceColor, FontAttributes.Bold);
        nameLabel.LineBreakMode = LineBreakMode.TailTruncation;

        var nameRow = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children = { nameLabel, statusPill },
        };
        nameRow.SetColumn(nameLabel, 0);
        nameRow.SetColumn(statusPill, 1);

        var courseLabel = Text($"{AlunoData.Curso} • {AlunoData.SemestreTexto}", 13, TextVariant);
        courseLabel.LineBreakMode = LineBreakMode.TailTruncation;

        var registration = new HorizontalStackLayout
        {
            Spacing = 4,
            Children =
            {
                Text("Matrícula:", 11, OutlineColor),
                Text(AlunoData.Matricula, 11, OnSurfaceColor, FontAttributes.Bold),
            },
        };

        var info = new VerticalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Fill,
            Children = { nameRow, courseLabel, registration },
        };

        var top = new Grid
        {
            ColumnSpacing = 16,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            Children = { avatarStack, info },
        };
        top.SetColumn(avatarStack, 0);
        top.SetColumn(info, 1);

        var editInner = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children =
            {
                new HorizontalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        Icon("badge", 18, PrimaryColor),
                        Text("Dados cadastrais completos", 12, TextVariant),
                    },
                },
                new HorizontalStackLayout
                {
                    Spacing = 2,
                    Children =
                    {
                        Text("Editar", 13, PrimaryColor, FontAttributes.Bold),
                        Icon("chevron_right", 16, PrimaryColor),
                    },
                },
            },
        };
        editInner.SetColumn(editInner.Children[0], 0);
        editInner.SetColumn(editInner.Children[1], 1);

        var editRow = new Border
        {
            BackgroundColor = LuminaTheme.SoftTint,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            StrokeThickness = 0,
            Padding = new Thickness(12, 8),
            Margin = new Thickness(0, 16, 0, 0),
            Content = editInner,
        };

        return Card(new VerticalStackLayout { Spacing = 0, Children = { top, editRow } });
    }

    View BuildAppearanceSection()
    {
        var dark = LuminaTheme.IsDark;

        _themeLightLabel = Text("Modo Claro", 13, dark ? TextVariant : PrimaryColor, dark ? FontAttributes.None : FontAttributes.Bold);
        _themeDarkLabel = Text("Modo Escuro", 13, dark ? PrimaryColor : TextVariant, dark ? FontAttributes.Bold : FontAttributes.None);

        _themeLight = ThemeButton(_themeLightLabel, "light_mode", !dark);
        _themeDark = ThemeButton(_themeDarkLabel, "dark_mode", dark);

        var lightTap = new TapGestureRecognizer();
        lightTap.Tapped += (_, _) => SelectTheme(true);
        _themeLight.GestureRecognizers.Add(lightTap);

        var darkTap = new TapGestureRecognizer();
        darkTap.Tapped += (_, _) => SelectTheme(false);
        _themeDark.GestureRecognizers.Add(darkTap);

        var themeSelector = new Grid
        {
            ColumnSpacing = 4,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
            Children = { _themeLight, _themeDark },
        };
        themeSelector.SetColumn(_themeLight, 0);
        themeSelector.SetColumn(_themeDark, 1);

        var selectorContainer = new Border
        {
            BackgroundColor = SoftSurface,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            StrokeThickness = 0,
            Padding = new Thickness(4),
            Content = themeSelector,
        };

        var themeBlock = new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                Text("Tema do AVA", 13, TextVariant, FontAttributes.Bold),
                selectorContainer,
            },
        };

        var scale = LuminaTheme.FontScale;

        _fontSmallLabel = Text("A", 12, scale == 0 ? PrimaryColor : TextVariant, scale == 0 ? FontAttributes.Bold : FontAttributes.None);
        _fontMediumLabel = Text("A+", 14, scale == 1 ? PrimaryColor : TextVariant, scale == 1 ? FontAttributes.Bold : FontAttributes.None);
        _fontLargeLabel = Text("A++", 16, scale == 2 ? PrimaryColor : TextVariant, scale == 2 ? FontAttributes.Bold : FontAttributes.None);

        _fontSmall = FontPill(_fontSmallLabel);
        _fontMedium = FontPill(_fontMediumLabel);
        _fontLarge = FontPill(_fontLargeLabel);

        var smallTap = new TapGestureRecognizer();
        smallTap.Tapped += (_, _) => SelectFont(0);
        _fontSmall.GestureRecognizers.Add(smallTap);

        var mediumTap = new TapGestureRecognizer();
        mediumTap.Tapped += (_, _) => SelectFont(1);
        _fontMedium.GestureRecognizers.Add(mediumTap);

        var largeTap = new TapGestureRecognizer();
        largeTap.Tapped += (_, _) => SelectFont(2);
        _fontLarge.GestureRecognizers.Add(largeTap);

        var fontRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 2,
                    Children =
                    {
                        Text("Tamanho da Fonte dos Textos", 13, OnSurfaceColor, FontAttributes.Bold),
                        Text("Ajuste de legibilidade nos e-books e fóruns", 12, OutlineColor),
                    },
                },
                new Border
                {
                    BackgroundColor = SoftSurface,
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    StrokeThickness = 0,
                    Padding = new Thickness(4),
                    Content = new HorizontalStackLayout
                    {
                        Spacing = 2,
                        Children = { _fontSmall, _fontMedium, _fontLarge },
                    },
                },
            },
        };
        fontRow.SetColumn(fontRow.Children[0], 0);
        fontRow.SetColumn(fontRow.Children[1], 1);

        ApplyThemeButton(_themeLight, _themeLightLabel, !dark);
        ApplyThemeButton(_themeDark, _themeDarkLabel, dark);
        ApplyFontPill(_fontSmall, _fontSmallLabel, scale == 0);
        ApplyFontPill(_fontMedium, _fontMediumLabel, scale == 1);
        ApplyFontPill(_fontLarge, _fontLargeLabel, scale == 2);

        return new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                SectionTitle("palette", "APARÊNCIA & LEITURA"),
                Card(new VerticalStackLayout { Spacing = 16, Children = { themeBlock, fontRow } }),
            },
        };
    }

    static Border ThemeButton(Label label, string glyph, bool active)
    {
        var icon = Icon(glyph, 18, active ? PrimaryColor : TextVariant);

        var content = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children = { icon, label },
        };

        var button = new Border
        {
            BackgroundColor = active ? LuminaTheme.SurfaceCard : Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            StrokeThickness = 0,
            Padding = new Thickness(0, 10),
            Content = content,
        };

        if (active)
        {
            button.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#14000000")),
                Offset = new Point(0, 1),
                Radius = 3,
                Opacity = 1,
            };
        }

        return button;
    }

    static Border FontPill(Label label)
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            StrokeThickness = 0,
            Padding = new Thickness(12, 6),
            Content = label,
        };
    }

    void SelectTheme(bool light)
    {
        LuminaTheme.SetDark(!light);
        ApplyThemeButton(_themeLight, _themeLightLabel, light);
        ApplyThemeButton(_themeDark, _themeDarkLabel, !light);
    }

    static void ApplyThemeButton(Border? button, Label? label, bool active)
    {
        if (button is null || label is null)
            return;

        button.BackgroundColor = active ? LuminaTheme.SurfaceCard : Colors.Transparent;
        button.Shadow = active
            ? new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#14000000")),
                Offset = new Point(0, 1),
                Radius = 3,
                Opacity = 1,
            }
            : null!;
        label.TextColor = active ? PrimaryColor : TextVariant;
        label.FontAttributes = active ? FontAttributes.Bold : FontAttributes.None;
    }

    void SelectFont(int index)
    {
        LuminaTheme.SetFontScale(index);
        ApplyFontPill(_fontSmall, _fontSmallLabel, index == 0);
        ApplyFontPill(_fontMedium, _fontMediumLabel, index == 1);
        ApplyFontPill(_fontLarge, _fontLargeLabel, index == 2);
    }

    static void ApplyFontPill(Border? pill, Label? label, bool active)
    {
        if (pill is null || label is null)
            return;

        pill.BackgroundColor = active ? LuminaTheme.SurfaceCard : Colors.Transparent;
        pill.Shadow = active
            ? new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#14000000")),
                Offset = new Point(0, 1),
                Radius = 3,
                Opacity = 1,
            }
            : null!;
        label.TextColor = active ? PrimaryColor : TextVariant;
        label.FontAttributes = active ? FontAttributes.Bold : FontAttributes.None;
    }

    View BuildCourseSection()
    {
        var courseHeader = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 2,
                    Children =
                    {
                        Text("GRADUAÇÃO PRESENCIAL & AVA", 11, PrimaryColor, FontAttributes.Bold),
                        Text(AlunoData.Curso, 18, OnSurfaceColor, FontAttributes.Bold),
                        Text($"{AlunoData.Polo} • {AlunoData.Turno} • {AlunoData.Matriz}", 12, TextVariant),
                    },
                },
                new Border
                {
                    BackgroundColor = LuminaTheme.IconTint,
                    StrokeShape = new RoundRectangle { CornerRadius = 18 },
                    StrokeThickness = 0,
                    WidthRequest = 36,
                    HeightRequest = 36,
                    Padding = new Thickness(0),
                    VerticalOptions = LayoutOptions.Start,
                    Content = Icon("terminal", 20, PrimaryColor),
                },
            },
        };
        courseHeader.SetColumn(courseHeader.Children[0], 0);
        courseHeader.SetColumn(courseHeader.Children[1], 1);

        var crRow = new Grid
        {
            BackgroundColor = LuminaTheme.SurfaceCard,
            Padding = new Thickness(12, 10),
            Margin = new Thickness(0, 16, 0, 0),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 2,
                    Children =
                    {
                        Text("Coeficiente de Rendimento (CR)", 11, OutlineColor),
                        new Label
                        {
                            Text = $"{AlunoData.CrTexto} / 10",
                            FontSize = 20 * LuminaTheme.FontFactor,
                            TextColor = SecondaryColor,
                            FontAttributes = FontAttributes.Bold,
                        },
                    },
                },
                new VerticalStackLayout
                {
                    Spacing = 2,
                    HorizontalOptions = LayoutOptions.End,
                    Children =
                    {
                        new Label { Text = "Carga Concluída", FontSize = 11 * LuminaTheme.FontFactor, TextColor = OutlineColor, HorizontalTextAlignment = TextAlignment.End },
                        new Label { Text = AlunoData.CargaTexto, FontSize = 13 * LuminaTheme.FontFactor, TextColor = OnSurfaceColor, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.End },
                    },
                },
            },
        };
        crRow.SetColumn(crRow.Children[0], 0);
        crRow.SetColumn(crRow.Children[1], 1);

        var courseBlock = new VerticalStackLayout
        {
            Padding = new Thickness(16),
            BackgroundColor = LuminaTheme.SoftTint,
            Children = { courseHeader, crRow },
        };

        var actions = new VerticalStackLayout
        {
            Spacing = 4,
            Padding = new Thickness(8),
            Children =
            {
                ActionRow("history_edu", "Histórico de Requerimentos", "2 solicitações finalizadas"),
                ActionRow("swap_horiz", "Transferência de Turno / Polo", "Janela aberta até 30 de Outubro"),
                BuildLockNotice(),
            },
        };

        return new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                SectionTitle("school", "SECRETARIA & CURSO"),
                Card(new VerticalStackLayout { Spacing = 0, Children = { courseBlock, actions } }, radius: 12, padding: new Thickness(0)),
            },
        };
    }

    View BuildLockNotice()
    {
        var request = new Border
        {
            BackgroundColor = LuminaTheme.SurfaceCard,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            StrokeThickness = 0,
            Padding = new Thickness(12, 6),
            VerticalOptions = LayoutOptions.Start,
            Content = Text("Solicitar", 11, TextVariant, FontAttributes.Bold),
        };

        var requestTap = new TapGestureRecognizer();
        requestTap.Tapped += async (_, _) => await DisplayAlertAsync(
            "Trancamento de Matrícula",
            "Abertura de protocolo para trancamento guiado. Sua vaga fica resguardada por até 2 semestres.",
            "OK");
        request.GestureRecognizers.Add(requestTap);

        var headerRow = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 2,
                    Children =
                    {
                        Text("Trancamento de Matrícula", 13, OnSurfaceColor, FontAttributes.Bold),
                        Text("Período letivo regular 2024.2 em vigência", 12, OutlineColor),
                    },
                },
                request,
            },
        };
        headerRow.SetColumn(headerRow.Children[0], 0);
        headerRow.SetColumn(request, 1);

        var warning = Text("Aviso: O trancamento mantém seu histórico e isenta mensalidades futuras após deferimento da coordenação.", 11, OutlineColor);
        warning.LineBreakMode = LineBreakMode.WordWrap;

        return new Border
        {
            BackgroundColor = LuminaTheme.SoftTintStrong,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            StrokeThickness = 0,
            Padding = new Thickness(12),
            Margin = new Thickness(0, 4, 0, 0),
            Content = new VerticalStackLayout { Spacing = 8, Children = { headerRow, warning } },
        };
    }

    View BuildNotificationsSection()
    {
        return new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                SectionTitle("notifications_active", "NOTIFICAÇÕES & AVISOS"),
                Card(new VerticalStackLayout
                {
                    Spacing = 16,
                    Children =
                    {
                        NotificationRow("assignment_late", "Lembretes de Provas e Prazos", "Alertas 48h e 6h antes de entregas", true),
                        NotificationRow("video_library", "Avisos de Novas Aulas e Fóruns", "Quando professores liberam novos módulos", true),
                        NotificationRow("forum", "Feedback dos Tutores", "Correções com notas e orientações", false),
                    },
                }),
            },
        };
    }

    View BuildSecuritySection()
    {
        var securityCard = Card(new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                ActionRow("lock_reset", "Alterar Senha de Acesso", "Última troca há 4 meses"),
                ActionRow("devices", "Dispositivos Conectados", "Este dispositivo (.NET MAUI) + 1 Web", SecondaryColor),
            },
        }, padding: new Thickness(8));

        var logout = new Border
        {
            BackgroundColor = LuminaTheme.SurfaceCard,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            StrokeThickness = 0,
            Padding = new Thickness(0, 14),
            Margin = new Thickness(0, 8, 0, 0),
            Content = new HorizontalStackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    Icon("logout", 18, ErrorColor),
                    Text("Sair da Conta (Logout)", 15, ErrorColor, FontAttributes.Bold),
                },
            },
        };

        var logoutTap = new TapGestureRecognizer();
        logoutTap.Tapped += async (_, _) =>
        {
            var confirm = await DisplayAlertAsync("Sair da Conta", "Deseja realmente desconectar sua conta do Lumina AVA?", "Sair", "Cancelar");
            if (confirm && Shell.Current is not null)
            {
                try
                {
                    await Shell.Current.GoToAsync("//login");
                }
                catch
                {
                }
            }
        };
        logout.GestureRecognizers.Add(logoutTap);

        var version = new Label
        {
            Text = "Lumina AVA Mobile • v3.8.4 (Build 412)",
            FontSize = 12 * LuminaTheme.FontFactor,
            TextColor = OutlineColor,
            HorizontalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0),
        };

        return new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                SectionTitle("security", "SEGURANÇA & ACESSO"),
                securityCard,
                logout,
                version,
            },
        };
    }

    static View SectionTitle(string glyph, string title) => new HorizontalStackLayout
    {
        Spacing = 6,
        Padding = new Thickness(4, 0, 0, 0),
        Children =
        {
            Icon(glyph, 18, TextVariant),
            Text(title, 11, OnSurfaceColor, FontAttributes.Bold),
        },
    };

    static Border IconBox(string glyph)
    {
        var icon = Icon(glyph, 18, OnSurfaceColor);
        icon.HorizontalOptions = LayoutOptions.Center;

        return new Border
        {
            BackgroundColor = IconBoxBackground,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            StrokeThickness = 0,
            WidthRequest = 32,
            HeightRequest = 32,
            Padding = new Thickness(0),
            VerticalOptions = LayoutOptions.Center,
            Content = icon,
        };
    }

    static View ActionRow(string glyph, string title, string subtitle, Color? subtitleColor = null)
    {
        var titleLabel = Text(title, 13, OnSurfaceColor, FontAttributes.Bold);
        titleLabel.LineBreakMode = LineBreakMode.TailTruncation;

        var subtitleLabel = Text(subtitle, 12, subtitleColor ?? OutlineColor);
        subtitleLabel.LineBreakMode = LineBreakMode.TailTruncation;

        var row = new Grid
        {
            ColumnSpacing = 12,
            Padding = new Thickness(8),
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children =
            {
                IconBox(glyph),
                new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { titleLabel, subtitleLabel } },
                Icon("chevron_right", 18, OutlineColor),
            },
        };
        row.SetColumn(row.Children[0], 0);
        row.SetColumn(row.Children[1], 1);
        row.SetColumn(row.Children[2], 2);

        return row;
    }

    static View NotificationRow(string glyph, string title, string subtitle, bool isOn)
    {
        var titleLabel = Text(title, 13, OnSurfaceColor, FontAttributes.Bold);
        titleLabel.LineBreakMode = LineBreakMode.TailTruncation;

        var subtitleLabel = Text(subtitle, 12, OutlineColor);
        subtitleLabel.LineBreakMode = LineBreakMode.TailTruncation;

        var row = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            Children =
            {
                IconBox(glyph),
                new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { titleLabel, subtitleLabel } },
                new Switch
                {
                    IsToggled = isOn,
                    OnColor = PrimaryColor,
                    ThumbColor = Colors.White,
                    VerticalOptions = LayoutOptions.Center,
                },
            },
        };
        row.SetColumn(row.Children[0], 0);
        row.SetColumn(row.Children[1], 1);
        row.SetColumn(row.Children[2], 2);

        return row;
    }
}
