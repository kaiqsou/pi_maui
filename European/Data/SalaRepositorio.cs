using European.Models;
using MongoDB.Driver;

namespace European.Data
{
    public static class SalaRepositorio
    {
        private static IMongoCollection<Sala> Colecao => MongoContext.Salas;

        public static Task<Sala?> PorIdAsync(string id) => Colecao.PorIdAsync(id);

        public static Task<List<Sala>> DoProfessorAsync(string professorId)
            => Colecao.Find(s => s.ProfessorId == professorId).SortBy(s => s.Nome).ToListAsync();

        public static Task<List<Sala>> DoAlunoAsync(string alunoId)
            => Colecao.Find(s => s.AlunoIds.Contains(alunoId)).ToListAsync();

        public static Task SalvarAsync(Sala sala) => Colecao.SalvarAsync(sala);

        public static async Task ExcluirAsync(string salaId)
        {
            // provas direcionadas à sala passam a valer para o curso inteiro
            await MongoContext.Provas.UpdateManyAsync(p => p.SalaId == salaId, Builders<Prova>.Update.Set(p => p.SalaId, null));
            await Colecao.ExcluirAsync(salaId);
        }
    }
}
