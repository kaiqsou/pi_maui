using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Login de usuários e de candidatos (quem pediu matrícula e ainda não foi liberado).</summary>
    public partial class LoginViewModel : BaseViewModel
    {
        public const string SemConexao = "Sem conexão com o banco de dados. Verifique se o MongoDB está em execução.";

        public LoginViewModel()
        {
            Title = "Acesso";
        }

        [ObservableProperty]
        public partial string Email { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Senha { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemMensagem))]
        public partial string Mensagem { get; private set; } = string.Empty;

        public bool TemMensagem => !string.IsNullOrWhiteSpace(Mensagem);

        /// <summary>Banco sem nenhuma conta (instalação nova): mostra "Primeiro acesso" para criar a coordenação.</summary>
        [ObservableProperty]
        public partial bool PrecisaConfigurar { get; private set; }

        public override async Task AoAparecerAsync()
        {
            try
            {
                await MongoContext.InicializarAsync();
                PrecisaConfigurar = !await UsuarioRepositorio.ExisteAlgumUsuarioAsync();
                if (Mensagem == SemConexao)
                    Mensagem = string.Empty;
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(LoginViewModel), ex);
                Mensagem = SemConexao;
            }
        }

        [RelayCommand]
        private Task IrParaCadastro() => Navegacao.IrAsync(Navegacao.Cadastro);

        [RelayCommand]
        private Task IrParaPrimeiroAcesso() => Navegacao.IrAsync(Navegacao.PrimeiroAcesso);

        [RelayCommand]
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

                if (usuario != null && SenhaService.Verificar(Senha, usuario.SenhaHash))
                {
                    await ContaService.AtualizarHashAntigoAsync(usuario, Senha);
                    Mensagem = Senha = string.Empty;
                    await AcessoService.EntrarAsync(usuario);
                    return;
                }

                // ainda não é usuário: pode ser um candidato com solicitação em andamento
                var solicitacao = usuario == null ? await SolicitacaoRepositorio.PorEmailAsync(Email) : null;
                if (solicitacao != null && SenhaService.Verificar(Senha, solicitacao.SenhaHash))
                {
                    await ContaService.AtualizarHashAntigoAsync(solicitacao, Senha);
                    Mensagem = Senha = string.Empty;
                    await AcessoService.EntrarCandidatoAsync(solicitacao);
                    return;
                }

                Mensagem = "E-mail ou senha inválidos.";
            });
        }
    }
}
