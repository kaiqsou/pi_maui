using European.ViewModels;

namespace European.Views
{
    public partial class ProvasPage : ContentView
    {
        private readonly ProvasViewModel _viewModel;

        public ProvasPage()
        {
            InitializeComponent();
            _viewModel = new ProvasViewModel();
            BindingContext = _viewModel;
        }

        public void Recarregar() => _viewModel.Carregar();
    }
}
