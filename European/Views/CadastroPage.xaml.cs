using European.ViewModels;

namespace European.Views
{
    public partial class CadastroPage : ContentPage, IQueryAttributable
    {
        private readonly UserViewModel _viewModel = new();
        private bool _carregado;

        public CadastroPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_carregado)
                return;

            _carregado = true;
            await _viewModel.CarregarCursosAsync();
        }

        /// <summary>Volta da página de detalhes com "Escolher este curso".</summary>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("CursoEscolhidoId", out var valor) && valor is string cursoId)
                _viewModel.SelecionarCursoPorId(cursoId);
            query.Clear();
        }
    }
}
