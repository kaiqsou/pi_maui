using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class CursoEditPage : ContentPage, IQueryAttributable
    {
        private readonly CursoEditViewModel _viewModel;

        public CursoEditPage()
        {
            InitializeComponent();
            _viewModel = new CursoEditViewModel();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var curso = query.TryGetValue("Curso", out var value) ? value as Curso : null;
            _viewModel.Carregar(curso);
        }
    }
}
