using European.Services;
using European.ViewModels;

namespace European.Views
{
    /// <summary>Área do aluno: mostra uma aba por vez e recarrega os dados dela sempre que aparece.</summary>
    public partial class MainPage : BasePage
    {
        private readonly MainViewModel _viewModel = new();
        private readonly InicioAba _inicio = new();

        private readonly Dictionary<string, View> _abas;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;

            _abas = new Dictionary<string, View>
            {
                ["inicio"] = _inicio,
                ["aulas"] = new AulasAba(),
                ["provas"] = new ProvasAba(),
                ["desempenho"] = new DesempenhoAba(),
                ["certificados"] = new CertificadosAba()
            };

            _viewModel.AbaAlterada += MostrarAbaAtual;
            ((InicioViewModel)_inicio.BindingContext).AbaSolicitada += _viewModel.SelecionarAba;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MostrarAbaAtual();
        }

        private async void MostrarAbaAtual()
        {
            var aba = _abas.GetValueOrDefault(_viewModel.AbaAtual, _inicio);
            if (!ReferenceEquals(HostAba.Content, aba))
                HostAba.Content = aba;

            try
            {
                if (aba.BindingContext is BaseViewModel viewModel)
                    await viewModel.AoAparecerAsync();
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(MainPage), ex);
            }
        }
    }
}
