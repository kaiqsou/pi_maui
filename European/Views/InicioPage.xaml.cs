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

        public Task RecarregarAsync() => _viewModel.CarregarAsync();
    }
}
