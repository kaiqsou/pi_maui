using European.Models;

namespace European.Data
{
    public static class ProgressoRepositorio
    {
        public static Task<List<ProgressoAula>> DaMatriculaAsync(string matriculaId)
            => Api.GetAsync<List<ProgressoAula>>($"progressos/matricula/{Api.Esc(matriculaId)}");

        /// <summary>Progresso já gravado da aula, ou um novo (ainda não salvo).</summary>
        public static async Task<ProgressoAula> ObterOuCriarAsync(string matriculaId, string aulaId)
            => await Api.GetOuNullAsync<ProgressoAula>($"progressos/matricula/{Api.Esc(matriculaId)}/aula/{Api.Esc(aulaId)}")
               ?? new ProgressoAula { Id = Ids.Novo(), MatriculaId = matriculaId, AulaId = aulaId };

        public static Task SalvarAsync(ProgressoAula progresso)
        {
            progresso.AtualizadoEm = DateTime.Now;
            return Api.PutAsync($"progressos/{Api.Esc(progresso.Id)}", progresso);
        }
    }
}
