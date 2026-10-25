using European.ViewModels;

namespace European.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _viewModel;
        private readonly InicioPage _abaInicio = new();
        private readonly AulasPage _abaAulas = new();
        private readonly ProvasPage _abaProvas = new();
        private readonly DesempenhoPage _abaDesempenho = new();
        private readonly CertificadosPage _abaCertificados = new();

        public MainPage()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            BindingContext = _viewModel;
            MainViewModel.Atual = _viewModel;
            _viewModel.AbaAlterada += AoAlterarAba;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            TrocarAbaVisivel();
            await RecarregarAbaAsync(_viewModel.AbaAtual);
        }

        private async void AoAlterarAba()
        {
            TrocarAbaVisivel();
            await RecarregarAbaAsync(_viewModel.AbaAtual);
        }

        private Task RecarregarAbaAsync(string aba) => aba switch
        {
            "aulas" => _abaAulas.RecarregarAsync(),
            "provas" => _abaProvas.RecarregarAsync(),
            "desempenho" => _abaDesempenho.RecarregarAsync(),
            "certificados" => _abaCertificados.RecarregarAsync(),
            _ => _abaInicio.RecarregarAsync()
        };

        private void TrocarAbaVisivel()
        {
            View alvo;
            switch (_viewModel.AbaAtual)
            {
                case "aulas": alvo = _abaAulas; break;
                case "provas": alvo = _abaProvas; break;
                case "desempenho": alvo = _abaDesempenho; break;
                case "certificados": alvo = _abaCertificados; break;
                default: alvo = _abaInicio; break;
            }

            if (!ReferenceEquals(HostAba.Content, alvo))
                HostAba.Content = alvo;
        }
    }
}
