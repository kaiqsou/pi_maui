using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class NotificacaoItem
    {
        public Notificacao Notificacao { get; init; } = new();
        public string Titulo => Notificacao.Titulo;
        public string Mensagem => Notificacao.Mensagem;
        public bool Nova => !Notificacao.Lida;

        public string QuandoTexto
        {
            get
            {
                var tempo = DateTime.Now - Notificacao.CriadaEm;
                return tempo.TotalMinutes < 1 ? "agora"
                    : tempo.TotalHours < 1 ? $"há {(int)tempo.TotalMinutes} min"
                    : tempo.TotalDays < 1 ? $"há {(int)tempo.TotalHours} h"
                    : Notificacao.CriadaEm.ToString("dd/MM/yyyy HH:mm");
            }
        }
    }

    /// <summary>Notificações do usuário logado (qualquer perfil). Ao abrir a tela, todas passam a contar como lidas.</summary>
    public partial class NotificacoesViewModel : BaseViewModel
    {
        public NotificacoesViewModel()
        {
            Title = "Notificações";
        }

        public ObservableCollection<NotificacaoItem> Notificacoes { get; } = new();

        [ObservableProperty]
        public partial bool SemNotificacoes { get; private set; }

        public override Task AoAparecerAsync() => ExecutarAsync(async () =>
        {
            var usuario = SessionService.UsuarioAtual;
            Notificacoes.Clear();
            if (usuario != null)
            {
                // as novas continuam destacadas nesta visita; na próxima já aparecem como lidas
                foreach (var notificacao in await NotificacaoRepositorio.DoUsuarioAsync(usuario.Id))
                    Notificacoes.Add(new NotificacaoItem { Notificacao = notificacao });
                await NotificacaoRepositorio.MarcarTodasComoLidasAsync(usuario.Id);
            }

            SemNotificacoes = Notificacoes.Count == 0;
        });
    }
}
