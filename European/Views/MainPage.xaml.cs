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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            TrocarAbaVisivel();
            RecarregarAba(_viewModel.AbaAtual);
        }

        private void AoAlterarAba()
        {
            TrocarAbaVisivel();
            RecarregarAba(_viewModel.AbaAtual);
        }

        private void RecarregarAba(string aba)
        {
            switch (aba)
            {
                case "inicio": _abaInicio.Recarregar(); break;
                case "aulas": _abaAulas.Recarregar(); break;
                case "provas": _abaProvas.Recarregar(); break;
                case "desempenho": _abaDesempenho.Recarregar(); break;
                case "certificados": _abaCertificados.Recarregar(); break;
            }
        }

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
