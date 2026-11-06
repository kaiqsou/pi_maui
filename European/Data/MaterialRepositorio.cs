using European.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace European.Data
{
    /// <summary>Materiais complementares das aulas. Arquivos enviados ficam no GridFS do próprio MongoDB.</summary>
    public static class MaterialRepositorio
    {
        private static IMongoCollection<MaterialComplementar> Colecao => MongoContext.Materiais;

        public static Task<List<MaterialComplementar>> DaAulaAsync(string aulaId)
            => Colecao.Find(m => m.AulaId == aulaId).SortBy(m => m.Ordem).ToListAsync();

        public static async Task InserirAsync(MaterialComplementar material, Stream? arquivo = null)
        {
            material.Id = MongoContext.NovoId();
            material.Ordem = (int)await Colecao.CountDocumentsAsync(m => m.AulaId == material.AulaId) + 1;

            if (arquivo != null && !string.IsNullOrEmpty(material.NomeArquivo))
            {
                var id = await MongoContext.Arquivos.UploadFromStreamAsync(material.NomeArquivo, arquivo, new GridFSUploadOptions
                {
                    Metadata = new BsonDocument { { "aulaId", material.AulaId }, { "cursoId", material.CursoId } }
                });
                material.ArquivoId = id.ToString();
            }

            await Colecao.InsertOneAsync(material);
        }

        public static async Task ExcluirAsync(MaterialComplementar material)
        {
            await ExcluirArquivoAsync(material.ArquivoId);
            await Colecao.ExcluirAsync(material.Id);
        }

        public static async Task ExcluirDasAulasAsync(IEnumerable<string> aulaIds)
        {
            var materiais = await Colecao.OndeEmAsync(m => m.AulaId, aulaIds);
            foreach (var material in materiais)
                await ExcluirAsync(material);
        }

        public static async Task ExcluirDoCursoAsync(string cursoId)
        {
            var materiais = await Colecao.Find(m => m.CursoId == cursoId).ToListAsync();
            foreach (var material in materiais)
                await ExcluirAsync(material);
        }

        /// <summary>Baixa o arquivo do GridFS para a pasta de cache e devolve o caminho local.</summary>
        public static async Task<string> BaixarAsync(MaterialComplementar material)
        {
            if (string.IsNullOrEmpty(material.ArquivoId))
                throw new InvalidOperationException("O material não possui arquivo.");

            var pasta = Path.Combine(FileSystem.CacheDirectory, "materiais", material.Id);
            Directory.CreateDirectory(pasta);

            var caminho = Path.Combine(pasta, Path.GetFileName(material.NomeArquivo ?? "material"));
            await using (var destino = File.Create(caminho))
                await MongoContext.Arquivos.DownloadToStreamAsync(ObjectId.Parse(material.ArquivoId), destino);

            return caminho;
        }

        private static async Task ExcluirArquivoAsync(string? arquivoId)
        {
            if (string.IsNullOrEmpty(arquivoId) || !ObjectId.TryParse(arquivoId, out var id))
                return;

            try
            {
                await MongoContext.Arquivos.DeleteAsync(id);
            }
            catch (GridFSFileNotFoundException)
            {
                // já removido
            }
        }
    }
}
