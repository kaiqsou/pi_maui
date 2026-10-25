using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class CursoDetalhePage : ContentPage, IQueryAttributable
    {
        private readonly CursoDetalheViewModel _viewModel;
        private string? _cursoId;

        public CursoDetalhePage()
        {
            InitializeComponent();
            _viewModel = new CursoDetalheViewModel();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Curso", out var value) && value is Curso curso)
                _cursoId = curso.Id;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_cursoId != null)
                await _viewModel.CarregarAsync(_cursoId);
        }
    }
}
