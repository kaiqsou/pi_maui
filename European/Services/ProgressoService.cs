using European.Data;
using European.Enums;
using European.Models;

namespace European.Services
{
    /// <summary>Progresso do aluno nas aulas, médias por matéria e emissão do certificado único do mestrado.</summary>
    public static class ProgressoService
    {
        public const double NotaCortePadrao = 6;

        /// <summary>A aula só é liberada depois que todas as anteriores (na ordem do curso) foram concluídas.</summary>
        public static bool AulaEstaLiberada(Curso curso, IReadOnlyCollection<ProgressoAula> progressos, Aula aula)
        {
            var concluidas = progressos.Where(p => p.Concluida).Select(p => p.AulaId).ToHashSet();

            foreach (var anterior in curso.TodasAulas)
            {
                if (anterior.Id == aula.Id)
                    return true;

                if (!concluidas.Contains(anterior.Id))
                    return false;
            }

            return true;
        }

        public static async Task ConcluirAulaAsync(Matricula matricula, Aula aula)
        {
            var progresso = await ProgressoRepositorio.ObterOuCriarAsync(matricula.Id, aula.Id);
            progresso.Concluida = true;
            progresso.PercentualAssistido = 100;
            await ProgressoRepositorio.SalvarAsync(progresso);

            await AtualizarAsync(matricula);
        }

        /// <summary>Média ponderada (0 a 10) das provas realizadas, usando o peso de cada prova.</summary>
        public static double Media(IEnumerable<(ResultadoProva Resultado, Prova Prova)> notas)
        {
            var lista = notas.ToList();
            var pesos = lista.Sum(n => Math.Max(0, n.Prova.Peso));
            if (pesos <= 0)
                return lista.Count == 0 ? 0 : lista.Average(n => n.Resultado.NotaNormalizada);

            return lista.Sum(n => n.Resultado.NotaNormalizada * Math.Max(0, n.Prova.Peso)) / pesos;
        }

        /// <summary>Recalcula o percentual da matrícula e emite o certificado quando o curso é concluído com aprovação.</summary>
        public static async Task AtualizarAsync(Matricula matricula)
        {
            var curso = matricula.Curso ?? await CursoRepositorio.PorIdAsync(matricula.CursoId);
            if (curso == null)
                return;

            var aulas = curso.TodasAulas.ToList();
            var progressos = await ProgressoRepositorio.DaMatriculaAsync(matricula.Id);
            var concluidas = aulas.Count(a => progressos.Any(p => p.AulaId == a.Id && p.Concluida));

            matricula.ProgressoPercentual = aulas.Count == 0 ? 0 : Math.Round(concluidas * 100.0 / aulas.Count);

            var provas = (await ProvaRepositorio.DosCursosAsync(new[] { curso.Id })).Where(p => p.IsPublicada).ToList();
            var resultados = (await ResultadoRepositorio.DoAlunoAsync(matricula.AlunoId)).Where(r => r.CursoId == curso.Id).ToList();
            var notas = resultados
                .Select(r => (Resultado: r, Prova: provas.FirstOrDefault(p => p.Id == r.ProvaId)))
                .Where(n => n.Prova != null)
                .Select(n => (n.Resultado, n.Prova!))
                .ToList();

            // Aprovado quando cada matéria com prova publicada tem média >= nota de corte.
            var aprovado = provas.GroupBy(p => p.MateriaId).All(grupo =>
            {
                var daMateria = notas.Where(n => n.Item2.MateriaId == grupo.Key).ToList();
                var corte = grupo.Max(p => p.NotaCorte / Math.Max(1, p.NotaMaxima) * 10);
                return daMateria.Count > 0 && Media(daMateria) >= corte;
            });

            var concluiu = aulas.Count > 0 && concluidas == aulas.Count && aprovado;
            if (concluiu && matricula.Status == StatusMatricula.Ativa)
            {
                matricula.Status = StatusMatricula.Concluida;
                matricula.DataConclusao = DateTime.Now;
            }

            await MatriculaRepositorio.AtualizarAsync(matricula);

            if (matricula.Status == StatusMatricula.Concluida && !await CertificadoRepositorio.ExisteParaMatriculaAsync(matricula.Id))
            {
                var aluno = await UsuarioRepositorio.PorIdAsync(matricula.AlunoId);
                await CertificadoRepositorio.InserirAsync(new Certificado
                {
                    MatriculaId = matricula.Id,
                    CursoId = curso.Id,
                    AlunoId = matricula.AlunoId,
                    Titulo = curso.Nome,
                    AlunoNome = aluno?.Nome ?? string.Empty,
                    CursoNome = curso.Nome,
                    CargaHorariaHoras = curso.CargaHorariaHoras,
                    MediaFinal = Math.Round(Media(notas), 1),
                    EmitidoEm = DateTime.Now
                });
            }
        }
    }
}
