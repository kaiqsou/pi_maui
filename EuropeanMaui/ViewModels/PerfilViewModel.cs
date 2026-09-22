using System.Windows.Input;
using EuropeanMaui.Models;
using EuropeanMaui.Services;

namespace EuropeanMaui.ViewModels
{
    public class PerfilViewModel : BaseViewModel
    {
        private readonly AuthSession _auth;
        private readonly ApiService _api;
        private readonly NavigationService _navigation;
        private readonly DialogService _dialog;

        private MasterProgram? _program;

        public PerfilViewModel(
            AuthSession auth,
            LayoutService layout,
            ApiService api,
            NavigationService navigation,
            DialogService dialog)
            : base(layout)
        {
            _auth = auth;
            _api = api;
            _navigation = navigation;
            _dialog = dialog;

            OpenSettingsCommand = new RelayCommand(() => _navigation.Navigate(NavigationKey.Configuracoes));
            LogoutCommand = new AsyncRelayCommand(ConfirmLogoutAsync);

            LoadProgram();
        }

        public User? User => _auth.User;

        public MasterProgram? Program => _program;

        public string RoleLabel
        {
            get
            {
                return User?.Role switch
                {
                    UserRole.Teacher => AppText.Perfil.RoleTeacher,
                    UserRole.Coordinator => AppText.Perfil.RoleCoordinator,
                    UserRole.Admin => AppText.Perfil.RoleAdmin,
                    _ => AppText.Perfil.RoleStudent
                };
            }
        }

        public string Initials =>
            User switch
            {
                null => "?",
                _ => string.Concat(User.Name.Split(' ').Select(p => p.Length > 0 ? p[0].ToString() : "").Take(2)).ToUpperInvariant()
            };

        public ICommand OpenSettingsCommand { get; }
        public ICommand LogoutCommand { get; }

        private async void LoadProgram()
        {
            try
            {
                var programId = _auth.ActiveMasterProgramId;
                var programs = await _api.FetchMasterProgramsAsync();
                _program = string.IsNullOrEmpty(programId)
                    ? programs.FirstOrDefault()
                    : programs.FirstOrDefault(p => p.Id == programId);
            }
            catch
            {
                _program = null;
            }
            finally
            {
                OnPropertyChanged(nameof(Program));
            }
        }

        private async Task ConfirmLogoutAsync()
        {
            bool confirmed = await _dialog.ConfirmAsync(
                AppText.Perfil.LogoutConfirmTitle,
                AppText.Perfil.LogoutConfirmMessage,
                AppText.Perfil.Logout,
                AppText.Common.Cancel);

            if (!confirmed) return;

            _auth.SignOut();
            _navigation.GoToLogin();
        }
    }
}