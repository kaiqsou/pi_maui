using European.ViewModels;

namespace European.Views
{
    public partial class DesempenhoPage : ContentView
    {
        private readonly DesempenhoViewModel _viewModel;

        public DesempenhoPage()
        {
            InitializeComponent();
            _viewModel = new DesempenhoViewModel();
            BindingContext = _viewModel;
        }

        public void Recarregar() => _viewModel.Carregar();
    }
}
