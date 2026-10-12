using European.ViewModels;

namespace European.Views
{
    public partial class GerenciarPage : ContentPage
    {
        private readonly GerenciarViewModel _viewModel;

        public GerenciarPage()
        {
            InitializeComponent();
            _viewModel = new GerenciarViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.Carregar();
        }
    }
}
