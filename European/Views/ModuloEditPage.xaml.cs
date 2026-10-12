using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class ModuloEditPage : ContentPage, IQueryAttributable
    {
        private readonly ModuloEditViewModel _viewModel;

        public ModuloEditPage()
        {
            InitializeComponent();
            _viewModel = new ModuloEditViewModel();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var curso = query.TryGetValue("Curso", out var c) ? c as Curso : null;
            var modulo = query.TryGetValue("Modulo", out var m) ? m as Modulo : null;
            _viewModel.Carregar(curso, modulo);
        }
    }
}
