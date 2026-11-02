using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class ProvaResultadosPage : ContentPage, IQueryAttributable
    {
        private readonly ProvaResultadosViewModel _viewModel = new();
        private bool _carregado;

        public ProvaResultadosPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Prova", out var valor) && valor is Prova prova)
            {
                _carregado = true;
                await _viewModel.CarregarAsync(prova);
            }
            query.Clear();
        }

        /// <summary>Volta da correção de uma entrega: atualiza a lista (e revela os nomes se acabou).</summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_carregado)
                await _viewModel.RecarregarAsync();
        }
    }
}
