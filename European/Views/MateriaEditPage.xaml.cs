using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class MateriaEditPage : ContentPage, IQueryAttributable
    {
        private readonly MateriaEditViewModel _viewModel;

        public MateriaEditPage()
        {
            InitializeComponent();
            _viewModel = new MateriaEditViewModel();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var modulo = query.TryGetValue("Modulo", out var m) ? m as Modulo : null;
            var materia = query.TryGetValue("Materia", out var x) ? x as Materia : null;
            _viewModel.Carregar(modulo, materia);
        }
    }
}
