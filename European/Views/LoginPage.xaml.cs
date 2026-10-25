using European.ViewModels;

namespace European.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly UserViewModel _viewModel;

        public LoginPage()
        {
            InitializeComponent();
            _viewModel = new UserViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.VerificarConfiguracaoAsync();
        }
    }
}
