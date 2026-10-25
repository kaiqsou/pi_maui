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

        public static async Task<List<Matricula>> DosAlunosAsync(IEnumerable<string> alunoIds)
        {
            var ids = alunoIds.Distinct().ToList();
            return await Colecao.Find(Builders<Matricula>.Filter.In(m => m.AlunoId, ids)).ToListAsync();
        }

        public static async Task<List<Matricula>> DoCursoAsync(string cursoId)
            => await Colecao.Find(m => m.CursoId == cursoId).ToListAsync();

        public static async Task<Dictionary<string, int>> AtivasPorCursoAsync()
        {
            var ativas = await Colecao.Find(m => m.Status == StatusMatricula.Ativa).Project(m => m.CursoId).ToListAsync();
            return ativas.GroupBy(id => id).ToDictionary(g => g.Key, g => g.Count());
        }

        public static async Task<Matricula?> PorAlunoECursoAsync(string alunoId, string cursoId)
            => await Colecao.Find(m => m.AlunoId == alunoId && m.CursoId == cursoId).FirstOrDefaultAsync();

        public static async Task InserirAsync(Matricula matricula)
        {
            matricula.Id = string.IsNullOrEmpty(matricula.Id) ? MongoContext.NovoId() : matricula.Id;
            matricula.CreatedAt = DateTime.Now;
            matricula.UpdatedAt = DateTime.Now;
            await Colecao.InsertOneAsync(matricula);
        }

        public static async Task AtualizarAsync(Matricula matricula)
        {
            matricula.UpdatedAt = DateTime.Now;
            await Colecao.ReplaceOneAsync(m => m.Id == matricula.Id, matricula);
        }
    }

    public static class ProgressoRepositorio
    {
        private static IMongoCollection<ProgressoAula> Colecao => MongoContext.Progressos;

        public static async Task<List<ProgressoAula>> DaMatriculaAsync(string matriculaId)
            => await Colecao.Find(p => p.MatriculaId == matriculaId).ToListAsync();

        public static async Task<ProgressoAula?> ObterAsync(string matriculaId, string aulaId)
            => await Colecao.Find(p => p.MatriculaId == matriculaId && p.AulaId == aulaId).FirstOrDefaultAsync();

        public static async Task<ProgressoAula> ObterOuCriarAsync(string matriculaId, string aulaId)
            => await ObterAsync(matriculaId, aulaId) ?? new ProgressoAula
            {
                Id = MongoContext.NovoId(),
                MatriculaId = matriculaId,
                AulaId = aulaId
            };

        public static async Task SalvarAsync(ProgressoAula progresso)
        {
            progresso.AtualizadoEm = DateTime.Now;
            await Colecao.ReplaceOneAsync(p => p.Id == progresso.Id, progresso, new ReplaceOptions { IsUpsert = true });
        }
    }

    public static class CertificadoRepositorio
    {
        private static IMongoCollection<Certificado> Colecao => MongoContext.Certificados;

        public static async Task<List<Certificado>> DoAlunoAsync(string alunoId)
            => await Colecao.Find(c => c.AlunoId == alunoId).SortByDescending(c => c.EmitidoEm).ToListAsync();

        public static async Task<bool> ExisteParaMatriculaAsync(string matriculaId)
            => await Colecao.Find(c => c.MatriculaId == matriculaId).AnyAsync();

        public static async Task InserirAsync(Certificado certificado)
        {
            certificado.Id = MongoContext.NovoId();
            var total = await Colecao.CountDocumentsAsync(FilterDefinition<Certificado>.Empty);
            certificado.CodigoVerificacao = $"EURO-{DateTime.Now:yyyy}-{total + 1:0000}-{certificado.Id[^4..].ToUpperInvariant()}";
            await Colecao.InsertOneAsync(certificado);
        }
    }
}
