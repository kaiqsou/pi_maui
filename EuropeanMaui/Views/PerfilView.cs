using EuropeanMaui.Controls;
using EuropeanMaui.Services;
using EuropeanMaui.ViewModels;

namespace EuropeanMaui.Views
{
    /// <summary>Perfil do usuário (equivalente ao ProfileScreen).</summary>
    public class PerfilView : Grid
    {
        public PerfilView(PerfilViewModel vm)
        {
            BackgroundColor = Colors.Transparent;

            var header = new HeaderView { Title = AppText.Perfil.Title, Subtitle = AppText.Perfil.Subtitle };

            // Hero
            var avatar = new AvatarView { Name = vm.User?.Name ?? "", Size = 72 };
            var nameLabel = new Label
            {
                Text = vm.User?.Name ?? "",
                FontSize = 20,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get("Text"),
            };
            var roleBadge = new BadgeView { Label = vm.RoleLabel, Tone = BadgeTone.Primary };
            var emailLabel = new Label
            {
                Text = vm.User?.Email ?? "",
                FontSize = 13,
                TextColor = ThemeColors.Get("TextMuted"),
            };
            var programLabel = new Label
            {
                Text = vm.Program?.FullName ?? "",
                FontSize = 13,
                FontFamily = "OpenSansSemibold",
                TextColor = ThemeColors.Get("Text"),
            };

            var heroCol = new VerticalStackLayout { Spacing = 4 };
            heroCol.Children.Add(nameLabel);
            heroCol.Children.Add(new HorizontalStackLayout { Spacing = 6, Children = { roleBadge, programLabel } });
            heroCol.Children.Add(emailLabel);

            var editButton = new ButtonView
            {
                Text = AppText.Navigation.MenuEditProfile,
                Variant = AppButtonVariant.Secondary,
                ButtonSize = AppButtonSize.Small,
                VerticalOptions = LayoutOptions.Center,
            };
            editButton.Command = vm.OpenSettingsCommand;

            var heroRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 16,
            };
            heroRow.AddAt(avatar, 0, 0);
            heroRow.AddAt(heroCol, 1, 0);
            heroRow.AddAt(editButton, 2, 0);

            var fieldsStack = new VerticalStackLayout { Spacing = 0 };
            fieldsStack.Children.Add(FieldRow(AppText.Perfil.NameLabel, vm.User?.Name ?? ""));
            fieldsStack.Children.Add(new BoxView { HeightRequest = 1, Color = ThemeColors.Get("Border"), Margin = new Thickness(0, 4) });
            fieldsStack.Children.Add(FieldRow(AppText.Perfil.EmailLabel, vm.User?.Email ?? ""));
            fieldsStack.Children.Add(new BoxView { HeightRequest = 1, Color = ThemeColors.Get("Border"), Margin = new Thickness(0, 4) });
            fieldsStack.Children.Add(FieldRow(AppText.Perfil.RoleLabel, vm.RoleLabel));
            fieldsStack.Children.Add(new BoxView { HeightRequest = 1, Color = ThemeColors.Get("Border"), Margin = new Thickness(0, 4) });
            fieldsStack.Children.Add(FieldRow(AppText.Perfil.ProgramLabel, vm.Program?.FullName ?? "-"));

            var preferencesStack = new VerticalStackLayout { Spacing = 12 };
            preferencesStack.Children.Add(SectionLabel(AppText.Perfil.Preferences));
            preferencesStack.Children.Add(new ThemeToggleView());

            var aboutStack = new VerticalStackLayout { Spacing = 8 };
            aboutStack.Children.Add(SectionLabel(AppText.Perfil.AboutLabel));
            aboutStack.Children.Add(InfoRow(AppText.Configuracoes.Version, AppText.Configuracoes.VersionValue));

            var logoutButton = new ButtonView
            {
                Text = AppText.Perfil.Logout,
                Variant = AppButtonVariant.Danger,
                ButtonSize = AppButtonSize.Medium,
                LeftGlyph = Icons.LogOutOutline,
                HorizontalOptions = LayoutOptions.Start,
            };
            logoutButton.Command = vm.LogoutCommand;

            var content = new VerticalStackLayout { Spacing = 16, HorizontalOptions = LayoutOptions.Center, MaximumWidthRequest = 680 };
            content.Children.Add(header);
            content.Children.Add(Card(heroRow));
            content.Children.Add(Card(fieldsStack));
            content.Children.Add(Card(preferencesStack));
            content.Children.Add(Card(aboutStack));
            content.Children.Add(logoutButton);

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

        private static Grid FieldRow(string label, string value)
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                Padding = new Thickness(0, 8),
            };
            grid.Add(new Label { Text = label, FontSize = 13, TextColor = ThemeColors.Get("TextMuted"), VerticalOptions = LayoutOptions.Center }, 0, 0);
            grid.Add(new Label { Text = value, FontSize = 14, FontFamily = "OpenSansSemibold", TextColor = ThemeColors.Get("Text"), VerticalOptions = LayoutOptions.Center }, 1, 0);
            return grid;
        }

        private static Grid InfoRow(string label, string value)
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
            };
            grid.Add(new Label { Text = label, FontSize = 13, TextColor = ThemeColors.Get("TextMuted"), VerticalOptions = LayoutOptions.Center }, 0, 0);
            grid.Add(new Label { Text = value, FontSize = 13, FontFamily = "OpenSansSemibold", TextColor = ThemeColors.Get("Text"), VerticalOptions = LayoutOptions.Center }, 1, 0);
            return grid;
        }
    }
}