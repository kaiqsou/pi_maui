using European.Enums;
using European.Models;

namespace European.Data
{
    public static class MatriculaRepositorio
    {
        /// <summary>Matrículas do aluno (mais recentes primeiro), com o curso já carregado.</summary>
        public static async Task<List<Matricula>> DoAlunoAsync(string alunoId)
        {
            var matriculas = await Api.GetAsync<List<Matricula>>($"matriculas/aluno/{Api.Esc(alunoId)}");
            var cursos = (await CursoRepositorio.PorIdsAsync(matriculas.Select(m => m.CursoId))).ToDictionary(c => c.Id);

            foreach (var matricula in matriculas)
                matricula.Curso = cursos.GetValueOrDefault(matricula.CursoId);

            return matriculas.Where(m => m.Curso != null).ToList();
        }

        public static async Task<List<Matricula>> AtivasDoAlunoAsync(string alunoId)
            => (await DoAlunoAsync(alunoId)).Where(m => m.Status == StatusMatricula.Ativa).ToList();

        /// <summary>Matrículas cujo conteúdo o aluno ainda consulta: ativas primeiro, depois as concluídas dentro da validade.</summary>
        public static async Task<List<Matricula>> ComConteudoDoAlunoAsync(string alunoId)
            => (await DoAlunoAsync(alunoId))
                .Where(m => m.AcessoAulasVigente)
                .OrderBy(m => m.Status == StatusMatricula.Ativa ? 0 : 1)
                .ToList();

        public static Task<List<Matricula>> DosAlunosAsync(IEnumerable<string> alunoIds)
            => Api.PorIdsAsync<Matricula>("matriculas/alunos", alunoIds);

        public static Task<List<Matricula>> DoCursoAsync(string cursoId)
            => Api.GetAsync<List<Matricula>>($"matriculas/curso/{Api.Esc(cursoId)}");

        /// <summary>Quantidade de matrículas ativas por curso.</summary>
        public static Task<Dictionary<string, int>> AtivasPorCursoAsync()
            => Api.GetAsync<Dictionary<string, int>>("matriculas/ativas-por-curso");

        public static Task<Matricula?> PorAlunoECursoAsync(string alunoId, string cursoId)
            => Api.GetOuNullAsync<Matricula>($"matriculas/aluno/{Api.Esc(alunoId)}/curso/{Api.Esc(cursoId)}");

        public static Task InserirAsync(Matricula matricula)
        {
            ApiContext.PrepararInsercao(matricula);
            return Api.PostAsync("matriculas", matricula);
        }

        public static Task AtualizarAsync(Matricula matricula)
        {
            ApiContext.PrepararAtualizar(matricula);
            return Api.PutAsync($"matriculas/{Api.Esc(matricula.Id)}", matricula);
        }

        /// <summary>Grava só o calendário de provas (não sobrescreve status/progresso alterados em outro lugar).</summary>
        public static Task AtualizarAgendaAsync(Matricula matricula)
            => Api.PatchAsync($"matriculas/{Api.Esc(matricula.Id)}/agenda", new { matricula.AgendaProvas });
    }
}
