using System.Windows.Input;
using European.Data;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Primeiro login com senha provisória (criada pela coordenação): o usuário define a própria senha.</summary>
    public class NovaSenhaViewModel : BaseViewModel
    {
        private string _novaSenha = string.Empty;
        private string _confirmacao = string.Empty;
        private string _mensagem = string.Empty;

        public NovaSenhaViewModel()
        {
            SalvarCommand = new Command(async () => await SalvarAsync());
            SairCommand = new Command(async () => await AcessoService.SairAsync());
        }

        public string PrimeiroNome => SessionService.NomeAtual.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        public string NovaSenha { get => _novaSenha; set => SetProperty(ref _novaSenha, value); }
        public string Confirmacao { get => _confirmacao; set => SetProperty(ref _confirmacao, value); }

        public string Mensagem
        {
            get => _mensagem;
            private set
            {
                if (SetProperty(ref _mensagem, value))
                    OnPropertyChanged(nameof(TemMensagem));
            }
        }

        public bool TemMensagem => !string.IsNullOrEmpty(Mensagem);

        public ICommand SalvarCommand { get; }
        public ICommand SairCommand { get; }

        public void Atualizar()
        {
            NovaSenha = string.Empty;
            Confirmacao = string.Empty;
            Mensagem = string.Empty;
            OnPropertyChanged(nameof(PrimeiroNome));
        }

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
                await Shell.Current.GoToAsync(resultado.Rota ?? "//login");
            });
        }
    }
}
