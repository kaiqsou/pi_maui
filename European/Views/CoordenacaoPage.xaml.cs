using European.ViewModels;

namespace European.Views
{
    public partial class CoordenacaoPage : ContentPage
    {
        private readonly CoordenacaoViewModel _viewModel = new();

        public CoordenacaoPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CarregarAsync();
        }
    }
}
