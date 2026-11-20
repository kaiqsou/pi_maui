using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Primeiro login com senha provisória (criada pela coordenação): o usuário define a própria senha.</summary>
    public partial class NovaSenhaViewModel : BaseViewModel
    {
        [ObservableProperty]
        public partial string NovaSenha { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Confirmacao { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemMensagem))]
        public partial string Mensagem { get; private set; } = string.Empty;

        public bool TemMensagem => !string.IsNullOrEmpty(Mensagem);
        public string PrimeiroNome => Formatos.PrimeiroNome(SessionService.NomeAtual);

        public override Task AoAparecerAsync()
        {
            NovaSenha = string.Empty;
            Confirmacao = string.Empty;
            Mensagem = string.Empty;
            OnPropertyChanged(nameof(PrimeiroNome));
            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task Sair() => AcessoService.SairAsync();

        [RelayCommand]
        private async Task SalvarAsync()
        {
            var usuario = SessionService.UsuarioAtual;
            if (usuario == null)
            {
                await AcessoService.SairAsync();
                return;
            }

            if (!SenhaService.EhValida(NovaSenha, out var erro))
            {
                Mensagem = erro;
                return;
            }

            if (NovaSenha != Confirmacao)
            {
                Mensagem = "A confirmação não confere com a nova senha.";
                return;
            }

            if (SenhaService.Verificar(NovaSenha, usuario.SenhaHash))
            {
                Mensagem = "Escolha uma senha diferente da provisória.";
                return;
            }

            Mensagem = string.Empty;
            await ExecutarAsync(async () =>
            {
                var hash = SenhaService.GerarHash(NovaSenha);
                await UsuarioRepositorio.AtualizarSenhaAsync(usuario.Id, hash);
                usuario.SenhaHash = hash;
                usuario.SenhaProvisoria = false;

                // segue para a área do usuário, como num login normal
                var resultado = await AcessoService.RotaInicialAsync(usuario);
                NovaSenha = string.Empty;
                Confirmacao = string.Empty;
                await AvisarAsync("Senha", "Sua senha foi definida.");
                await Shell.Current.GoToAsync(resultado.Rota ?? Navegacao.Login);
            });
        }
    }
}
