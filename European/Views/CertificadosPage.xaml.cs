using European.ViewModels;

namespace European.Views
{
    public partial class CertificadosPage : ContentView
    {
        private readonly CertificadoViewModel _viewModel;

        public CertificadosPage()
        {
            InitializeComponent();
            _viewModel = new CertificadoViewModel();
            BindingContext = _viewModel;
        }

        public void Recarregar() => _viewModel.Carregar();
    }
}
