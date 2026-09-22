using System.ComponentModel;
using System.Runtime.CompilerServices;
using EuropeanMaui.Models;

namespace EuropeanMaui.Services
{
    /// <summary>
    /// Estado global de autenticação/sessão (equivalente ao useAuthStore do front-end).
    /// </summary>
    public class AuthSession : INotifyPropertyChanged
    {
        private User? _user;
        private bool _isAuthenticated;
        private string? _activeMasterProgramId;

        public event PropertyChangedEventHandler? PropertyChanged;

        public User? User
        {
            get => _user;
            private set => SetField(ref _user, value);
        }

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            private set => SetField(ref _isAuthenticated, value);
        }

        public string? ActiveMasterProgramId
        {
            get => _activeMasterProgramId;
            private set => SetField(ref _activeMasterProgramId, value);
        }

        public void SignIn(User user)
        {
            User = user;
            IsAuthenticated = true;
        }

        public void SetActiveMasterProgram(string id)
        {
            ActiveMasterProgramId = id;
        }

        public void SignOut()
        {
            User = null;
            IsAuthenticated = false;
            ActiveMasterProgramId = null;
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}