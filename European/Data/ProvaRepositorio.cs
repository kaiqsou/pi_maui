using European.Models;

namespace European.Data
{
    public static class ProvaRepositorio
    {
        public static Task<Prova?> PorIdAsync(string id) => Api.GetOuNullAsync<Prova>($"provas/{Api.Esc(id)}");

        public static Task<List<Prova>> PorIdsAsync(IEnumerable<string> ids) => Api.PorIdsAsync<Prova>("provas/ids", ids);

        /// <summary>Provas do professor, as alteradas mais recentemente primeiro.</summary>
        public static Task<List<Prova>> DoProfessorAsync(string professorId)
            => Api.GetAsync<List<Prova>>($"provas/professor/{Api.Esc(professorId)}");

        public static Task<List<Prova>> DosCursosAsync(IEnumerable<string> cursoIds)
            => Api.PorIdsAsync<Prova>("provas/cursos", cursoIds);

        /// <summary>Cria ou substitui a prova.</summary>
        public static Task SalvarAsync(Prova prova)
        {
            ApiContext.PrepararSalvar(prova);
            return Api.PutAsync($"provas/{Api.Esc(prova.Id)}", prova);
        }

        /// <summary>Grava a nova ordem das provas de um curso (1, 2, 3... na ordem da lista).</summary>
        public static Task SalvarOrdemAsync(IReadOnlyList<Prova> provas)
            => provas.Count == 0 ? Task.CompletedTask : Api.PutAsync("provas/ordem", new { ids = provas.Select(p => p.Id).ToList() });

        /// <summary>Exclui a prova e os resultados dela.</summary>
        public static Task ExcluirAsync(string provaId) => Api.DeleteAsync($"provas/{Api.Esc(provaId)}");

        public static Task ExcluirDaMateriaAsync(string materiaId) => Api.DeleteAsync($"provas/materia/{Api.Esc(materiaId)}");
    }
}
