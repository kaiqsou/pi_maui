using European.ViewModels;

namespace European.Views
{
    public partial class SomenteCertificadosPage : ContentPage
    {
        private readonly AcessoRestritoViewModel _viewModel = new();

        public SomenteCertificadosPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.Atualizar();
            await Certificados.RecarregarAsync();
        }
    }
}
