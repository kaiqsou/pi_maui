using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using EuropeanMaui.Models;
using EuropeanMaui.Services;

namespace EuropeanMaui.ViewModels
{
    public enum LoginStage
    {
        Credentials,
        Program
    }

    public class LoginViewModel : BaseViewModel
    {
        private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

        private readonly AuthSession _auth;
        private readonly ApiService _api;
        private readonly NavigationService _navigation;

        private LoginStage _stage = LoginStage.Credentials;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private bool _showPassword;
        private string? _emailError;
        private string? _passwordError;
        private string? _serverError;
        private string? _programError;
        private string? _selectedProgramId;
        private User? _authenticatedUser;
        private bool _programsLoading;
        private bool _programsError;

        public LoginViewModel(AuthSession auth, LayoutService layout, ApiService api, NavigationService navigation)
            : base(layout)
        {
            _auth = auth;
            _api = api;
            _navigation = navigation;

            SubmitCredentialsCommand = new AsyncRelayCommand(SubmitCredentialsAsync);
            SubmitProgramCommand = new AsyncRelayCommand(SubmitProgramAsync);
            GoBackCommand = new RelayCommand(GoBack);
            ToggleShowPasswordCommand = new RelayCommand(() => ShowPassword = !ShowPassword);
        }

        public LoginStage Stage
        {
            get => _stage;
            private set
            {
                if (SetProperty(ref _stage, value))
                {
                    OnPropertyChanged(nameof(IsCredentialsStage));
                    OnPropertyChanged(nameof(IsProgramStage));
                }
            }
        }

        public bool IsCredentialsStage => Stage == LoginStage.Credentials;
        public bool IsProgramStage => Stage == LoginStage.Program;

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    if (ServerError != null) ResetError();
                    if (EmailError != null) EmailError = null;
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    if (ServerError != null) ResetError();
                    if (PasswordError != null) PasswordError = null;
                }
            }
        }

        public bool ShowPassword
        {
            get => _showPassword;
            set => SetProperty(ref _showPassword, value);
        }

        public string? EmailError
        {
            get => _emailError;
            private set => SetProperty(ref _emailError, value);
        }

        public string? PasswordError
        {
            get => _passwordError;
            private set => SetProperty(ref _passwordError, value);
        }

        public string? ServerError
        {
            get => _serverError;
            private set => SetProperty(ref _serverError, value);
        }

        public string? ProgramError
        {
            get => _programError;
            private set => SetProperty(ref _programError, value);
        }

        public string? SelectedProgramId
        {
            get => _selectedProgramId;
            private set => SetProperty(ref _selectedProgramId, value);
        }

        public bool ProgramsLoading
        {
            get => _programsLoading;
            private set => SetProperty(ref _programsLoading, value);
        }

        public bool ProgramsError
        {
            get => _programsError;
            private set => SetProperty(ref _programsError, value);
        }

        public ObservableCollection<MasterProgram> Programs { get; } = new();

        public bool HasPrograms => Programs.Count > 0;

        public bool IsLoading => IsBusy;

        public ICommand SubmitCredentialsCommand { get; }
        public ICommand SubmitProgramCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand ToggleShowPasswordCommand { get; }

        private bool ValidateCredentials()
        {
            var email = Email.Trim();
            var errors = false;

            if (string.IsNullOrEmpty(email))
            {
                EmailError = AppText.Login.ErrorEmailRequired;
                errors = true;
            }
            else if (!EmailRegex.IsMatch(email))
            {
                EmailError = AppText.Login.ErrorEmailInvalid;
                errors = true;
            }
            else
            {
                EmailError = null;
            }

            if (string.IsNullOrEmpty(Password))
            {
                PasswordError = AppText.Login.ErrorPasswordRequired;
                errors = true;
            }
            else if (Password.Length < 6)
            {
                PasswordError = AppText.Login.ErrorPasswordMin;
                errors = true;
            }
            else
            {
                PasswordError = null;
            }

            return !errors;
        }

        private async Task SubmitCredentialsAsync()
        {
            if (!ValidateCredentials()) return;

            ProgramError = null;
            IsBusy = true;
            ServerError = null;
            try
            {
                var result = await _api.LoginAsync(Email.Trim(), Password);
                if (result.User.MasterProgramIds.Count == 1)
                {
                    _auth.SetActiveMasterProgram(result.User.MasterProgramIds[0]);
                    _auth.SignIn(result.User);
                    _navigation.Replace(NavigationKey.Dashboard);
                    return;
                }

                _authenticatedUser = result.User;
                SelectedProgramId = result.User.MasterProgramIds.FirstOrDefault();
                Stage = LoginStage.Program;
                await LoadProgramsAsync(_authenticatedUser.MasterProgramIds);
            }
            catch
            {
                ServerError = AppText.Login.ServerError;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadProgramsAsync(List<string> ids)
        {
            ProgramsLoading = true;
            ProgramsError = false;
            try
            {
                var list = await _api.FetchMyMasterProgramsAsync(ids);
                Programs.Clear();
                foreach (var p in list) Programs.Add(p);
                OnPropertyChanged(nameof(HasPrograms));
            }
            catch
            {
                ProgramsError = true;
            }
            finally
            {
                ProgramsLoading = false;
            }
        }

        public void RetryPrograms()
        {
            if (_authenticatedUser != null)
                _ = LoadProgramsAsync(_authenticatedUser.MasterProgramIds);
        }

        public void SelectProgram(string id)
        {
            SelectedProgramId = id;
            ProgramError = null;
        }

        private async Task SubmitProgramAsync()
        {
            if (SelectedProgramId == null || _authenticatedUser == null)
            {
                ProgramError = AppText.Login.SelectProgram;
                return;
            }

            _auth.SetActiveMasterProgram(SelectedProgramId);
            _auth.SignIn(_authenticatedUser);
            _navigation.Replace(NavigationKey.Dashboard);
            await Task.CompletedTask;
        }

        private void GoBack()
        {
            Stage = LoginStage.Credentials;
            Programs.Clear();
            ProgramError = null;
            _authenticatedUser = null;
        }

        private void ResetError()
        {
            ServerError = null;
        }
    }
}