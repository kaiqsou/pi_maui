using EuropeanMaui.Controls;
using EuropeanMaui.Services;
using EuropeanMaui.ViewModels;

namespace EuropeanMaui.Views
{
    /// <summary>Configurações (equivalente ao SettingsScreen).</summary>
    public class ConfiguracoesView : Grid
    {
        public ConfiguracoesView(ConfiguracoesViewModel vm)
        {
            BackgroundColor = Colors.Transparent;

            var header = new HeaderView { Title = AppText.Configuracoes.Title };

            var appearance = new VerticalStackLayout { Spacing = 12 };
            appearance.Children.Add(SectionLabel(AppText.Configuracoes.Appearance));
            appearance.Children.Add(new ThemeToggleView());

            var notifications = new VerticalStackLayout { Spacing = 12 };
            notifications.Children.Add(SectionLabel(AppText.Configuracoes.NotificationsLabel));
            notifications.Children.Add(new Label
            {
                Text = AppText.Configuracoes.NotificationsDisabled,
                FontSize = 13,
                TextColor = ThemeColors.Get("TextMuted"),
            });

            var about = new VerticalStackLayout { Spacing = 8 };
            about.Children.Add(SectionLabel(AppText.Configuracoes.AboutLabel));
            about.Children.Add(InfoRow(AppText.Configuracoes.AppName, ""));
            about.Children.Add(InfoRow(AppText.Configuracoes.Version, AppText.Configuracoes.VersionValue));

            var content = new VerticalStackLayout { Spacing = 16, HorizontalOptions = LayoutOptions.Center, MaximumWidthRequest = 680 };
            content.Children.Add(header);
            content.Children.Add(Card(appearance));
            content.Children.Add(Card(notifications));
            content.Children.Add(Card(about));

            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            RowDefinitions.Add(new RowDefinition(GridLength.Star));
            this.AddAt(content, 0, 0);
        }

        private static Border Card(View content) => new()
        {
            StrokeThickness = 1,
            Stroke = ThemeColors.Get("Border"),
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            BackgroundColor = ThemeColors.Get("Surface"),
            Padding = new Thickness(20),
            Content = content,
        };

        private static Label SectionLabel(string text) => new()
        {
            Text = text,
            FontSize = 12,
            FontFamily = "OpenSansBold",
            TextColor = ThemeColors.Get("TextMuted"),
            CharacterSpacing = 0.4,
        };

        private static Grid InfoRow(string label, string value)
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                Margin = new Thickness(0, 4),
            };
            grid.Add(new Label { Text = label, FontSize = 14, TextColor = ThemeColors.Get("Text") }, 0, 0);
            if (!string.IsNullOrEmpty(value))
                grid.Add(new Label { Text = value, FontSize = 13, TextColor = ThemeColors.Get("TextMuted") }, 1, 0);
            return grid;
        }
    }
}