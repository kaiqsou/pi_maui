using European.Models;
using MongoDB.Driver;

namespace European.Data
{
    public static class ResultadoRepositorio
    {
        private static IMongoCollection<ResultadoProva> Colecao => MongoContext.Resultados;

        public static Task<ResultadoProva?> PorIdAsync(string id) => Colecao.PorIdAsync(id);

        public static Task<List<ResultadoProva>> DoAlunoAsync(string alunoId)
            => Colecao.Find(r => r.AlunoId == alunoId).SortByDescending(r => r.DataRealizacao).ToListAsync();

        public static Task<List<ResultadoProva>> DaProvaAsync(string provaId)
            => Colecao.Find(r => r.ProvaId == provaId).SortByDescending(r => r.NotaObtida).ToListAsync();

        public static Task<List<ResultadoProva>> DasProvasAsync(IEnumerable<string> provaIds)
            => Colecao.OndeEmAsync(r => r.ProvaId, provaIds);

        public static Task<bool> ExisteAsync(string alunoId, string provaId)
            => Colecao.Find(r => r.AlunoId == alunoId && r.ProvaId == provaId).AnyAsync();

        public static Task InserirAsync(ResultadoProva resultado) => Colecao.InserirAsync(resultado);

        public static Task AtualizarAsync(ResultadoProva resultado) => Colecao.AtualizarAsync(resultado);
    }
}
