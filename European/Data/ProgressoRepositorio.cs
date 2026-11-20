using European.Models;
using MongoDB.Driver;

namespace European.Data
{
    public static class ProgressoRepositorio
    {
        private static IMongoCollection<ProgressoAula> Colecao => MongoContext.Progressos;

        public static Task<List<ProgressoAula>> DaMatriculaAsync(string matriculaId)
            => Colecao.Find(p => p.MatriculaId == matriculaId).ToListAsync();

        /// <summary>Progresso já gravado da aula, ou um novo (ainda não salvo).</summary>
        public static async Task<ProgressoAula> ObterOuCriarAsync(string matriculaId, string aulaId)
            => await Colecao.Find(p => p.MatriculaId == matriculaId && p.AulaId == aulaId).FirstOrDefaultAsync()
               ?? new ProgressoAula { Id = MongoContext.NovoId(), MatriculaId = matriculaId, AulaId = aulaId };

        public static async Task SalvarAsync(ProgressoAula progresso)
        {
            progresso.AtualizadoEm = DateTime.Now;
            await Colecao.ReplaceOneAsync(p => p.Id == progresso.Id, progresso, new ReplaceOptions { IsUpsert = true });
        }
    }
}
