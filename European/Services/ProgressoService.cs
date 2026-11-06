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

        /// <summary>Junta cada resultado à sua prova, descartando resultados de provas que não estão na lista.</summary>
        public static List<(ResultadoProva Resultado, Prova Prova)> ComProva(IEnumerable<ResultadoProva> resultados, IEnumerable<Prova> provas)
        {
            var porId = provas.ToDictionary(p => p.Id);
            return resultados
                .Where(r => porId.ContainsKey(r.ProvaId))
                .Select(r => (r, porId[r.ProvaId]))
                .ToList();
        }

        /// <summary>Situação do aluno numa matéria: média ponderada das provas corrigidas e aprovação pela média.</summary>
        public sealed record AvaliacaoMateria(double Media, double NotaCorte, bool Completa, bool Aprovada);

        /// <summary>
        /// Regra única de aprovação (Desempenho e certificado): a matéria é aprovada quando todas as provas
        /// exigidas dela foram corrigidas e a média ponderada (0 a 10, pelo peso de cada prova) atinge a nota de corte.
        /// Se as provas tiverem notas de corte diferentes, vale a maior (convertida para 0 a 10).
        /// </summary>
        public static AvaliacaoMateria AvaliarMateria(IReadOnlyCollection<Prova> provasExigidas, IEnumerable<ResultadoProva> corrigidos)
        {
            var notas = ComProva(corrigidos.Where(r => r.Corrigida), provasExigidas);

            var media = Math.Round(Media(notas), 1);
            var corte = provasExigidas.Count == 0
                ? NotaCortePadrao
                : provasExigidas.Max(p => p.NotaCorte / Math.Max(1, p.NotaMaxima) * 10);
            var completa = provasExigidas.All(p => notas.Any(n => n.Prova.Id == p.Id));

            return new AvaliacaoMateria(media, Math.Round(corte, 1), completa, completa && provasExigidas.Count > 0 && media >= corte);
        }

        /// <summary>Quanto falta para o certificado final: módulos com todas as aulas concluídas e matérias aprovadas pela média.</summary>
        public sealed record SituacaoCertificado(
            int AulasConcluidas, int TotalAulas,
            int ModulosConcluidos, int TotalModulos,
            int MateriasAprovadas, int TotalMateriasComProva,
            double MediaFinal)
        {
            public bool Apto => TotalModulos > 0 && ModulosConcluidos == TotalModulos && MateriasAprovadas == TotalMateriasComProva;
        }

        public static async Task<SituacaoCertificado> SituacaoAsync(Matricula matricula, Curso curso)
        {
            var progressos = await ProgressoRepositorio.DaMatriculaAsync(matricula.Id);
            var concluidas = progressos.Where(p => p.Concluida).Select(p => p.AulaId).ToHashSet();

            var aulas = curso.TodasAulas.ToList();
            // módulos ainda sem aulas não contam: não há o que finalizar neles
            var modulos = curso.Modulos.Where(m => m.Materias.Any(x => x.Aulas.Count > 0)).ToList();
            var modulosConcluidos = modulos.Count(m => m.Materias.SelectMany(x => x.Aulas).All(a => concluidas.Contains(a.Id)));

            // provas exigidas: as publicadas que o aluno enxerga (do curso inteiro ou da sala dele)
            var provas = await ProvaService.ExigidasDoAlunoAsync(matricula.AlunoId, new[] { curso.Id });
            // só contam as provas já corrigidas pelo professor
            var resultados = (await ResultadoRepositorio.DoAlunoAsync(matricula.AlunoId))
                .Where(r => r.CursoId == curso.Id && r.Corrigida).ToList();

            var notas = ComProva(resultados, provas);

            // aprovação pela média de cada matéria que tem prova (uma prova baixa pode ser compensada pelas outras)
            var materias = provas.GroupBy(p => p.MateriaId).ToList();
            var aprovadas = materias.Count(g => AvaliarMateria(g.ToList(), resultados).Aprovada);

            return new SituacaoCertificado(
                aulas.Count(a => concluidas.Contains(a.Id)), aulas.Count,
                modulosConcluidos, modulos.Count,
                aprovadas, materias.Count,
                Math.Round(Media(notas), 1));
        }

        /// <summary>Recalcula o percentual da matrícula e emite o certificado final quando todos os módulos e provas foram concluídos com aprovação.</summary>
        public static async Task AtualizarAsync(Matricula matricula)
        {
            var curso = matricula.Curso ?? await CursoRepositorio.PorIdAsync(matricula.CursoId);
            if (curso == null)
                return;

            var situacao = await SituacaoAsync(matricula, curso);
            matricula.ProgressoPercentual = situacao.TotalAulas == 0 ? 0 : Math.Round(situacao.AulasConcluidas * 100.0 / situacao.TotalAulas);

            if (situacao.Apto && matricula.Status == StatusMatricula.Ativa)
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
                    MediaFinal = situacao.MediaFinal,
                    // a validade do acesso às aulas (Matricula.AcessoAte) conta a partir desta data
                    EmitidoEm = matricula.DataConclusao ?? DateTime.Now
                });
            }
        }
    }
}
