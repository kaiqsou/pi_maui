using European.Models;
using European.Services;

namespace European.Data
{
    /// <summary>
    /// Substitui o antigo MongoContext: o app não acessa mais o MongoDB, só a API (ver <see cref="Api"/>).
    /// Também concentra o preenchimento de Id e datas que o ColecaoExtensions fazia antes de gravar.
    /// </summary>
    public static class ApiContext
    {
        private static readonly SemaphoreSlim _inicializacao = new(1, 1);
        private static bool _inicializado;

        public static string NovoId() => Ids.Novo();

        /// <summary>Pede à API os índices e os cursos iniciais (só se não houver nenhum) e protege senhas antigas em texto.</summary>
        public static async Task InicializarAsync()
        {
            if (_inicializado)
                return;

            await _inicializacao.WaitAsync();
            try
            {
                if (_inicializado)
                    return;

                await Api.PostAsync("inicializar", new { CursosIniciais = CursosIniciais.Criar().ToList() });

                // Versões antigas gravavam a senha em texto: troca pelo hash (o login continua funcionando).
                var pendentes = await Api.GetAsync<List<SenhaEmTexto>>("manutencao/senhas-texto");
                foreach (var pendente in pendentes)
                    await Api.PatchAsync("manutencao/senhas",
                        new { pendente.Colecao, pendente.Id, SenhaHash = SenhaService.GerarHash(pendente.Senha) });

                _inicializado = true;
            }
            finally
            {
                _inicializacao.Release();
            }
        }

        /// <summary>Antes de inserir: gera o Id (se vazio) e as datas de criação/alteração.</summary>
        internal static void PrepararInsercao(IEntidade entidade)
        {
            if (string.IsNullOrEmpty(entidade.Id))
                entidade.Id = NovoId();

            if (entidade is IAuditavel auditavel)
                auditavel.CreatedAt = auditavel.UpdatedAt = DateTime.Now;
        }

        /// <summary>Antes de criar ou substituir: gera Id e data de criação se for novo, e atualiza a data de alteração.</summary>
        internal static void PrepararSalvar(IAuditavel entidade)
        {
            if (string.IsNullOrEmpty(entidade.Id))
            {
                entidade.Id = NovoId();
                entidade.CreatedAt = DateTime.Now;
            }

            entidade.UpdatedAt = DateTime.Now;
        }

        /// <summary>Antes de substituir um documento existente.</summary>
        internal static void PrepararAtualizar(IEntidade entidade)
        {
            if (entidade is IAuditavel auditavel)
                auditavel.UpdatedAt = DateTime.Now;
        }

        private sealed class SenhaEmTexto
        {
            public string Colecao { get; set; } = string.Empty;
            public string Id { get; set; } = string.Empty;
            public string Senha { get; set; } = string.Empty;
        }
    }
}
