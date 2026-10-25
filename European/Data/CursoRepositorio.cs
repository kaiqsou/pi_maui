using European.Models;
using MongoDB.Driver;

namespace European.Data
{
    /// <summary>
    /// Cada curso é um documento com a árvore Módulo > Matéria > Aula embutida.
    /// Alterações na estrutura relêem o documento antes de gravar, para não sobrescrever
    /// mudanças feitas ao mesmo tempo por outro usuário.
    /// </summary>
    public static class CursoRepositorio
    {
        private static IMongoCollection<Curso> Colecao => MongoContext.Cursos;

        public static async Task<List<Curso>> ListarAsync()
        {
            var cursos = await Colecao.Find(FilterDefinition<Curso>.Empty).SortBy(c => c.Nome).ToListAsync();
            return cursos.Select(c => c.Vincular()).ToList();
        }

        public static async Task<List<Curso>> PorIdsAsync(IEnumerable<string> ids)
        {
            var lista = ids.Distinct().ToList();
            if (lista.Count == 0)
                return new List<Curso>();

            var cursos = await Colecao.Find(Builders<Curso>.Filter.In(c => c.Id, lista)).SortBy(c => c.Nome).ToListAsync();
            return cursos.Select(c => c.Vincular()).ToList();
        }

        public static async Task<List<Curso>> DoProfessorAsync(string professorId)
        {
            var cursos = await Colecao.Find(c => c.ProfessorIds.Contains(professorId)).SortBy(c => c.Nome).ToListAsync();
            return cursos.Select(c => c.Vincular()).ToList();
        }

        public static async Task<Curso?> PorIdAsync(string id)
            => (await Colecao.Find(c => c.Id == id).FirstOrDefaultAsync())?.Vincular();

        public static async Task SalvarAsync(Curso curso)
        {
            if (string.IsNullOrEmpty(curso.Id))
            {
                curso.Id = MongoContext.NovoId();
                curso.CreatedAt = DateTime.Now;
            }

            curso.UpdatedAt = DateTime.Now;
            await Colecao.ReplaceOneAsync(c => c.Id == curso.Id, curso, new ReplaceOptions { IsUpsert = true });
            curso.Vincular();
        }

        /// <summary>Relê o curso, aplica a alteração e grava. Retorna o curso atualizado (ou null se não existe mais).</summary>
        public static async Task<Curso?> AlterarAsync(string cursoId, Action<Curso> alteracao)
        {
            var curso = await PorIdAsync(cursoId);
            if (curso == null)
                return null;

            alteracao(curso);
            await SalvarAsync(curso);
            return curso;
        }

        /// <summary>Grava a duração real do vídeo detectada pelo player, sem reescrever o documento inteiro.</summary>
        public static async Task AtualizarDuracaoAulaAsync(string cursoId, string aulaId, int segundos)
        {
            var update = Builders<Curso>.Update.Set("Modulos.$[].Materias.$[].Aulas.$[a].DuracaoSegundos", segundos);
            var opcoes = new UpdateOptions
            {
                ArrayFilters = new[] { new BsonDocumentArrayFilterDefinition<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("a._id", aulaId)) }
            };
            await Colecao.UpdateOneAsync(c => c.Id == cursoId, update, opcoes);
        }

        /// <summary>Exclui o curso e tudo que depende dele. Certificados já emitidos são mantidos.</summary>
        public static async Task ExcluirAsync(string cursoId)
        {
            var matriculaIds = await MongoContext.Matriculas.Find(m => m.CursoId == cursoId).Project(m => m.Id).ToListAsync();
            var provaIds = await MongoContext.Provas.Find(p => p.CursoId == cursoId).Project(p => p.Id).ToListAsync();

            await MongoContext.Progressos.DeleteManyAsync(Builders<ProgressoAula>.Filter.In(p => p.MatriculaId, matriculaIds));
            await MongoContext.Matriculas.DeleteManyAsync(m => m.CursoId == cursoId);
            await MongoContext.Resultados.DeleteManyAsync(Builders<ResultadoProva>.Filter.In(r => r.ProvaId, provaIds));
            await MongoContext.Provas.DeleteManyAsync(p => p.CursoId == cursoId);
            await MaterialRepositorio.ExcluirDoCursoAsync(cursoId);
            await MongoContext.Salas.UpdateManyAsync(s => s.CursoId == cursoId, Builders<Sala>.Update.Set(s => s.CursoId, null));
            await Colecao.DeleteOneAsync(c => c.Id == cursoId);
        }
    }
}
