using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class PagamentoPage : ContentPage, IQueryAttributable
    {
        private readonly PagamentoViewModel _viewModel;

        public PagamentoPage()
        {
            InitializeComponent();
            _viewModel = new PagamentoViewModel();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Curso", out var curso) && curso is Curso c)
                _viewModel.Carregar(c);
        }
    }
}