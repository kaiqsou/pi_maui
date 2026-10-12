using System.Windows.Input;
using European.MockData;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class UserViewModel : BaseViewModel
    {
        private string _nome = string.Empty;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private string _mensagem = string.Empty;
        private Usuario? _usuarioLogado;

        public UserViewModel()
        {
            LoginCommand = new Command(async () => await LoginAsync());
            CadastroCommand = new Command(async () => await CadastrarAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            IrParaCadastroCommand = new Command(async () => await Shell.Current.GoToAsync("cadastro"));
            IrParaLoginCommand = new Command(async () => await Shell.Current.GoToAsync("//login"));
            Title = "Acesso";
        }

        public string Nome
        {
            get => _nome;
            set => SetProperty(ref _nome, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Senha
        {
            get => _senha;
            set => SetProperty(ref _senha, value);
        }

        public string Mensagem
        {
            get => _mensagem;
            set
            {
                if (SetProperty(ref _mensagem, value))
                    OnPropertyChanged(nameof(TemMensagem));
            }
        }

        public bool TemMensagem => !string.IsNullOrWhiteSpace(Mensagem);

        public Usuario? UsuarioLogado
        {
            get => _usuarioLogado;
            set
            {
                if (SetProperty(ref _usuarioLogado, value))
                    OnPropertyChanged(nameof(IsLogado));
            }
        }

        public bool IsLogado => UsuarioLogado != null;

        public ICommand LoginCommand { get; }
        public ICommand CadastroCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand IrParaCadastroCommand { get; }
        public ICommand IrParaLoginCommand { get; }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                Mensagem = "Informe e-mail e senha.";
                return;
            }

            var usuario = MockDataService.Usuarios()
                .FirstOrDefault(u => u.Email.Equals(Email, StringComparison.OrdinalIgnoreCase));

            if (usuario == null || !SenhaService.Verificar(Senha, usuario.SenhaHash))
            {
                Mensagem = "E-mail ou senha inválidos.";
                return;
            }

            UsuarioLogado = usuario;
            SessionService.Entrar(usuario);
            Mensagem = string.Empty;
            await IrParaInicioOuCatalogoAsync(usuario);
        }

        private async Task CadastrarAsync()
        {
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                Mensagem = "Informe nome, e-mail e senha.";
                return;
            }

            if (!SenhaService.EhValida(Senha, out var erroSenha))
            {
                Mensagem = erroSenha;
                return;
            }

            if (MockDataService.Usuarios().Any(u => u.Email.Equals(Email, StringComparison.OrdinalIgnoreCase)))
            {
                Mensagem = "Este e-mail já está cadastrado.";
                return;
            }

            SessionService.DefinirCadastroPendente(new DadosCadastro
            {
                Nome = Nome,
                Email = Email,
                Senha = Senha
            });

            Mensagem = string.Empty;
            await Shell.Current.GoToAsync("catalogo");
        }

        private static async Task IrParaInicioOuCatalogoAsync(Usuario usuario)
        {
            if (usuario.Roles != Enums.Roles.Aluno ||
                MockDataService.Matriculas().Any(m => m.AlunoId == usuario.Id))
            {
                await Shell.Current.GoToAsync("//main");
                return;
            }

            await Shell.Current.GoToAsync("catalogo");
        }

        private async Task LogoutAsync()
        {
            UsuarioLogado = null;
            SessionService.Sair();
            Mensagem = string.Empty;
            await Shell.Current.GoToAsync("//login");
        }
    }
}
