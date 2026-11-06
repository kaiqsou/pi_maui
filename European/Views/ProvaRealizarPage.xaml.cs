using European.ViewModels;

namespace European.Views
{
    public partial class ProvaRealizarPage : BasePage
    {
        private readonly ProvaRealizarViewModel _viewModel = new();

        public ProvaRealizarPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        /// <summary>Voltar do sistema também passa pela confirmação de envio.</summary>
        protected override bool OnBackButtonPressed()
        {
            _viewModel.SairCommand.Execute(null);
            return true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.Parar();
        }
    }
}
