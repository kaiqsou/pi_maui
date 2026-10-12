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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.Carregar();
        }
    }
}
