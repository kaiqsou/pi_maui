using European.Enums;
using European.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace European.Data
{
    /// <summary>Pedidos de matrícula ainda não liberados (o candidato ainda não é usuário) e as fotos dos documentos.</summary>
    public static class SolicitacaoRepositorio
    {
        private static IMongoCollection<SolicitacaoMatricula> Colecao => MongoContext.Solicitacoes;

        /// <summary>
        /// Solicitações que chegaram à coordenação: depois do pagamento (em análise ou devolvidas para correção).
        /// Candidatos que ainda estão enviando documentos ou pagando não aparecem; as canceladas ficam só para o
        /// candidato ver o reembolso.
        /// </summary>
        public static Task<List<SolicitacaoMatricula>> ListarAsync()
            => Colecao.Find(s => s.Status == StatusSolicitacao.EmAnalise || s.Status == StatusSolicitacao.Devolvida)
                .SortBy(s => s.CriadaEm).ToListAsync();

        public static Task<SolicitacaoMatricula?> PorIdAsync(string id) => Colecao.PorIdAsync(id);

        public static async Task<SolicitacaoMatricula?> PorEmailAsync(string email)
        {
            var normalizado = UsuarioRepositorio.NormalizarEmail(email);
            return await Colecao.Find(s => s.Email == normalizado).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Grava o pedido; retorna false se o e-mail já tem conta ou outro pedido em andamento.
        /// Um pedido cancelado do mesmo e-mail é apagado: o candidato pode se cadastrar de novo.
        /// </summary>
        public static async Task<bool> InserirAsync(SolicitacaoMatricula solicitacao)
        {
            solicitacao.Id = MongoContext.NovoId();
            solicitacao.Email = UsuarioRepositorio.NormalizarEmail(solicitacao.Email);
            solicitacao.Nome = solicitacao.Nome.Trim();
            solicitacao.CriadaEm = DateTime.Now;
            solicitacao.Status = StatusSolicitacao.Documentos;

            if (await UsuarioRepositorio.PorEmailAsync(solicitacao.Email) != null)
                return false;

            await Colecao.DeleteManyAsync(s => s.Email == solicitacao.Email && s.Status == StatusSolicitacao.Cancelada);
            return await Colecao.TentarInserirAsync(solicitacao);
        }

        /// <summary>Grava a solicitação inteira (status, documentos, pagamento, devolução...).</summary>
        public static Task AtualizarAsync(SolicitacaoMatricula solicitacao) => Colecao.AtualizarAsync(solicitacao);

        public static Task AtualizarSenhaAsync(string id, string senhaHash)
            => Colecao.UpdateOneAsync(s => s.Id == id, Builders<SolicitacaoMatricula>.Update.Set(s => s.SenhaHash, senhaHash));

        /// <summary>Apaga a solicitação e as fotos dos documentos (ao liberar, a conta do aluno já foi criada).</summary>
        public static async Task ExcluirAsync(string id)
        {
            if (await PorIdAsync(id) is { } solicitacao)
                await ExcluirArquivosAsync(solicitacao.Documentos.SelectMany(d => d.Arquivos));
            await Colecao.ExcluirAsync(id);
        }

        // ---- Arquivos dos documentos (GridFS) ----

        /// <summary>Guarda o arquivo (foto ou PDF) no GridFS e devolve o id.</summary>
        public static async Task<string> EnviarArquivoAsync(string solicitacaoId, string tipo, string nomeArquivo, Stream conteudo)
        {
            var id = await MongoContext.Arquivos.UploadFromStreamAsync(nomeArquivo, conteudo, new GridFSUploadOptions
            {
                Metadata = new BsonDocument { { "solicitacaoId", solicitacaoId }, { "documento", tipo } }
            });
            return id.ToString();
        }

        /// <summary>Baixa o arquivo para a pasta de cache (para exibir ou abrir) e devolve o caminho local.</summary>
        public static async Task<string> BaixarArquivoAsync(ArquivoDocumento arquivo)
        {
            var pasta = Path.Combine(FileSystem.CacheDirectory, "documentos", arquivo.ArquivoId);
            Directory.CreateDirectory(pasta);
            var caminho = Path.Combine(pasta, Path.GetFileName(string.IsNullOrEmpty(arquivo.NomeArquivo) ? "documento.jpg" : arquivo.NomeArquivo));

            if (!File.Exists(caminho))
            {
                await using var destino = File.Create(caminho);
                await MongoContext.Arquivos.DownloadToStreamAsync(ObjectId.Parse(arquivo.ArquivoId), destino);
            }

            return caminho;
        }

        public static async Task ExcluirArquivosAsync(IEnumerable<ArquivoDocumento> arquivos)
        {
            foreach (var arquivo in arquivos)
            {
                if (!ObjectId.TryParse(arquivo.ArquivoId, out var id))
                    continue;

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
}
