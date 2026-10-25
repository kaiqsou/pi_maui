using System.Windows.Input;
using European.Data;
using European.Enums;
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
            LogoutCommand = new Command(async () => await AcessoService.SairAsync());
            VoltarCommand = new Command(async () => await VoltarAsync());
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
                    OnPropertyChanged(nameof(IsAluno));
                }
            }
        }

        public string Nome => Usuario?.Nome ?? string.Empty;
        public string Email => Usuario?.Email ?? string.Empty;
        public string RoleTexto => Usuario?.Roles switch
        {
            Roles.Coordenador or Roles.Administrador => "Coordenação",
            Roles.Professor => "Professor",
            Roles.Aluno => "Aluno",
            _ => string.Empty
        };

        public string Iniciais => Usuario?.Iniciais ?? "?";
        public bool IsAluno => Usuario?.Roles == Roles.Aluno;

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
        public ICommand VoltarCommand { get; }

        public Task CarregarAsync() => ExecutarAsync(async () =>
        {
            Usuario = SessionService.UsuarioAtual;
            if (Usuario == null || !IsAluno)
                return;

            TotalCursos = (await MatriculaRepositorio.AtivasDoAlunoAsync(Usuario.Id)).Count;
            TotalCertificados = (await CertificadoRepositorio.DoAlunoAsync(Usuario.Id)).Count;
        });
    }
}
