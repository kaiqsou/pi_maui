using System.Windows.Input;
using European.Enums;
using European.MockData;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class PerfilViewModel : BaseViewModel
    {
        private Usuario? _usuario;
        private int _totalCursos;
        private int _totalCertificados;

        public PerfilViewModel()
        {
            Title = "Perfil";
            LogoutCommand = new Command(async () =>
            {
                SessionService.Sair();
                await Shell.Current.GoToAsync("//login");
            });
            GerenciarCommand = new Command(async () => await Shell.Current.GoToAsync("gerenciar"));
            VoltarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public Usuario? Usuario
        {
            get => _usuario;
            set
            {
                if (SetProperty(ref _usuario, value))
                {
                    OnPropertyChanged(nameof(Nome));
                    OnPropertyChanged(nameof(Email));
                    OnPropertyChanged(nameof(RoleTexto));
                    OnPropertyChanged(nameof(Iniciais));
                    OnPropertyChanged(nameof(PodeGerenciar));
                }
            }
        }

        public string Nome => Usuario?.Nome ?? string.Empty;
        public string Email => Usuario?.Email ?? string.Empty;
        public string RoleTexto => Usuario?.Roles.ToString() ?? string.Empty;

        public string Iniciais
        {
            get
            {
                var partes = (Usuario?.Nome ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return partes.Length switch
                {
                    0 => "?",
                    1 => partes[0][..1].ToUpper(),
                    _ => $"{partes[0][..1]}{partes[^1][..1]}".ToUpper()
                };
            }
        }

        public bool PodeGerenciar => Usuario is { Roles: Roles.Administrador or Roles.Professor or Roles.Coordenador };

        public int TotalCursos
        {
            get => _totalCursos;
            set => SetProperty(ref _totalCursos, value);
        }

        public int TotalCertificados
        {
            get => _totalCertificados;
            set => SetProperty(ref _totalCertificados, value);
        }

        public ICommand LogoutCommand { get; }
        public ICommand GerenciarCommand { get; }
        public ICommand VoltarCommand { get; }

        public void Carregar()
        {
            Usuario = SessionService.UsuarioAtual;
            if (Usuario == null)
                return;

            TotalCursos = MockDataService.Matriculas().Count(m => m.AlunoId == Usuario.Id);
            TotalCertificados = MockDataService.CertificadosDoAluno(Usuario.Id).Count;
        }
    }
}
