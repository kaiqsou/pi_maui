using System.ComponentModel;
using EuropeanMaui.Controls;
using EuropeanMaui.Services;
using EuropeanMaui.ViewModels;
using InputView = EuropeanMaui.Controls.InputView;

namespace EuropeanMaui.Views
{
    /// <summary>Página de login com seleção de mestrado (equivalente ao LoginScreen).</summary>
    public class LoginView : Grid
    {
        private readonly LoginViewModel _vm;
        private readonly VerticalStackLayout _credentials;
        private readonly VerticalStackLayout _program;
        private readonly InputView _emailInput;
        private readonly InputView _passwordInput;
        private readonly Label _serverErrorLabel;
        private readonly Label _programErrorLabel;
        private readonly ButtonView _loginButton;
        private readonly ButtonView _enterButton;
        private readonly MasterProgramSelectorView _selector;

        public LoginView(LoginViewModel vm)
        {
            _vm = vm;
            BackgroundColor = Colors.Transparent;

            // ---- Credentials ----
            var brand = new VerticalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.Center };
            var logo = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                BackgroundColor = ThemeColors.Get("Accent"),
                WidthRequest = 56,
                HeightRequest = 56,
                HorizontalOptions = LayoutOptions.Center,
                Content = new GlyphLabel
                {
                    Glyph = Icons.School,
                    FontSize = 28,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
            };
            brand.Children.Add(logo);
            brand.Children.Add(new Label
            {
                Text = AppText.Common.AppName,
                FontSize = 26,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get("Text"),
                HorizontalTextAlignment = TextAlignment.Center,
            });
            brand.Children.Add(new Label
            {
                Text = AppText.Common.Tagline,
                FontSize = 13,
                TextColor = ThemeColors.Get("TextMuted"),
                HorizontalTextAlignment = TextAlignment.Center,
            });

            var demoHint = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                BackgroundColor = ThemeColors.Get("PrimaryLight"),
                Padding = new Thickness(14, 10),
                Content = new Label
                {
                    Text = AppText.Login.DemoHint,
                    FontSize = 12,
                    TextColor = ThemeColors.Get("PrimaryDark"),
                },
            };

            _emailInput = new InputView
            {
                Label = AppText.Login.EmailLabel,
                Placeholder = AppText.Login.EmailPlaceholder,
                PrefixGlyph = Icons.MailOutline,
                Keyboard = Keyboard.Email,
            };
            _emailInput.TextChanged += (_, _) => _vm.Email = _emailInput.Text;

            _passwordInput = new InputView
            {
                Label = AppText.Login.PasswordLabel,
                Placeholder = AppText.Login.PasswordPlaceholder,
                PrefixGlyph = Icons.LockClosedOutline,
                IsPassword = true,
            };
            _passwordInput.TextChanged += (_, _) => _vm.Password = _passwordInput.Text;

            _serverErrorLabel = new Label
            {
                FontSize = 13,
                TextColor = ThemeColors.Get("Danger"),
                IsVisible = false,
            };

            _loginButton = new ButtonView
            {
                Text = AppText.Login.LoginButton,
                Variant = AppButtonVariant.Primary,
                ButtonSize = AppButtonSize.Large,
                IsFullWidth = true,
            };
            _loginButton.Command = vm.SubmitCredentialsCommand;

            var forgotRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
            };
            var remember = new Label
            {
                Text = AppText.Login.RememberMe,
                FontSize = 13,
                TextColor = ThemeColors.Get("TextMuted"),
                VerticalOptions = LayoutOptions.Center,
            };
            var forgot = new PressableScale
            {
                Content = new Label
                {
                    Text = AppText.Login.ForgotPassword,
                    FontSize = 13,
                    FontFamily = "OpenSansSemibold",
                    TextColor = ThemeColors.Get("Primary"),
                },
            };
            forgotRow.AddAt(remember, 0, 0);
            forgotRow.AddAt(forgot, 1, 0);

            var noAccountRow = new HorizontalStackLayout
            {
                Spacing = 4,
                HorizontalOptions = LayoutOptions.Center,
            };
            noAccountRow.Children.Add(new Label
            {
                Text = AppText.Login.NoAccount,
                FontSize = 13,
                TextColor = ThemeColors.Get("TextMuted"),
                VerticalOptions = LayoutOptions.Center,
            });
            var signup = new PressableScale
            {
                Content = new Label
                {
                    Text = AppText.Login.SignupLink,
                    FontSize = 13,
                    FontFamily = "OpenSansSemibold",
                    TextColor = ThemeColors.Get("Primary"),
                    VerticalOptions = LayoutOptions.Center,
                },
            };
            noAccountRow.Children.Add(signup);

            var loginCard = new Border
            {
                StrokeThickness = 1,
                Stroke = ThemeColors.Get("Border"),
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                BackgroundColor = ThemeColors.Get("Surface"),
                Padding = new Thickness(24, 28),
            };
            var loginFields = new VerticalStackLayout { Spacing = 16 };
            loginFields.Children.Add(demoHint);
            loginFields.Children.Add(_emailInput);
            loginFields.Children.Add(_passwordInput);
            loginFields.Children.Add(_serverErrorLabel);
            loginFields.Children.Add(_loginButton);
            loginFields.Children.Add(forgotRow);
            loginCard.Content = loginFields;

            _credentials = new VerticalStackLayout { Spacing = 24, HorizontalOptions = LayoutOptions.Center, MaximumWidthRequest = 440 };
            _credentials.Children.Add(brand);
            _credentials.Children.Add(loginCard);
            _credentials.Children.Add(noAccountRow);

            // ---- Program selection ----
            var programTitle = new Label
            {
                Text = AppText.Login.SelectProgram,
                FontSize = 26,
                FontFamily = "OpenSansBold",
                TextColor = ThemeColors.Get("Text"),
                HorizontalTextAlignment = TextAlignment.Center,
            };
            var programSubtitle = new Label
            {
                Text = AppText.Login.SelectProgramHint,
                FontSize = 13,
                TextColor = ThemeColors.Get("TextMuted"),
                HorizontalTextAlignment = TextAlignment.Center,
            };
            _selector = new MasterProgramSelectorView(vm.SelectProgram);
            _programErrorLabel = new Label
            {
                FontSize = 13,
                TextColor = ThemeColors.Get("Danger"),
                IsVisible = false,
            };
            _enterButton = new ButtonView
            {
                Text = AppText.Login.EnterProgram,
                Variant = AppButtonVariant.Primary,
                ButtonSize = AppButtonSize.Large,
                IsFullWidth = true,
            };
            _enterButton.Command = vm.SubmitProgramCommand;

            var backButton = new ButtonView
            {
                Text = AppText.Common.Back,
                Variant = AppButtonVariant.Ghost,
                ButtonSize = AppButtonSize.Small,
                HorizontalOptions = LayoutOptions.Center,
            };
            backButton.Command = vm.GoBackCommand;

            var programCard = new Border
            {
                StrokeThickness = 1,
                Stroke = ThemeColors.Get("Border"),
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                BackgroundColor = ThemeColors.Get("Surface"),
                Padding = new Thickness(24, 28),
            };
            var programFields = new VerticalStackLayout { Spacing = 16 };
            programFields.Children.Add(_selector);
            programFields.Children.Add(_programErrorLabel);
            programFields.Children.Add(_enterButton);
            programCard.Content = programFields;

            _program = new VerticalStackLayout { Spacing = 24, HorizontalOptions = LayoutOptions.Center, MaximumWidthRequest = 440, IsVisible = false };
            _program.Children.Add(programTitle);
            _program.Children.Add(programSubtitle);
            _program.Children.Add(programCard);
            _program.Children.Add(backButton);

            ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            RowDefinitions.Add(new RowDefinition(GridLength.Star));
            this.AddAt(_credentials, 0, 0);
            this.AddAt(_program, 0, 0);

            _vm.PropertyChanged += OnVmChanged;
            SyncWithVm();
        }

        private void OnVmChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(LoginViewModel.Stage):
                case nameof(LoginViewModel.IsCredentialsStage):
                case nameof(LoginViewModel.IsProgramStage):
                    SyncWithVm();
                    break;
                case nameof(LoginViewModel.EmailError):
                    _emailInput.ErrorText = _vm.EmailError;
                    break;
                case nameof(LoginViewModel.PasswordError):
                    _passwordInput.ErrorText = _vm.PasswordError;
                    break;
                case nameof(LoginViewModel.ServerError):
                    _serverErrorLabel.Text = _vm.ServerError;
                    _serverErrorLabel.IsVisible = !string.IsNullOrEmpty(_vm.ServerError);
                    break;
                case nameof(LoginViewModel.ProgramError):
                    _programErrorLabel.Text = _vm.ProgramError;
                    _programErrorLabel.IsVisible = !string.IsNullOrEmpty(_vm.ProgramError);
                    break;
                case nameof(LoginViewModel.IsBusy):
                    _loginButton.ShowLoading = _vm.IsBusy;
                    _enterButton.ShowLoading = _vm.IsBusy;
                    break;
                case nameof(LoginViewModel.ShowPassword):
                    _passwordInput.IsPassword = !_vm.ShowPassword;
                    break;
                case nameof(LoginViewModel.ProgramsLoading):
                    if (_vm.ProgramsLoading)
                        _selector.SetPrograms(new List<EuropeanMaui.Models.MasterProgram>(), "");
                    break;
                case nameof(LoginViewModel.HasPrograms):
                case nameof(LoginViewModel.ProgramsError):
                    _selector.SetPrograms(_vm.Programs, _vm.SelectedProgramId ?? "");
                    break;
            }
        }

        private void SyncWithVm()
        {
            bool credentialsStage = _vm.IsCredentialsStage;
            _credentials.IsVisible = credentialsStage;
            _program.IsVisible = !credentialsStage;
            _emailInput.ErrorText = _vm.EmailError;
            _passwordInput.ErrorText = _vm.PasswordError;
            _passwordInput.IsPassword = !_vm.ShowPassword;
            _selector.SetPrograms(_vm.Programs, _vm.SelectedProgramId ?? "");
        }
    }
}