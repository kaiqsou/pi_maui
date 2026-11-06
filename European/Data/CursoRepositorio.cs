using European.Models;
using MongoDB.Bson;
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
            => Vincular(await Colecao.Find(FilterDefinition<Curso>.Empty).ToListAsync());

        public static async Task<List<Curso>> PorIdsAsync(IEnumerable<string> ids)
            => Vincular(await Colecao.PorIdsAsync(ids));

        public static async Task<List<Curso>> DoProfessorAsync(string professorId)
            => Vincular(await Colecao.Find(c => c.ProfessorIds.Contains(professorId)).ToListAsync());

        public static async Task<Curso?> PorIdAsync(string id)
            => (await Colecao.PorIdAsync(id))?.Vincular();

        public static async Task SalvarAsync(Curso curso)
        {
            await Colecao.SalvarAsync(curso);
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

        /// <summary>Cria (no fim da lista) ou altera um módulo do curso.</summary>
        public static Task<Curso?> SalvarModuloAsync(string cursoId, string? moduloId, Action<Modulo> aplicar)
            => AlterarAsync(cursoId, curso => aplicar(ObterOuAdicionar(curso.Modulos, moduloId)));

        /// <summary>Cria (no fim do módulo) ou altera uma matéria.</summary>
        public static Task<Curso?> SalvarMateriaAsync(string cursoId, string moduloId, string? materiaId, Action<Materia> aplicar)
            => AlterarAsync(cursoId, curso =>
            {
                var modulo = curso.Modulos.FirstOrDefault(m => m.Id == moduloId)
                    ?? throw new InvalidOperationException("O módulo foi removido.");
                aplicar(ObterOuAdicionar(modulo.Materias, materiaId));
            });

        /// <summary>Cria (no fim da matéria) ou altera uma aula. Retorna a aula gravada.</summary>
        public static async Task<Aula?> SalvarAulaAsync(string cursoId, string materiaId, string? aulaId, Action<Aula> aplicar)
        {
            Aula? salva = null;
            await AlterarAsync(cursoId, curso =>
            {
                var materia = curso.TodasMaterias.FirstOrDefault(m => m.Id == materiaId)
                    ?? throw new InvalidOperationException("A matéria foi removida.");
                salva = ObterOuAdicionar(materia.Aulas, aulaId);
                aplicar(salva);
            });
            return salva;
        }

        /// <summary>Grava a duração real do vídeo detectada pelo player, sem reescrever o documento inteiro.</summary>
        public static async Task AtualizarDuracaoAulaAsync(string cursoId, string aulaId, int segundos)
        {
            var update = Builders<Curso>.Update.Set("Modulos.$[].Materias.$[].Aulas.$[a].DuracaoSegundos", segundos);
            var opcoes = new UpdateOptions
            {
                ArrayFilters = new[] { new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("a._id", aulaId)) }
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

        /// <summary>Exclui o módulo com suas matérias, aulas, materiais e provas.</summary>
        public static async Task ExcluirModuloAsync(string cursoId, Modulo modulo)
        {
            foreach (var materia in modulo.Materias)
                await ExcluirConteudoDaMateriaAsync(materia);

            await AlterarAsync(cursoId, curso => Remover(curso.Modulos, modulo.Id));
        }

        /// <summary>Exclui a matéria com suas aulas, materiais e provas.</summary>
        public static async Task ExcluirMateriaAsync(string cursoId, Materia materia)
        {
            await ExcluirConteudoDaMateriaAsync(materia);
            await AlterarAsync(cursoId, curso =>
            {
                var modulo = curso.Modulos.FirstOrDefault(m => m.Id == materia.Modulo?.Id);
                if (modulo != null)
                    Remover(modulo.Materias, materia.Id);
            });
        }

        /// <summary>Exclui a aula e seus materiais.</summary>
        public static async Task ExcluirAulaAsync(string cursoId, Aula aula)
        {
            await MaterialRepositorio.ExcluirDasAulasAsync(new[] { aula.Id });
            await AlterarAsync(cursoId, curso =>
            {
                var materia = curso.TodasMaterias.FirstOrDefault(m => m.Id == aula.Materia?.Id);
                if (materia != null)
                    Remover(materia.Aulas, aula.Id);
            });
        }

        private static async Task ExcluirConteudoDaMateriaAsync(Materia materia)
        {
            await MaterialRepositorio.ExcluirDasAulasAsync(materia.Aulas.Select(a => a.Id));
            await ProvaRepositorio.ExcluirDaMateriaAsync(materia.Id);
        }

        private static List<Curso> Vincular(List<Curso> cursos)
            => cursos.Select(c => c.Vincular()).OrderBy(c => c.Nome).ToList();

        private static T ObterOuAdicionar<T>(List<T> itens, string? id) where T : class, IOrdenavel, new()
        {
            var item = id == null ? null : itens.FirstOrDefault(i => i.Id == id);
            if (item == null)
            {
                item = new T { Id = MongoContext.NovoId(), Ordem = itens.Count + 1 };
                itens.Add(item);
            }

            return item;
        }

        /// <summary>Remove o item e renumera os demais (1, 2, 3...).</summary>
        private static void Remover<T>(List<T> itens, string id) where T : IOrdenavel
        {
            itens.RemoveAll(i => i.Id == id);
            for (var i = 0; i < itens.Count; i++)
                itens[i].Ordem = i + 1;
        }
    }
}
