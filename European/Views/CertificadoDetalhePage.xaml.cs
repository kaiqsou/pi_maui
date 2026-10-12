using European.Models;
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
            if (certificado?.UrlPdf is null)
                return;

            try
            {
                await Launcher.Default.OpenAsync(certificado.UrlPdf);
            }
            catch
            {
                await DisplayAlert("Certificado", "Download indisponível no ambiente de teste.", "OK");
            }
        }
    }
}
