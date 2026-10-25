using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class ProvaRealizarPage : ContentPage, IQueryAttributable
    {
        private readonly ProvaRealizarViewModel _viewModel = new();

        public ProvaRealizarPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Prova", out var valor) && valor is Prova prova)
                await _viewModel.CarregarAsync(prova);
        }

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
