using European.ViewModels;

namespace European.Views
{
    public partial class PagamentoPage : ContentPage
    {
        private readonly PagamentoViewModel _viewModel = new();

        public PagamentoPage()
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
