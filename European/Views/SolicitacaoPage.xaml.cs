using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class SolicitacaoPage : ContentPage, IQueryAttributable
    {
        private readonly SolicitacaoViewModel _viewModel = new();

        public SolicitacaoPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Solicitacao", out var valor) && valor is SolicitacaoMatricula solicitacao)
                await _viewModel.CarregarAsync(solicitacao.Id);
        }
    }
}
