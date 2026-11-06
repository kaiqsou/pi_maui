using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Cria a primeira conta da coordenação (só é permitido enquanto não existe nenhuma).</summary>
    public partial class PrimeiroAcessoViewModel : BaseViewModel
    {
        [ObservableProperty]
        public partial string Nome { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Email { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Senha { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ConfirmacaoSenha { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemMensagem))]
        public partial string Mensagem { get; private set; } = string.Empty;

        public bool TemMensagem => !string.IsNullOrWhiteSpace(Mensagem);

        [RelayCommand]
        private Task IrParaLogin() => Navegacao.IrAsync(Navegacao.Login);

        [RelayCommand]
        private async Task ConfigurarCoordenacaoAsync()
        {
            Mensagem = ContaService.ValidarDados(Nome, Email, Senha)
                       ?? (Senha != ConfirmacaoSenha ? "As senhas não conferem." : string.Empty);
            if (TemMensagem)
                return;

            await ExecutarAsync(async () =>
            {
                await MongoContext.InicializarAsync();

                // a conta inicial da coordenação só pode ser criada enquanto não houver nenhuma
                if (await UsuarioRepositorio.ExisteCoordenacaoAsync())
                {
                    Mensagem = "A coordenação já foi configurada. Faça login.";
                    return;
                }

                var usuario = new Usuario
                {
                    Nome = Nome,
                    Email = Email,
                    SenhaHash = SenhaService.GerarHash(Senha),
                    Roles = Roles.Coordenador,
                    StatusAcesso = StatusAcesso.Liberado
                };

                if (!await UsuarioRepositorio.InserirAsync(usuario))
                {
                    Mensagem = "Este e-mail já está cadastrado.";
                    return;
                }

                Senha = ConfirmacaoSenha = string.Empty;
                await AcessoService.EntrarAsync(usuario);
            });
        }
    }
}
