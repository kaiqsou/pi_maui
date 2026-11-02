using European.ViewModels;

namespace European.Views
{
    public partial class NovaSenhaPage : ContentPage
    {
        private readonly NovaSenhaViewModel _viewModel = new();

        public NovaSenhaPage()
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
