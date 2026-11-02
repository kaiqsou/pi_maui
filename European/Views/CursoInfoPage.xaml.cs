using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class CursoInfoPage : ContentPage, IQueryAttributable
    {
        private readonly CursoInfoViewModel _viewModel = new();

        public CursoInfoPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Curso", out var valor) && valor is Curso curso)
                _viewModel.Carregar(curso);
        }
    }
}
