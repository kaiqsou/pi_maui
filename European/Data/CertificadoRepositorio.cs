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

        public static async Task<Certificado?> DaMatriculaAsync(string matriculaId)
            => await Colecao.Find(Builders<Certificado>.Filter.Eq(c => c.MatriculaId, matriculaId) & SomenteFinal).FirstOrDefaultAsync();

        /// <summary>Grava o certificado com um código de verificação único (derivado do Id, sem contador sujeito a repetição).</summary>
        public static Task InserirAsync(Certificado certificado)
        {
            certificado.Id = MongoContext.NovoId();
            certificado.CodigoVerificacao = $"EURO-{certificado.EmitidoEm:yyyy}-{certificado.Id[^8..].ToUpperInvariant()}";
            return Colecao.InsertOneAsync(certificado);
        }
    }
}
