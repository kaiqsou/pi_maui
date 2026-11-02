using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class CorrecaoPage : ContentPage, IQueryAttributable
    {
        private readonly CorrecaoViewModel _viewModel = new();

        public CorrecaoPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Prova", out var p) && p is Prova prova
                && query.TryGetValue("ResultadoId", out var r) && r is string resultadoId)
            {
                var rotulo = query.TryGetValue("Rotulo", out var t) ? t as string ?? "Correção" : "Correção";
                await _viewModel.CarregarAsync(prova, resultadoId, rotulo);
            }
        }
    }
}
