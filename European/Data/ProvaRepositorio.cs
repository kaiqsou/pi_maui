using European.Models;
using MongoDB.Driver;

namespace European.Data
{
    public static class ProvaRepositorio
    {
        private static IMongoCollection<Prova> Colecao => MongoContext.Provas;

        public static async Task<Prova?> PorIdAsync(string id)
            => await Colecao.Find(p => p.Id == id).FirstOrDefaultAsync();

        public static async Task<List<Prova>> DoProfessorAsync(string professorId)
            => await Colecao.Find(p => p.ProfessorId == professorId).SortByDescending(p => p.UpdatedAt).ToListAsync();

        public static async Task<List<Prova>> DosCursosAsync(IEnumerable<string> cursoIds)
        {
            var ids = cursoIds.Distinct().ToList();
            return await Colecao.Find(Builders<Prova>.Filter.In(p => p.CursoId, ids)).ToListAsync();
        }

        public static async Task SalvarAsync(Prova prova)
        {
            if (string.IsNullOrEmpty(prova.Id))
            {
                prova.Id = MongoContext.NovoId();
                prova.CreatedAt = DateTime.Now;
            }

            prova.UpdatedAt = DateTime.Now;
            await Colecao.ReplaceOneAsync(p => p.Id == prova.Id, prova, new ReplaceOptions { IsUpsert = true });
        }

        public static async Task ExcluirAsync(string provaId)
        {
            await MongoContext.Resultados.DeleteManyAsync(r => r.ProvaId == provaId);
            await Colecao.DeleteOneAsync(p => p.Id == provaId);
        }

        public static async Task ExcluirDaMateriaAsync(string materiaId)
        {
            var ids = await Colecao.Find(p => p.MateriaId == materiaId).Project(p => p.Id).ToListAsync();
            await MongoContext.Resultados.DeleteManyAsync(Builders<ResultadoProva>.Filter.In(r => r.ProvaId, ids));
            await Colecao.DeleteManyAsync(p => p.MateriaId == materiaId);
        }
    }

    public static class ResultadoRepositorio
    {
        private static IMongoCollection<ResultadoProva> Colecao => MongoContext.Resultados;

        public static async Task<List<ResultadoProva>> DoAlunoAsync(string alunoId)
            => await Colecao.Find(r => r.AlunoId == alunoId).SortByDescending(r => r.DataRealizacao).ToListAsync();

        public static async Task<List<ResultadoProva>> DaProvaAsync(string provaId)
            => await Colecao.Find(r => r.ProvaId == provaId).SortByDescending(r => r.NotaObtida).ToListAsync();

        public static async Task<List<ResultadoProva>> DasProvasAsync(IEnumerable<string> provaIds)
        {
            var ids = provaIds.Distinct().ToList();
            return await Colecao.Find(Builders<ResultadoProva>.Filter.In(r => r.ProvaId, ids)).ToListAsync();
        }

        public static async Task<ResultadoProva?> PorIdAsync(string id)
            => await Colecao.Find(r => r.Id == id).FirstOrDefaultAsync();

        public static async Task AtualizarAsync(ResultadoProva resultado)
            => await Colecao.ReplaceOneAsync(r => r.Id == resultado.Id, resultado);

        public static async Task<bool> ExisteAsync(string alunoId, string provaId)
            => await Colecao.Find(r => r.AlunoId == alunoId && r.ProvaId == provaId).AnyAsync();

        public static async Task InserirAsync(ResultadoProva resultado)
        {
            resultado.Id = MongoContext.NovoId();
            await Colecao.InsertOneAsync(resultado);
        }
    }

    public static class SalaRepositorio
    {
        private static IMongoCollection<Sala> Colecao => MongoContext.Salas;

        public static async Task<List<Sala>> DoProfessorAsync(string professorId)
            => await Colecao.Find(s => s.ProfessorId == professorId).SortBy(s => s.Nome).ToListAsync();

        public static async Task<List<Sala>> DoAlunoAsync(string alunoId)
            => await Colecao.Find(s => s.AlunoIds.Contains(alunoId)).ToListAsync();

        public static async Task<Sala?> PorIdAsync(string id)
            => await Colecao.Find(s => s.Id == id).FirstOrDefaultAsync();

        public static async Task SalvarAsync(Sala sala)
        {
            if (string.IsNullOrEmpty(sala.Id))
            {
                sala.Id = MongoContext.NovoId();
                sala.CreatedAt = DateTime.Now;
            }

            sala.UpdatedAt = DateTime.Now;
            await Colecao.ReplaceOneAsync(s => s.Id == sala.Id, sala, new ReplaceOptions { IsUpsert = true });
        }

        public static async Task ExcluirAsync(string salaId)
        {
            // provas direcionadas à sala passam a valer para o curso inteiro
            await MongoContext.Provas.UpdateManyAsync(p => p.SalaId == salaId, Builders<Prova>.Update.Set(p => p.SalaId, null));
            await Colecao.DeleteOneAsync(s => s.Id == salaId);
        }
    }
}
