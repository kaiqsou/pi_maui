using European.ViewModels;

namespace European.Views
{
    public partial class ProfessorPage : ContentPage
    {
        private readonly ProfessorViewModel _viewModel = new();

        public ProfessorPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.CarregarAsync();
        }
    }
}
