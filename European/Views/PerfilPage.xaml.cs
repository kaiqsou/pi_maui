using European.ViewModels;

namespace European.Views
{
    public partial class PerfilPage : ContentPage
    {
        private readonly PerfilViewModel _viewModel;

        public PerfilPage()
        {
            InitializeComponent();
            _viewModel = new PerfilViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CarregarAsync();
        }
    }
}
