using European.Models;
using European.ViewModels;

namespace European.Views
{
    public partial class ProfessorAcessoPage : ContentPage, IQueryAttributable
    {
        private readonly ProfessorAcessoViewModel _viewModel = new();

        public ProfessorAcessoPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Usuario", out var valor) && valor is Usuario professor)
                await _viewModel.CarregarAsync(professor.Id);
        }
    }
}
