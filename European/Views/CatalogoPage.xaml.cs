using European.ViewModels;

namespace European.Views
{
    public partial class CatalogoPage : ContentPage
    {
        private readonly CatalogoViewModel _viewModel;

        public CatalogoPage()
        {
            InitializeComponent();
            _viewModel = new CatalogoViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.Carregar();
        }
    }
}