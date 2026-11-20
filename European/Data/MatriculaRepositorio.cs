using European.Enums;
using European.Models;
using MongoDB.Driver;

namespace European.Data
{
    public static class MatriculaRepositorio
    {
        private static IMongoCollection<Matricula> Colecao => MongoContext.Matriculas;

        /// <summary>Matrículas do aluno, com o curso já carregado.</summary>
        public static async Task<List<Matricula>> DoAlunoAsync(string alunoId)
        {
            var matriculas = await Colecao.Find(m => m.AlunoId == alunoId).SortByDescending(m => m.DataMatricula).ToListAsync();
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
            => Colecao.OndeEmAsync(m => m.AlunoId, alunoIds);

        public static Task<List<Matricula>> DoCursoAsync(string cursoId)
            => Colecao.Find(m => m.CursoId == cursoId).ToListAsync();

        public static async Task<Dictionary<string, int>> AtivasPorCursoAsync()
        {
            var ativas = await Colecao.Find(m => m.Status == StatusMatricula.Ativa).Project(m => m.CursoId).ToListAsync();
            return ativas.GroupBy(id => id).ToDictionary(g => g.Key, g => g.Count());
        }

        public static async Task<Matricula?> PorAlunoECursoAsync(string alunoId, string cursoId)
            => await Colecao.Find(m => m.AlunoId == alunoId && m.CursoId == cursoId).FirstOrDefaultAsync();

        public static Task InserirAsync(Matricula matricula) => Colecao.InserirAsync(matricula);

        public static Task AtualizarAsync(Matricula matricula) => Colecao.AtualizarAsync(matricula);
    }
}
