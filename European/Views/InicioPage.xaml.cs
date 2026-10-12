using European.ViewModels;

namespace European.Views
{
    public partial class InicioPage : ContentView
    {
        private readonly InicioViewModel _viewModel;

        public InicioPage()
        {
            InitializeComponent();
            _viewModel = new InicioViewModel();
            BindingContext = _viewModel;
        }

        public void Recarregar() => _viewModel.Carregar();
    }
}
