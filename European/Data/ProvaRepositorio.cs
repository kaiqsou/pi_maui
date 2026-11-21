using European.Models;
using MongoDB.Driver;

namespace European.Data
{
    public static class ProvaRepositorio
    {
        private static IMongoCollection<Prova> Colecao => MongoContext.Provas;

        public static Task<Prova?> PorIdAsync(string id) => Colecao.PorIdAsync(id);

        public static Task<List<Prova>> PorIdsAsync(IEnumerable<string> ids) => Colecao.PorIdsAsync(ids);

        public static Task<List<Prova>> DoProfessorAsync(string professorId)
            => Colecao.Find(p => p.ProfessorId == professorId).SortByDescending(p => p.UpdatedAt).ToListAsync();

        public static Task<List<Prova>> DosCursosAsync(IEnumerable<string> cursoIds)
            => Colecao.OndeEmAsync(p => p.CursoId, cursoIds);

        public static Task SalvarAsync(Prova prova) => Colecao.SalvarAsync(prova);

        /// <summary>Grava a nova ordem das provas de um curso (1, 2, 3... na ordem da lista).</summary>
        public static Task SalvarOrdemAsync(IReadOnlyList<Prova> provas)
        {
            if (provas.Count == 0)
                return Task.CompletedTask;

            var atualizacoes = provas.Select((p, i) => new UpdateOneModel<Prova>(
                Builders<Prova>.Filter.Eq(x => x.Id, p.Id), Builders<Prova>.Update.Set(x => x.Ordem, i + 1)));
            return Colecao.BulkWriteAsync(atualizacoes);
        }

        public static async Task ExcluirAsync(string provaId)
        {
            await MongoContext.Resultados.DeleteManyAsync(r => r.ProvaId == provaId);
            await Colecao.ExcluirAsync(provaId);
        }

        public static async Task ExcluirDaMateriaAsync(string materiaId)
        {
            var ids = await Colecao.Find(p => p.MateriaId == materiaId).Project(p => p.Id).ToListAsync();
            await MongoContext.Resultados.DeleteManyAsync(Builders<ResultadoProva>.Filter.In(r => r.ProvaId, ids));
            await Colecao.DeleteManyAsync(p => p.MateriaId == materiaId);
        }
    }
}
