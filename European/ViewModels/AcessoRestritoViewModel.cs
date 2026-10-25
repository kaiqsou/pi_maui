using System.Windows.Input;
using European.Data;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Telas de aluno sem acesso liberado (aguardando) ou ex-aluno (somente certificado).</summary>
    public class AcessoRestritoViewModel : BaseViewModel
    {
        public AcessoRestritoViewModel()
        {
            VerificarCommand = new Command(async () => await VerificarAsync());
            SairCommand = new Command(async () => await AcessoService.SairAsync());
        }

        public string Nome => SessionService.UsuarioAtual?.Nome ?? string.Empty;
        public string Email => SessionService.UsuarioAtual?.Email ?? string.Empty;
        public string Iniciais => SessionService.UsuarioAtual?.Iniciais ?? "?";

        public ICommand VerificarCommand { get; }
        public ICommand SairCommand { get; }

        public void Atualizar()
        {
            OnPropertyChanged(nameof(Nome));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(Iniciais));
        }

        private async Task VerificarAsync()
        {
            var atual = SessionService.UsuarioAtual;
            if (atual == null)
            {
                await AcessoService.SairAsync();
                return;
            }

            await ExecutarAsync(async () =>
            {
                var usuario = await UsuarioRepositorio.PorIdAsync(atual.Id) ?? atual;
                var resultado = await AcessoService.RotaInicialAsync(usuario);

                if (resultado.Rota == AcessoService.RotaAguardando)
                {
                    await AvisarAsync("Acesso", "Seu acesso ainda não foi liberado pela coordenação.");
                    return;
                }

                SessionService.Entrar(usuario);
                if (resultado.Rota != null)
                    await Shell.Current.GoToAsync(resultado.Rota);
            });
        }
    }
}
