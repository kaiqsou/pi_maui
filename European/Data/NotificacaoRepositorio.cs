using European.Models;

namespace European.Data
{
    public static class NotificacaoRepositorio
    {
        /// <summary>As mais recentes primeiro (só as últimas 50).</summary>
        public static Task<List<Notificacao>> DoUsuarioAsync(string usuarioId)
            => Api.GetAsync<List<Notificacao>>($"notificacoes/usuario/{Api.Esc(usuarioId)}");

        public static async Task<int> NaoLidasAsync(string usuarioId)
            => (await Api.GetAsync<Api.RespostaTotal>($"notificacoes/usuario/{Api.Esc(usuarioId)}/nao-lidas")).Total;

        public static Task InserirAsync(IEnumerable<Notificacao> notificacoes)
        {
            var lista = notificacoes.ToList();
            foreach (var notificacao in lista)
                notificacao.Id = Ids.Novo();
            return lista.Count == 0 ? Task.CompletedTask : Api.PostAsync("notificacoes/lote", lista);
        }

        /// <summary>Das chaves informadas, as que o usuário já recebeu (avisos automáticos não se repetem).</summary>
        public static async Task<HashSet<string>> ChavesRecebidasAsync(string usuarioId, IEnumerable<string> chaves)
        {
            var lista = chaves.ToList();
            if (lista.Count == 0)
                return new HashSet<string>();

            return (await Api.PostAsync<List<string>>($"notificacoes/usuario/{Api.Esc(usuarioId)}/chaves", new { chaves = lista })).ToHashSet();
        }

        public static Task MarcarTodasComoLidasAsync(string usuarioId)
            => Api.PostAsync($"notificacoes/usuario/{Api.Esc(usuarioId)}/marcar-lidas", null);
    }
}
