using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class SalaEditPage : ContentPage, IQueryAttributable
    {
        private readonly SalaEditViewModel _viewModel = new();

        public SalaEditPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
            => await _viewModel.CarregarAsync(query.TryGetValue("Sala", out var valor) ? valor as Sala : null);
    }
}
