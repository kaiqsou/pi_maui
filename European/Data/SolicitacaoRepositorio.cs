using European.Enums;
using European.Models;
using MongoDB.Driver;

namespace European.Data
{
    /// <summary>Pedidos de matrícula ainda não liberados (o candidato ainda não é usuário).</summary>
    public static class SolicitacaoRepositorio
    {
        private static IMongoCollection<SolicitacaoMatricula> Colecao => MongoContext.Solicitacoes;

        public static async Task<List<SolicitacaoMatricula>> ListarAsync()
            => await Colecao.Find(FilterDefinition<SolicitacaoMatricula>.Empty).SortBy(s => s.CriadaEm).ToListAsync();

        public static async Task<SolicitacaoMatricula?> PorIdAsync(string id)
            => await Colecao.Find(s => s.Id == id).FirstOrDefaultAsync();

        public static async Task<SolicitacaoMatricula?> PorEmailAsync(string email)
        {
            var normalizado = UsuarioRepositorio.NormalizarEmail(email);
            return await Colecao.Find(s => s.Email == normalizado).FirstOrDefaultAsync();
        }

        /// <summary>Grava o pedido; retorna false se o e-mail já tem conta ou outro pedido em andamento.</summary>
        public static async Task<bool> InserirAsync(SolicitacaoMatricula solicitacao)
        {
            solicitacao.Id = MongoContext.NovoId();
            solicitacao.Email = UsuarioRepositorio.NormalizarEmail(solicitacao.Email);
            solicitacao.Nome = solicitacao.Nome.Trim();
            solicitacao.CriadaEm = DateTime.Now;

            if (await UsuarioRepositorio.PorEmailAsync(solicitacao.Email) != null)
                return false;

            try
            {
                await Colecao.InsertOneAsync(solicitacao);
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
            }
        }

        /// <summary>Um novo pagamento responde à pendência anterior: o pedido volta para a fila da coordenação.</summary>
        public static async Task RegistrarPagamentoAsync(string id, FormaPagamento forma, DateTime quando)
        {
            var update = Builders<SolicitacaoMatricula>.Update
                .Set(s => s.FormaPagamento, forma)
                .Set(s => s.PagamentoEm, quando)
                .Set(s => s.PendenciaMotivo, null)
                .Set(s => s.PendenciaEm, null);
            await Colecao.UpdateOneAsync(s => s.Id == id, update);
        }

        public static async Task RegistrarPendenciaAsync(string id, string motivo)
        {
            var update = Builders<SolicitacaoMatricula>.Update
                .Set(s => s.PendenciaMotivo, motivo)
                .Set(s => s.PendenciaEm, DateTime.Now);
            await Colecao.UpdateOneAsync(s => s.Id == id, update);
        }

        public static async Task ExcluirAsync(string id)
            => await Colecao.DeleteOneAsync(s => s.Id == id);
    }
}
