using European.ViewModels;

namespace European.Views
{
    public partial class AulasPage : ContentView
    {
        private readonly AulasViewModel _viewModel;

        public AulasPage()
        {
            InitializeComponent();
            _viewModel = new AulasViewModel();
            BindingContext = _viewModel;
        }

        public void Recarregar() => _viewModel.Carregar();
    }
}
