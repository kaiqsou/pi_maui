using European.Services;
using European.ViewModels;

namespace European.Views
{
    /// <summary>Ex-aluno: só consulta o certificado (a aba de certificados do aluno é reaproveitada aqui).</summary>
    public partial class SomenteCertificadosPage : BasePage
    {
        public SomenteCertificadosPage()
        {
            InitializeComponent();
            BindingContext = new AcessoRestritoViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                if (Certificados.BindingContext is BaseViewModel aba)
                    await aba.AoAparecerAsync();
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(SomenteCertificadosPage), ex);
            }
        }
    }
}
