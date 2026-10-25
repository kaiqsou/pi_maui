using European.Enums;
using European.ViewModels;

namespace European.Views
{
    public partial class ContaNovaPage : ContentPage, IQueryAttributable
    {
        private readonly ContaNovaViewModel _viewModel = new();

        public ContaNovaPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var role = query.TryGetValue("Role", out var r) && r is Roles valor ? valor : Roles.Aluno;
            var email = query.TryGetValue("Email", out var e) ? e as string : null;
            await _viewModel.CarregarAsync(role, email);
        }
    }
}
