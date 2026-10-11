using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace LuminaAVA.Views;

public static class LuminaAvatar
{
    const string IconFont = "MaterialSymbolsOutlined";

    static readonly Color PrimaryColor = Color.FromArgb("#3525CD");
    static readonly Color AvatarBackground = Color.FromArgb("#E9DDFF");
    static readonly Color AvatarStroke = Color.FromArgb("#D9DDF0");

    public static View Build(double size = 30)
    {
        var iconSize = size * 0.6;

        var icon = new Image
        {
            Source = new FontImageSource
            {
                FontFamily = IconFont,
                Glyph = "person",
                Size = iconSize,
                Color = PrimaryColor,
            },
            WidthRequest = iconSize,
            HeightRequest = iconSize,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };

        var avatar = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = size / 2 },
            Stroke = AvatarStroke,
            StrokeThickness = 1,
            BackgroundColor = AvatarBackground,
            WidthRequest = size,
            HeightRequest = size,
            Padding = new Thickness(0),
            VerticalOptions = LayoutOptions.Center,
            Content = icon,
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            if (Shell.Current is null)
                return;

            try
            {
                await Shell.Current.GoToAsync("//perfil");
            }
            catch
            {
            }
        };
        avatar.GestureRecognizers.Add(tap);

        return avatar;
    }
}
