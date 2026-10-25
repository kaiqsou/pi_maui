using System.Collections.ObjectModel;
using System.Windows.Input;
using European.Data;
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

        public Task CarregarAsync() => ExecutarAsync(async () =>
        {
            var aluno = SessionService.UsuarioAtual;
            Certificados = aluno == null
                ? new ObservableCollection<Certificado>()
                : new ObservableCollection<Certificado>(await CertificadoRepositorio.DoAlunoAsync(aluno.Id));

            OnPropertyChanged(nameof(PossuiCertificado));
        });

        private static async Task AbrirAsync(Certificado? certificado)
        {
            if (certificado == null)
                return;

            var parametros = new Dictionary<string, object> { ["Certificado"] = certificado };
            await Shell.Current.GoToAsync("certificado", parametros);
        }
    }
}
