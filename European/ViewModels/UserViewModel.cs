using System.Net.Mail;
using System.Windows.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class UserViewModel : BaseViewModel
    {
        private string _nome = string.Empty;
        private string _email = string.Empty;
        private string _senha = string.Empty;
        private string _confirmacaoSenha = string.Empty;
        private string _mensagem = string.Empty;
        private bool _precisaConfigurar;

        public UserViewModel()
        {
            LoginCommand = new Command(async () => await LoginAsync());
            CadastroCommand = new Command(async () => await CadastrarAsync(Roles.Aluno));
            ConfigurarCoordenacaoCommand = new Command(async () => await CadastrarAsync(Roles.Coordenador));
            IrParaCadastroCommand = new Command(async () => await Shell.Current.GoToAsync("cadastro"));
            IrParaPrimeiroAcessoCommand = new Command(async () => await Shell.Current.GoToAsync("primeiroacesso"));
            IrParaLoginCommand = new Command(async () => await Shell.Current.GoToAsync("//login"));
            Title = "Acesso";
        }

        public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Senha { get => _senha; set => SetProperty(ref _senha, value); }
        public string ConfirmacaoSenha { get => _confirmacaoSenha; set => SetProperty(ref _confirmacaoSenha, value); }

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

        /// <summary>Verdadeiro enquanto não existe nenhuma conta de coordenação no banco.</summary>
        public bool PrecisaConfigurar
        {
            get => _precisaConfigurar;
            set => SetProperty(ref _precisaConfigurar, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand CadastroCommand { get; }
        public ICommand ConfigurarCoordenacaoCommand { get; }
        public ICommand IrParaCadastroCommand { get; }
        public ICommand IrParaPrimeiroAcessoCommand { get; }
        public ICommand IrParaLoginCommand { get; }

        public async Task VerificarConfiguracaoAsync()
        {
            try
            {
                await MongoContext.InicializarAsync();
                PrecisaConfigurar = !await UsuarioRepositorio.ExisteCoordenacaoAsync();
                if (Mensagem.StartsWith("Sem conexão"))
                    Mensagem = string.Empty;
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(VerificarConfiguracaoAsync), ex);
                Mensagem = "Sem conexão com o banco de dados. Verifique se o MongoDB está em execução.";
            }
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                Mensagem = "Informe e-mail e senha.";
                return;
            }

            await ExecutarAsync(async () =>
            {
                await MongoContext.InicializarAsync();
                var usuario = await UsuarioRepositorio.PorEmailAsync(Email);

                if (usuario == null || !SenhaService.Verificar(Senha, usuario.SenhaHash))
                {
                    Mensagem = "E-mail ou senha inválidos.";
                    return;
                }

                Mensagem = string.Empty;
                Senha = string.Empty;
                await AcessoService.EntrarAsync(usuario);
            });
        }

        private async Task CadastrarAsync(Roles role)
        {
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                Mensagem = "Informe nome, e-mail e senha.";
                return;
            }

            if (!EmailValido(Email))
            {
                Mensagem = "Informe um e-mail válido.";
                return;
            }

            if (!SenhaService.EhValida(Senha, out var erroSenha))
            {
                Mensagem = erroSenha;
                return;
            }

            if (role == Roles.Coordenador && Senha != ConfirmacaoSenha)
            {
                Mensagem = "As senhas não conferem.";
                return;
            }

            await ExecutarAsync(async () =>
            {
                await MongoContext.InicializarAsync();

                // A conta inicial da coordenação só pode ser criada enquanto não houver nenhuma.
                if (role == Roles.Coordenador && await UsuarioRepositorio.ExisteCoordenacaoAsync())
                {
                    Mensagem = "A coordenação já foi configurada. Faça login.";
                    return;
                }

                var usuario = new Usuario
                {
                    Nome = Nome,
                    Email = Email,
                    SenhaHash = SenhaService.GerarHash(Senha),
                    Roles = role,
                    StatusAcesso = role == Roles.Coordenador ? StatusAcesso.Liberado : StatusAcesso.Pendente
                };

                if (!await UsuarioRepositorio.InserirAsync(usuario))
                {
                    Mensagem = "Este e-mail já está cadastrado.";
                    return;
                }

                Mensagem = string.Empty;
                Senha = string.Empty;
                ConfirmacaoSenha = string.Empty;
                await AcessoService.EntrarAsync(usuario);
            });
        }

        public static bool EmailValido(string email)
        {
            try
            {
                return new MailAddress(email.Trim()).Address == email.Trim();
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
