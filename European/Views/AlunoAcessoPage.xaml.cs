using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class AlunoAcessoPage : ContentPage, IQueryAttributable
    {
        private readonly AlunoAcessoViewModel _viewModel = new();

        public AlunoAcessoPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Usuario", out var valor) && valor is Usuario aluno)
                await _viewModel.CarregarAsync(aluno.Id);
        }
    }
}
