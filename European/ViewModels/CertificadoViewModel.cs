using System.Collections.ObjectModel;
using System.Windows.Input;
using European.MockData;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class CertificadoViewModel : BaseViewModel
    {
        private ObservableCollection<Certificado> _certificados = new();

        public CertificadoViewModel()
        {
            Title = "Certificados";
            AbrirCommand = new Command<Certificado>(async c => await AbrirAsync(c));
        }

        public ObservableCollection<Certificado> Certificados
        {
            get => _certificados;
            private set => SetProperty(ref _certificados, value);
        }

        public bool PossuiCertificado => Certificados.Count > 0;

        public ICommand AbrirCommand { get; }

        public void Carregar()
        {
            var aluno = SessionService.UsuarioAtual;
            var certificados = new ObservableCollection<Certificado>();

            if (aluno != null)
            {
                foreach (var certificado in MockDataService.CertificadosDoAluno(aluno.Id))
                    certificados.Add(certificado);
            }

            Certificados = certificados;
            OnPropertyChanged(nameof(PossuiCertificado));
        }

        private static async Task AbrirAsync(Certificado? certificado)
        {
            if (certificado == null)
                return;

            var parametros = new Dictionary<string, object> { ["Certificado"] = certificado };
            await Shell.Current.GoToAsync("certificado", parametros);
        }
    }
}
