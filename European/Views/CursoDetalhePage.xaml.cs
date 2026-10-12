using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class CursoDetalhePage : ContentPage, IQueryAttributable
    {
        private readonly CursoDetalheViewModel _viewModel;

        public CursoDetalhePage()
        {
            InitializeComponent();
            _viewModel = new CursoDetalheViewModel();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Curso", out var value) && value is Curso curso)
                _viewModel.Carregar(curso);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.Curso != null)
                _viewModel.Carregar(_viewModel.Curso);
        }
    }
}
