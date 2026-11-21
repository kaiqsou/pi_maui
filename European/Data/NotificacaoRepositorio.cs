using European.Models;
using MongoDB.Driver;

namespace European.Data
{
    public static class NotificacaoRepositorio
    {
        private const int Limite = 50;

        private static IMongoCollection<Notificacao> Colecao => MongoContext.Notificacoes;

        /// <summary>As mais recentes primeiro (só as últimas 50).</summary>
        public static Task<List<Notificacao>> DoUsuarioAsync(string usuarioId)
            => Colecao.Find(n => n.UsuarioId == usuarioId).SortByDescending(n => n.CriadaEm).Limit(Limite).ToListAsync();

        public static async Task<int> NaoLidasAsync(string usuarioId)
            => (int)await Colecao.CountDocumentsAsync(n => n.UsuarioId == usuarioId && !n.Lida);

        public static Task InserirAsync(IEnumerable<Notificacao> notificacoes)
        {
            var lista = notificacoes.ToList();
            foreach (var notificacao in lista)
                notificacao.Id = MongoContext.NovoId();
            return lista.Count == 0 ? Task.CompletedTask : Colecao.InsertManyAsync(lista);
        }

        /// <summary>Das chaves informadas, as que o usuário já recebeu (avisos automáticos não se repetem).</summary>
        public static async Task<HashSet<string>> ChavesRecebidasAsync(string usuarioId, IEnumerable<string> chaves)
        {
            var filtro = Builders<Notificacao>.Filter.Eq(n => n.UsuarioId, usuarioId)
                         & Builders<Notificacao>.Filter.In(n => n.Chave, chaves.ToList());
            return (await Colecao.Find(filtro).Project(n => n.Chave).ToListAsync()).OfType<string>().ToHashSet();
        }

        public static Task MarcarTodasComoLidasAsync(string usuarioId)
            => Colecao.UpdateManyAsync(n => n.UsuarioId == usuarioId && !n.Lida, Builders<Notificacao>.Update.Set(n => n.Lida, true));
    }
}
