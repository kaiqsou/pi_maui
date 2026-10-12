using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class ProvaEditPage : ContentPage, IQueryAttributable
    {
        private readonly ProvaEditViewModel _viewModel;

        public ProvaEditPage()
        {
            InitializeComponent();
            _viewModel = new ProvaEditViewModel();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var materia = query.TryGetValue("Materia", out var m) ? m as Materia : null;
            var prova = query.TryGetValue("Prova", out var p) ? p as Prova : null;
            _viewModel.Carregar(materia, prova);
        }
    }
}
