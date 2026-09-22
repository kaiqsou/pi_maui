using EuropeanMaui.Controls;
using EuropeanMaui.Services;
using EuropeanMaui.ViewModels;

namespace EuropeanMaui.Views
{
    /// <summary>Página provisória para telas em construção.</summary>
    public class PlaceholderView : Grid
    {
        public PlaceholderView(PlaceholderViewModel vm)
        {
            BackgroundColor = Colors.Transparent;

            var header = new HeaderView { Title = vm.Title };

            var icon = new GlyphLabel
            {
                Glyph = Icons.GridOutline,
                FontSize = 48,
                TextColor = ThemeColors.Get("TextMuted"),
                HorizontalTextAlignment = TextAlignment.Center,
            };
            var message = new Label
            {
                Text = vm.Message,
                FontSize = 14,
                TextColor = ThemeColors.Get("TextMuted"),
                HorizontalTextAlignment = TextAlignment.Center,
            };
            var state = new VerticalStackLayout { Spacing = 16, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            state.Children.Add(icon);
            state.Children.Add(message);

            var backButton = new ButtonView
            {
                Text = vm.BackLabel,
                Variant = AppButtonVariant.Secondary,
                ButtonSize = AppButtonSize.Medium,
                HorizontalOptions = LayoutOptions.Center,
            };
            backButton.Command = vm.BackCommand;

            var content = new VerticalStackLayout { Spacing = 24, HorizontalOptions = LayoutOptions.Center, MaximumWidthRequest = 680 };
            content.Children.Add(header);
            content.Children.Add(state);
            content.Children.Add(backButton);

            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            RowDefinitions.Add(new RowDefinition(GridLength.Star));
            this.AddAt(content, 0, 0);
        }
    }
}