using European.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace European.Data
{
    public static class CertificadoRepositorio
    {
        private static IMongoCollection<Certificado> Colecao => MongoContext.Certificados;

        // Versões antigas emitiam certificados por matéria (campo MateriaId); só o certificado final do curso vale.
        private static readonly FilterDefinition<Certificado> SomenteFinal =
            Builders<Certificado>.Filter.Eq("MateriaId", BsonNull.Value);

        public static Task<List<Certificado>> DoAlunoAsync(string alunoId)
            => Colecao.Find(Builders<Certificado>.Filter.Eq(c => c.AlunoId, alunoId) & SomenteFinal)
                .SortByDescending(c => c.EmitidoEm).ToListAsync();

        public static Task<bool> ExisteParaMatriculaAsync(string matriculaId)
            => Colecao.Find(Builders<Certificado>.Filter.Eq(c => c.MatriculaId, matriculaId) & SomenteFinal).AnyAsync();

        public static async Task InserirAsync(Certificado certificado)
        {
            certificado.Id = MongoContext.NovoId();
            var total = await Colecao.CountDocumentsAsync(FilterDefinition<Certificado>.Empty);
            certificado.CodigoVerificacao = $"EURO-{DateTime.Now:yyyy}-{total + 1:0000}-{certificado.Id[^4..].ToUpperInvariant()}";
            await Colecao.InsertOneAsync(certificado);
        }
    }
}
