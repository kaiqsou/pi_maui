using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class AulaEditPage : ContentPage, IQueryAttributable
    {
        private readonly AulaEditViewModel _viewModel;

        public AulaEditPage()
        {
            InitializeComponent();
            _viewModel = new AulaEditViewModel();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var materia = query.TryGetValue("Materia", out var m) ? m as Materia : null;
            var aula = query.TryGetValue("Aula", out var a) ? a as Aula : null;
            _viewModel.Carregar(materia, aula);
        }
    }
}
