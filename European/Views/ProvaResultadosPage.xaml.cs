using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class ProvaResultadosPage : ContentPage, IQueryAttributable
    {
        private readonly ProvaResultadosViewModel _viewModel = new();

        public ProvaResultadosPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Prova", out var valor) && valor is Prova prova)
                await _viewModel.CarregarAsync(prova);
        }
    }
}
