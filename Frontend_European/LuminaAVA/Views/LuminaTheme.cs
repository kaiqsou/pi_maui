using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace LuminaAVA.Views;

public static class LuminaTheme
{
    public static bool IsDark { get; private set; }

    public static int FontScale { get; private set; } = 1;

    public static double FontFactor => FontScale switch
    {
        0 => 0.9,
        2 => 1.15,
        _ => 1.0,
    };

    public static event Action? Changed;

    public static void SetDark(bool dark)
    {
        if (IsDark == dark)
            return;

        IsDark = dark;
        if (Application.Current is not null)
            Application.Current.UserAppTheme = dark ? AppTheme.Dark : AppTheme.Light;
        Changed?.Invoke();
    }

    public static void SetFontScale(int scale)
    {
        if (FontScale == scale)
            return;

        FontScale = scale;
        Changed?.Invoke();
    }

    static Color Pick(string light, string dark) => Color.FromArgb(IsDark ? dark : light);

    public static Color Surface => Pick("#FAF8FF", "#0F1320");
    public static Color SurfaceCard => Pick("#FFFFFF", "#1B2036");
    public static Color SurfaceSoft => Pick("#F2F3FF", "#232841");
    public static Color SurfaceContainer => Pick("#EAEDFF", "#262C45");
    public static Color SurfaceContainerHigh => Pick("#E2E7FF", "#2E3550");
    public static Color OnSurface => Pick("#131B2E", "#E6E8F6");
    public static Color TextVariant => Pick("#464555", "#C2C6DA");
    public static Color TextMuted => Pick("#737373", "#9BA0B6");
    public static Color TextFaint => Pick("#A3A3A3", "#7C8199");
    public static Color TextSoft => Pick("#404040", "#D6D9E8");
    public static Color Outline => Pick("#777587", "#8C91A8");
    public static Color Primary => Pick("#3525CD", "#7C74FF");
    public static Color PrimaryContainer => Pick("#4F46E5", "#6C63FF");
    public static Color Secondary => Pick("#006C49", "#4EDEA3");
    public static Color Success => Pick("#059669", "#4EDEA3");
    public static Color Error => Pick("#BA1A1A", "#FFB4AB");
    public static Color OnSecondaryContainer => Pick("#00714D", "#6FFBBE");
    public static Color Track => Pick("#F5F5F5", "#2A3050");
    public static Color Divider => Pick("#F1F1F4", "#2A3050");
    public static Color DividerSoft => Pick("#E6E9F5", "#2E3550");
    public static Color RowDivider => Pick("#F2F3FF", "#232841");
    public static Color CardStroke => Pick("#0D000000", "#33FFFFFF");
    public static Color CardStrokeStrong => Pick("#E2E7FF", "#2D3450");
    public static Color HeaderStroke => Pick("#0A000000", "#1FFFFFFF");
    public static Color HeaderStrokeSoft => Pick("#EAEDFF", "#262C45");
    public static Color SecondaryStroke => Pick("#0F000000", "#1FFFFFFF");
    public static Color AvatarBackground => Pick("#E9DDFF", "#332B6B");
    public static Color FieldBackground => Pick("#FAFAFA", "#1B2036");
    public static Color FieldStroke => Pick("#E5E5E5", "#333A56");
    public static Color Placeholder => Pick("#A3A3A3", "#6E7388");
    public static Color LabelStrong => Pick("#404040", "#D6D9E8");
    public static Color SubtleText => Pick("#525252", "#B7BCD0");
    public static Color BadgePrimaryBg => Pick("#66E2DFFF", "#337C74FF");
    public static Color BadgeGradeBg => Pick("#4D6CF8BB", "#334EDEA3");
    public static Color ActivePillBg => Pick("#666CF8BB", "#334EDEA3");
    public static Color IconBoxBackground => Pick("#EAEDFF", "#262C45");
    public static Color SoftTint => Pick("#99F2F3FF", "#262C45");
    public static Color SoftTintStrong => Pick("#80F2F3FF", "#232841");
    public static Color IconTint => Pick("#1A3525CD", "#337C74FF");
    public static Color PlayerBackground => Pick("#283044", "#05070F");
}
