using European.Enums;
using European.Models;

namespace European.Data
{
    public static class UsuarioRepositorio
    {
        public static string NormalizarEmail(string email) => (email ?? string.Empty).Trim().ToLowerInvariant();

        public static Task<Usuario?> PorEmailAsync(string email)
            => Api.GetOuNullAsync<Usuario>($"usuarios/email/{Api.Esc(NormalizarEmail(email))}");

        public static Task<Usuario?> PorIdAsync(string id) => Api.GetOuNullAsync<Usuario>($"usuarios/{Api.Esc(id)}");

        public static Task<List<Usuario>> PorIdsAsync(IEnumerable<string> ids) => Api.PorIdsAsync<Usuario>("usuarios/ids", ids);

        /// <summary>Usuários do papel informado, em ordem de nome.</summary>
        public static Task<List<Usuario>> ListarAsync(Roles role) => Api.GetAsync<List<Usuario>>($"usuarios?role={role}");

        public static Task<List<Usuario>> CoordenacaoAsync() => Api.GetAsync<List<Usuario>>("usuarios/coordenacao");

        /// <summary>Banco recém-instalado (nenhuma conta): só então é permitido criar a primeira coordenação.</summary>
        public static async Task<bool> ExisteAlgumUsuarioAsync()
            => (await Api.GetAsync<Api.RespostaExiste>("usuarios/existe")).Existe;

        /// <summary>Insere o usuário; retorna false se o e-mail já estiver cadastrado.</summary>
        public static Task<bool> InserirAsync(Usuario usuario)
        {
            usuario.Email = NormalizarEmail(usuario.Email);
            usuario.Nome = usuario.Nome.Trim();
            ApiContext.PrepararInsercao(usuario);
            return Api.InserirAsync("usuarios", usuario);
        }

        /// <summary>
        /// Grava só a senha, sem sobrescrever o resto do cadastro (ex.: acesso alterado pela coordenação).
        /// <paramref name="provisoria"/> = true quando é a coordenação que define a senha: o usuário troca no próximo login.
        /// </summary>
        public static Task AtualizarSenhaAsync(string usuarioId, string senhaHash, bool provisoria = false)
            => Api.PatchAsync($"usuarios/{Api.Esc(usuarioId)}/senha", new { SenhaHash = senhaHash, SenhaProvisoria = provisoria });

        public static Task AtualizarAsync(Usuario usuario)
        {
            ApiContext.PrepararAtualizar(usuario);
            return Api.PutAsync($"usuarios/{Api.Esc(usuario.Id)}", usuario);
        }
    }
}
