using System;

namespace EuropeanMaui.Services
{
    /// <summary>Paletas claro/escuro em code-behind (ResourceDictionary.Source não pode ser setado por código).</summary>
    public static class ThemePalettes
    {
        public static ResourceDictionary Get(bool dark)
        {
            var d = new ResourceDictionary();
            IThemePalette p = dark ? (IThemePalette)new DarkPalette() : new LightPalette();

            d.Add("Primary", p.Primary);
            d.Add("PrimaryDark", p.PrimaryDark);
            d.Add("PrimaryLight", p.PrimaryLight);
            d.Add("Accent", p.Accent);
            d.Add("AccentLight", p.AccentLight);
            d.Add("Background", p.Background);
            d.Add("Surface", p.Surface);
            d.Add("SurfaceAlt", p.SurfaceAlt);
            d.Add("Text", p.Text);
            d.Add("TextMuted", p.TextMuted);
            d.Add("Border", p.Border);
            d.Add("BorderStrong", p.BorderStrong);
            d.Add("Success", p.Success);
            d.Add("Warning", p.Warning);
            d.Add("Danger", p.Danger);
            d.Add("SuccessLight", p.SuccessLight);
            d.Add("WarningLight", p.WarningLight);
            d.Add("DangerLight", p.DangerLight);
            d.Add("Overlay", p.Overlay);

            d.Add("PrimaryDarkText", p.PrimaryDarkText);
            d.Add("Secondary", p.Secondary);
            d.Add("SecondaryDarkText", p.SecondaryDarkText);
            d.Add("Tertiary", p.Tertiary);
            d.Add("White", Colors.White);
            d.Add("Black", Colors.Black);
            d.Add("Magenta", p.Magenta);
            d.Add("MidnightBlue", p.MidnightBlue);
            d.Add("OffBlack", p.OffBlack);
            d.Add("Gray100", p.Gray100);
            d.Add("Gray200", p.Gray200);
            d.Add("Gray300", p.Gray300);
            d.Add("Gray400", p.Gray400);
            d.Add("Gray500", p.Gray500);
            d.Add("Gray600", p.Gray600);
            d.Add("Gray900", p.Gray900);
            d.Add("Gray950", p.Gray950);

            return d;
        }

        private interface IThemePalette
        {
            Color Primary { get; } Color PrimaryDark { get; } Color PrimaryLight { get; }
            Color Accent { get; } Color AccentLight { get; }
            Color Background { get; } Color Surface { get; } Color SurfaceAlt { get; }
            Color Text { get; } Color TextMuted { get; }
            Color Border { get; } Color BorderStrong { get; }
            Color Success { get; } Color Warning { get; } Color Danger { get; }
            Color SuccessLight { get; } Color WarningLight { get; } Color DangerLight { get; }
            Color Overlay { get; }
            Color PrimaryDarkText { get; } Color Secondary { get; } Color SecondaryDarkText { get; } Color Tertiary { get; }
            Color Magenta { get; } Color MidnightBlue { get; } Color OffBlack { get; }
            Color Gray100 { get; } Color Gray200 { get; } Color Gray300 { get; } Color Gray400 { get; }
            Color Gray500 { get; } Color Gray600 { get; } Color Gray900 { get; } Color Gray950 { get; }
        }

        private sealed class LightPalette : IThemePalette
        {
            public Color Primary => Color.FromArgb("#1E4C8A");
            public Color PrimaryDark => Color.FromArgb("#15366A");
            public Color PrimaryLight => Color.FromArgb("#E8EFF9");
            public Color Accent => Color.FromArgb("#1E4C8A");
            public Color AccentLight => Color.FromArgb("#F7EFDC");
            public Color Background => Color.FromArgb("#F6F7F5");
            public Color Surface => Color.FromArgb("#FFFFFF");
            public Color SurfaceAlt => Color.FromArgb("#EEF1F5");
            public Color Text => Color.FromArgb("#161B22");
            public Color TextMuted => Color.FromArgb("#5B6773");
            public Color Border => Color.FromArgb("#E3E7EC");
            public Color BorderStrong => Color.FromArgb("#CBD3DC");
            public Color Success => Color.FromArgb("#15803D");
            public Color Warning => Color.FromArgb("#B4790E");
            public Color Danger => Color.FromArgb("#C0342B");
            public Color SuccessLight => Color.FromArgb("#E7F5EC");
            public Color WarningLight => Color.FromArgb("#FBF0DD");
            public Color DangerLight => Color.FromArgb("#FBEAE8");
            public Color Overlay => Color.FromArgb("#11161D73");
            public Color PrimaryDarkText => Color.FromArgb("#242424");
            public Color Secondary => Color.FromArgb("#DFD8F7");
            public Color SecondaryDarkText => Color.FromArgb("#9880E5");
            public Color Tertiary => Color.FromArgb("#2B0B98");
            public Color Magenta => Color.FromArgb("#D600AA");
            public Color MidnightBlue => Color.FromArgb("#190649");
            public Color OffBlack => Color.FromArgb("#1F1F1F");
            public Color Gray100 => Color.FromArgb("#E1E1E1");
            public Color Gray200 => Color.FromArgb("#C8C8C8");
            public Color Gray300 => Color.FromArgb("#ACACAC");
            public Color Gray400 => Color.FromArgb("#919191");
            public Color Gray500 => Color.FromArgb("#6E6E6E");
            public Color Gray600 => Color.FromArgb("#404040");
            public Color Gray900 => Color.FromArgb("#212121");
            public Color Gray950 => Color.FromArgb("#141414");
        }

        private sealed class DarkPalette : IThemePalette
        {
            public Color Primary => Color.FromArgb("#6C96D6");
            public Color PrimaryDark => Color.FromArgb("#4A78BE");
            public Color PrimaryLight => Color.FromArgb("#1C2A3F");
            public Color Accent => Color.FromArgb("#6C96D6");
            public Color AccentLight => Color.FromArgb("#3A3220");
            public Color Background => Color.FromArgb("#111418");
            public Color Surface => Color.FromArgb("#1B1F24");
            public Color SurfaceAlt => Color.FromArgb("#262B33");
            public Color Text => Color.FromArgb("#F2F5F9");
            public Color TextMuted => Color.FromArgb("#A2AAB5");
            public Color Border => Color.FromArgb("#2A3038");
            public Color BorderStrong => Color.FromArgb("#3A424D");
            public Color Success => Color.FromArgb("#4ADE80");
            public Color Warning => Color.FromArgb("#FBBF24");
            public Color Danger => Color.FromArgb("#F87171");
            public Color SuccessLight => Color.FromArgb("#123524");
            public Color WarningLight => Color.FromArgb("#33240A");
            public Color DangerLight => Color.FromArgb("#3B1213");
            public Color Overlay => Color.FromArgb("#000000CC");
            public Color PrimaryDarkText => Color.FromArgb("#F2F5F9");
            public Color Secondary => Color.FromArgb("#3B3660");
            public Color SecondaryDarkText => Color.FromArgb("#C4B5FD");
            public Color Tertiary => Color.FromArgb("#A78BFA");
            public Color Magenta => Color.FromArgb("#E879C8");
            public Color MidnightBlue => Color.FromArgb("#6D5BB8");
            public Color OffBlack => Color.FromArgb("#E6E6E6");
            public Color Gray100 => Color.FromArgb("#3B3B3B");
            public Color Gray200 => Color.FromArgb("#3F3F3F");
            public Color Gray300 => Color.FromArgb("#4A4A4A");
            public Color Gray400 => Color.FromArgb("#5A5A5A");
            public Color Gray500 => Color.FromArgb("#6A6A6A");
            public Color Gray600 => Color.FromArgb("#8A8A8A");
            public Color Gray900 => Color.FromArgb("#E6E6E6");
            public Color Gray950 => Color.FromArgb("#FAFAFA");
        }
    }
}