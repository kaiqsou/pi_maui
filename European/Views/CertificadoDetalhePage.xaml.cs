using European.Models;
using European.Services;
using European.ViewModels;

namespace European.Views
{
    public partial class CertificadoDetalhePage : ContentPage, IQueryAttributable
    {
        private readonly CertificadoDetalheViewModel _viewModel;

        public CertificadoDetalhePage()
        {
            InitializeComponent();
            _viewModel = new CertificadoDetalheViewModel();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Certificado", out var value) && value is Certificado certificado)
                _viewModel.Carregar(certificado);
        }

        private async void OnBaixarClicked(object sender, EventArgs e)
        {
            var certificado = _viewModel.Certificado;
            if (certificado == null)
                return;

            try
            {
                var caminho = await CertificadoPdf.BaixarAsync(certificado);
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    await DisplayAlert("Certificado", $"PDF salvo em:\n{caminho}", "OK");
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(CertificadoDetalhePage), ex);
                await DisplayAlert("Certificado", $"Não foi possível gerar o PDF: {ex.Message}", "OK");
            }
        }
    }
}
