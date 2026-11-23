using European.Models;

namespace European.Data
{
    public static class ResultadoRepositorio
    {
        public static Task<ResultadoProva?> PorIdAsync(string id) => Api.GetOuNullAsync<ResultadoProva>($"resultados/{Api.Esc(id)}");

        /// <summary>Resultados do aluno, os mais recentes primeiro.</summary>
        public static Task<List<ResultadoProva>> DoAlunoAsync(string alunoId)
            => Api.GetAsync<List<ResultadoProva>>($"resultados/aluno/{Api.Esc(alunoId)}");

        /// <summary>Resultados da prova, da maior nota para a menor.</summary>
        public static Task<List<ResultadoProva>> DaProvaAsync(string provaId)
            => Api.GetAsync<List<ResultadoProva>>($"resultados/prova/{Api.Esc(provaId)}");

        public static Task<List<ResultadoProva>> DasProvasAsync(IEnumerable<string> provaIds)
            => Api.PorIdsAsync<ResultadoProva>("resultados/provas", provaIds);

        public static async Task<bool> ExisteAsync(string alunoId, string provaId)
            => (await Api.GetAsync<Api.RespostaExiste>($"resultados/existe?alunoId={Api.Esc(alunoId)}&provaId={Api.Esc(provaId)}")).Existe;

        public static Task InserirAsync(ResultadoProva resultado)
        {
            ApiContext.PrepararInsercao(resultado);
            return Api.PostAsync("resultados", resultado);
        }

        public static Task AtualizarAsync(ResultadoProva resultado) => Api.PutAsync($"resultados/{Api.Esc(resultado.Id)}", resultado);
    }
}
