using System.Windows.Input;
using EuropeanMaui.Services;

namespace EuropeanMaui.ViewModels
{
    public class PlaceholderViewModel : BaseViewModel
    {
        private readonly NavigationService _navigation;

        public PlaceholderViewModel(LayoutService layout, NavigationService navigation, string title, string message)
            : base(layout)
        {
            _navigation = navigation;

            Title = title;
            Message = message;

            BackCommand = new RelayCommand(() => _navigation.GoBack());
        }

        public string Title { get; }
        public string Message { get; }

        public string HomeLabel => AppText.Navigation.Home;
        public string BackLabel => AppText.Common.Back;

        public ICommand BackCommand { get; }
    }
}