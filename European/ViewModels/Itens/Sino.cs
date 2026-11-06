using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Sino de notificações das telas iniciais (aluno, professor e coordenação). Exibido pelo controle SinoNotificacoes.</summary>
    public partial class Sino : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemNaoLidas), nameof(Contador))]
        public partial int NaoLidas { get; private set; }

        public bool TemNaoLidas => NaoLidas > 0;
        public string Contador => NaoLidas > 9 ? "9+" : NaoLidas.ToString();

        /// <summary>Atualiza o contador (chamado sempre que a tela inicial aparece).</summary>
        public async Task AtualizarAsync()
        {
            try
            {
                NaoLidas = SessionService.UsuarioAtual is { } usuario ? await NotificacaoRepositorio.NaoLidasAsync(usuario.Id) : 0;
            }
            catch (Exception ex)
            {
                // sem conexão: o contador só não é atualizado
                CrashLogger.Registrar(nameof(Sino), ex);
            }
        }

        [RelayCommand]
        private Task Abrir() => Navegacao.IrAsync(Navegacao.Notificacoes);
    }
}
