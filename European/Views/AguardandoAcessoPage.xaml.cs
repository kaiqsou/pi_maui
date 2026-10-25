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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.Atualizar();
        }
    }
}
