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

        public static async Task<Usuario?> PorIdAsync(string id)
            => await Colecao.Find(u => u.Id == id).FirstOrDefaultAsync();

        public static async Task<List<Usuario>> PorIdsAsync(IEnumerable<string> ids)
        {
            var lista = ids.Distinct().ToList();
            if (lista.Count == 0)
                return new List<Usuario>();

            return await Colecao.Find(Builders<Usuario>.Filter.In(u => u.Id, lista)).ToListAsync();
        }

        public static async Task<List<Usuario>> ListarAsync(Roles role)
            => await Colecao.Find(u => u.Roles == role).SortBy(u => u.Nome).ToListAsync();

        public static async Task<bool> ExisteCoordenacaoAsync()
            => await Colecao.Find(u => u.Roles == Roles.Coordenador || u.Roles == Roles.Administrador).AnyAsync();

        /// <summary>Insere o usuário; retorna false se o e-mail já estiver cadastrado.</summary>
        public static async Task<bool> InserirAsync(Usuario usuario)
        {
            usuario.Id = string.IsNullOrEmpty(usuario.Id) ? MongoContext.NovoId() : usuario.Id;
            usuario.Email = NormalizarEmail(usuario.Email);
            usuario.Nome = usuario.Nome.Trim();
            usuario.CreatedAt = DateTime.Now;
            usuario.UpdatedAt = DateTime.Now;

            try
            {
                await Colecao.InsertOneAsync(usuario);
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
            }
        }

        public static async Task AtualizarAsync(Usuario usuario)
        {
            usuario.UpdatedAt = DateTime.Now;
            await Colecao.ReplaceOneAsync(u => u.Id == usuario.Id, usuario);
        }
    }
}
