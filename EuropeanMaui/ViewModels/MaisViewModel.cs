using System.Collections.ObjectModel;
using System.Windows.Input;
using EuropeanMaui.Services;

namespace EuropeanMaui.ViewModels
{
    public class NavCardItem
    {
        public required string Key { get; init; }
        public required string Label { get; init; }
        public required string Icon { get; init; }
        public required ICommand Command { get; init; }
    }

    public class MaisViewModel : BaseViewModel
    {
        private readonly NavigationService _navigation;

        public MaisViewModel(AuthSession auth, LayoutService layout, ApiService api, NavigationService navigation)
            : base(layout)
        {
            _navigation = navigation;

            Cards = new ObservableCollection<NavCardItem>
            {
                Nav(NavigationKey.Foruns, AppText.Navigation.SidebarForums, Icons.ChatbubblesOutline),
                Nav(NavigationKey.Chat, AppText.Navigation.SidebarChat, Icons.ChatboxEllipsesOutline),
                Nav(NavigationKey.Certificados, AppText.Navigation.SidebarCertificates, Icons.RibbonOutline),
                Nav(NavigationKey.Biblioteca, AppText.Navigation.SidebarLibrary, Icons.LibraryOutline),
            };
        }

        public string Title => AppText.Navigation.NavMore;

        public ObservableCollection<NavCardItem> Cards { get; }

        private NavCardItem Nav(NavigationKey key, string label, string icon) => new()
        {
            Key = key.ToString(),
            Label = label,
            Icon = icon,
            Command = new RelayCommand(() => _navigation.Navigate(key))
        };
    }
}