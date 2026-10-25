using European.Data;
using European.Enums;
using European.Models;

namespace European.Services
{
    public static class ProvaService
    {
        /// <summary>Provas publicadas que o aluno pode ver: do curso em que está matriculado e, se a prova for de uma sala, só para os alunos dela.</summary>
        public static async Task<List<Prova>> VisiveisParaAlunoAsync(string alunoId, IEnumerable<Matricula> matriculasAtivas)
        {
            var cursos = matriculasAtivas.Where(m => m.Status == StatusMatricula.Ativa).ToDictionary(m => m.CursoId, m => m.Curso);
            var salas = (await SalaRepositorio.DoAlunoAsync(alunoId)).Select(s => s.Id).ToHashSet();
            var provas = await ProvaRepositorio.DosCursosAsync(cursos.Keys);

            var visiveis = provas
                .Where(p => p.IsPublicada && p.Questoes.Count > 0)
                .Where(p => string.IsNullOrEmpty(p.SalaId) || salas.Contains(p.SalaId))
                .ToList();

            foreach (var prova in visiveis)
                prova.Materia = cursos.GetValueOrDefault(prova.CursoId)?.TodasMaterias.FirstOrDefault(m => m.Id == prova.MateriaId);

            return visiveis.OrderBy(p => p.Prazo ?? DateTime.MaxValue).ToList();
        }

        /// <summary>Corrige as respostas: cada questão vale o seu peso; a nota é proporcional à nota máxima da prova.</summary>
        public static ResultadoProva Corrigir(Prova prova, string alunoId, IReadOnlyList<int> respostas, bool tempoEsgotado)
        {
            double pesoTotal = 0, pesoAcertos = 0;
            var acertos = 0;

            for (var i = 0; i < prova.Questoes.Count; i++)
            {
                var questao = prova.Questoes[i];
                var peso = questao.Peso > 0 ? questao.Peso : 1;
                pesoTotal += peso;

                if (i < respostas.Count && respostas[i] == questao.IndiceCorreta)
                {
                    acertos++;
                    pesoAcertos += peso;
                }
            }

            return new ResultadoProva
            {
                ProvaId = prova.Id,
                AlunoId = alunoId,
                CursoId = prova.CursoId,
                MateriaId = prova.MateriaId,
                Respostas = respostas.ToList(),
                Acertos = acertos,
                TotalQuestoes = prova.Questoes.Count,
                NotaMaxima = prova.NotaMaxima,
                NotaObtida = pesoTotal <= 0 ? 0 : Math.Round(pesoAcertos / pesoTotal * prova.NotaMaxima, 2),
                TempoEsgotado = tempoEsgotado,
                DataRealizacao = DateTime.Now,
                Status = StatusProva.Corrigida
            };
        }
    }
}
