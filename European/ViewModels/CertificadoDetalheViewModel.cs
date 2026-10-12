using System.Windows.Input;
using European.Models;

namespace European.ViewModels
{
    public class CertificadoDetalheViewModel : BaseViewModel
    {
        private Certificado? _certificado;

        public CertificadoDetalheViewModel()
        {
            Title = "Certificado";
            VoltarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public Certificado? Certificado
        {
            get => _certificado;
            set
            {
                if (SetProperty(ref _certificado, value))
                {
                    OnPropertyChanged(nameof(EmissaoTexto));
                }
            }
        }

        public string EmissaoTexto => Certificado == null ? string.Empty : Certificado.EmitidoEm.ToString("dd/MM/yyyy");

        public void Carregar(Certificado certificado)
        {
            Certificado = certificado;
            Title = certificado.Titulo;
        }

        public ICommand VoltarCommand { get; }
    }
}
