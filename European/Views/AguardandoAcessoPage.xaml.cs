using European.ViewModels;

namespace European.Views
{
    public partial class AguardandoAcessoPage : ContentPage
    {
        private readonly AcessoRestritoViewModel _viewModel = new();

        public AguardandoAcessoPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.Atualizar();
            await _viewModel.CarregarPedidoAsync();
        }
    }
}
