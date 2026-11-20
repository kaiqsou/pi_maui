using European.Enums;
using European.Models;
using MongoDB.Driver;

namespace European.Data
{
    public static class UsuarioRepositorio
    {
        private static IMongoCollection<Usuario> Colecao => MongoContext.Usuarios;

        public static string NormalizarEmail(string email) => (email ?? string.Empty).Trim().ToLowerInvariant();

        public static async Task<Usuario?> PorEmailAsync(string email)
        {
            var normalizado = NormalizarEmail(email);
            return await Colecao.Find(u => u.Email == normalizado).FirstOrDefaultAsync();
        }

        public static Task<Usuario?> PorIdAsync(string id) => Colecao.PorIdAsync(id);

        public static Task<List<Usuario>> PorIdsAsync(IEnumerable<string> ids) => Colecao.PorIdsAsync(ids);

        public static Task<List<Usuario>> ListarAsync(Roles role)
            => Colecao.Find(u => u.Roles == role).SortBy(u => u.Nome).ToListAsync();

        public static Task<List<Usuario>> CoordenacaoAsync()
            => Colecao.Find(u => u.Roles == Roles.Coordenador || u.Roles == Roles.Administrador).ToListAsync();

        /// <summary>Banco recém-instalado (nenhuma conta): só então é permitido criar a primeira coordenação.</summary>
        public static Task<bool> ExisteAlgumUsuarioAsync() => Colecao.Find(FilterDefinition<Usuario>.Empty).AnyAsync();

        /// <summary>Insere o usuário; retorna false se o e-mail já estiver cadastrado.</summary>
        public static Task<bool> InserirAsync(Usuario usuario)
        {
            usuario.Email = NormalizarEmail(usuario.Email);
            usuario.Nome = usuario.Nome.Trim();
            return Colecao.TentarInserirAsync(usuario);
        }

        /// <summary>
        /// Grava só a senha, sem sobrescrever o resto do cadastro (ex.: acesso alterado pela coordenação).
        /// <paramref name="provisoria"/> = true quando é a coordenação que define a senha: o usuário troca no próximo login.
        /// </summary>
        public static async Task AtualizarSenhaAsync(string usuarioId, string senhaHash, bool provisoria = false)
        {
            var update = Builders<Usuario>.Update
                .Set(u => u.SenhaHash, senhaHash)
                .Set(u => u.SenhaProvisoria, provisoria)
                .Set(u => u.UpdatedAt, DateTime.Now);
            await Colecao.UpdateOneAsync(u => u.Id == usuarioId, update);
        }

        public static Task AtualizarAsync(Usuario usuario) => Colecao.AtualizarAsync(usuario);
    }
}
