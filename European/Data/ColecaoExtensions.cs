using System.Linq.Expressions;
using European.Models;
using MongoDB.Driver;

namespace European.Data
{
    /// <summary>Operações comuns a todas as coleções, usadas pelos repositórios.</summary>
    internal static class ColecaoExtensions
    {
        private static FilterDefinition<T> FiltroId<T>(string id) => Builders<T>.Filter.Eq("_id", id);

        public static async Task<T?> PorIdAsync<T>(this IMongoCollection<T> colecao, string id) where T : class, IEntidade
            => await colecao.Find(FiltroId<T>(id)).FirstOrDefaultAsync();

        public static Task<List<T>> PorIdsAsync<T>(this IMongoCollection<T> colecao, IEnumerable<string> ids) where T : IEntidade
            => colecao.FiltrarEmAsync("_id", ids);

        /// <summary>Documentos cujo campo tem um dos valores informados.</summary>
        public static Task<List<T>> OndeEmAsync<T>(this IMongoCollection<T> colecao, Expression<Func<T, string>> campo, IEnumerable<string> valores)
            => colecao.FiltrarEmAsync(new ExpressionFieldDefinition<T, string>(campo), valores);

        private static async Task<List<T>> FiltrarEmAsync<T>(this IMongoCollection<T> colecao, FieldDefinition<T, string> campo, IEnumerable<string> valores)
        {
            var lista = valores.Distinct().ToList();
            if (lista.Count == 0)
                return new List<T>();

            return await colecao.Find(Builders<T>.Filter.In(campo, lista)).ToListAsync();
        }

        /// <summary>Insere gerando o Id (se vazio) e as datas de criação/alteração.</summary>
        public static Task InserirAsync<T>(this IMongoCollection<T> colecao, T entidade) where T : IEntidade
        {
            if (string.IsNullOrEmpty(entidade.Id))
                entidade.Id = MongoContext.NovoId();

            if (entidade is IAuditavel auditavel)
                auditavel.CreatedAt = auditavel.UpdatedAt = DateTime.Now;

            return colecao.InsertOneAsync(entidade);
        }

        /// <summary>Insere; retorna false se violar um índice único (ex.: e-mail já cadastrado).</summary>
        public static async Task<bool> TentarInserirAsync<T>(this IMongoCollection<T> colecao, T entidade) where T : IEntidade
        {
            try
            {
                await colecao.InserirAsync(entidade);
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
            }
        }

        /// <summary>Cria (gerando Id e data de criação) ou substitui o documento inteiro.</summary>
        public static Task SalvarAsync<T>(this IMongoCollection<T> colecao, T entidade) where T : IAuditavel
        {
            if (string.IsNullOrEmpty(entidade.Id))
            {
                entidade.Id = MongoContext.NovoId();
                entidade.CreatedAt = DateTime.Now;
            }

            entidade.UpdatedAt = DateTime.Now;
            return colecao.ReplaceOneAsync(FiltroId<T>(entidade.Id), entidade, new ReplaceOptions { IsUpsert = true });
        }

        /// <summary>Substitui um documento existente.</summary>
        public static Task AtualizarAsync<T>(this IMongoCollection<T> colecao, T entidade) where T : IEntidade
        {
            if (entidade is IAuditavel auditavel)
                auditavel.UpdatedAt = DateTime.Now;

            return colecao.ReplaceOneAsync(FiltroId<T>(entidade.Id), entidade);
        }

        public static Task ExcluirAsync<T>(this IMongoCollection<T> colecao, string id)
            => colecao.DeleteOneAsync(FiltroId<T>(id));
    }
}
