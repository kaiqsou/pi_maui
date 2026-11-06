using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public partial class CertificadoDetalheViewModel : BaseViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EmissaoTexto))]
        public partial Certificado? Certificado { get; set; }

        public string EmissaoTexto => Certificado?.EmitidoEm.ToString("dd/MM/yyyy") ?? string.Empty;

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            if (parametros.Obter<Certificado>("Certificado") is { } certificado)
            {
                Certificado = certificado;
                Title = certificado.Titulo;
            }
            return Task.CompletedTask;
        }


        [RelayCommand]
        private async Task BaixarAsync()
        {
            if (Certificado == null)
                return;

            try
            {
                var caminho = await CertificadoPdf.BaixarAsync(Certificado);
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    await AvisarAsync("Certificado", $"PDF salvo em:\n{caminho}");
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(CertificadoDetalheViewModel), ex);
                await AvisarAsync("Certificado", $"Não foi possível gerar o PDF: {ex.Message}");
            }
        }
    }
}
