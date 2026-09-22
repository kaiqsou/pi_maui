using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EuropeanMaui.Services
{
    public enum NavigationKey
    {
        Login,
        Dashboard,
        Modulos,
        Provas,
        Foruns,
        Chat,
        Certificados,
        Biblioteca,
        Mais,
        Perfil,
        Configuracoes
    }

    /// <summary>
    /// Controla a página exibida no host de conteúdo (equivalente ao expo-router).
    /// </summary>
    public class NavigationService : INotifyPropertyChanged
    {
        private readonly AuthSession _auth;
        private readonly LayoutService _layout;
        private readonly ApiService _api;
        private readonly ThemeSession _theme;
        private readonly DialogService _dialog;

        public event PropertyChangedEventHandler? PropertyChanged;

        private View? _currentView;
        private NavigationKey _currentKey;
        private readonly Stack<NavigationKey> _history = new();

        public NavigationService(
            AuthSession auth,
            LayoutService layout,
            ApiService api,
            ThemeSession theme,
            DialogService dialog)
        {
            _auth = auth;
            _layout = layout;
            _api = api;
            _theme = theme;
            _dialog = dialog;
        }

        public View? CurrentView
        {
            get => _currentView;
            private set
            {
                if (ReferenceEquals(_currentView, value)) return;
                _currentView = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentView)));
            }
        }

        public NavigationKey CurrentKey => _currentKey;

        public void Navigate(NavigationKey key)
        {
            _history.Push(_currentKey);
            Show(key);
        }

        public void Replace(NavigationKey key)
        {
            _history.Clear();
            Show(key);
        }

        public void GoBack()
        {
            if (_history.Count == 0)
            {
                Show(NavigationKey.Dashboard);
                return;
            }
            Show(_history.Pop());
        }

        public void GoToLogin()
        {
            _history.Clear();
            Show(NavigationKey.Login);
        }

        private void Show(NavigationKey key)
        {
            _currentKey = key;
            CurrentView = key switch
            {
                NavigationKey.Login => new Views.LoginView(new ViewModels.LoginViewModel(_auth, _layout, _api, this)),
                NavigationKey.Dashboard => new Views.DashboardView(new ViewModels.DashboardViewModel(_auth, _layout, _api, this)),
                NavigationKey.Modulos => new Views.ModulosView(new ViewModels.ModulosViewModel(_auth, _layout, _api)),
                NavigationKey.Provas => Placeholder(AppText.Placeholders.ExamsTitle, AppText.Placeholders.ExamsMessage),
                NavigationKey.Foruns => Placeholder(AppText.Placeholders.ForumsTitle, AppText.Placeholders.ForumsMessage),
                NavigationKey.Chat => Placeholder(AppText.Placeholders.ChatTitle, AppText.Placeholders.ChatMessage),
                NavigationKey.Certificados => Placeholder(AppText.Placeholders.CertificatesTitle, AppText.Placeholders.CertificatesMessage),
                NavigationKey.Biblioteca => Placeholder(AppText.Placeholders.LibraryTitle, AppText.Placeholders.LibraryMessage),
                NavigationKey.Mais => new Views.MaisView(new ViewModels.MaisViewModel(_auth, _layout, _api, this)),
                NavigationKey.Perfil => new Views.PerfilView(new ViewModels.PerfilViewModel(_auth, _layout, _api, this, _dialog)),
                NavigationKey.Configuracoes => new Views.ConfiguracoesView(new ViewModels.ConfiguracoesViewModel(_theme, _layout)),
                _ => new Views.LoginView(new ViewModels.LoginViewModel(_auth, _layout, _api, this))
            };

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentKey)));
        }

        private View Placeholder(string title, string message)
            => new Views.PlaceholderView(new ViewModels.PlaceholderViewModel(_layout, this, title, message));
    }
}