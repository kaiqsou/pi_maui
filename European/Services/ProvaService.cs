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

        /// <summary>
        /// Monta a entrega do aluno. As questões de alternativas já saem corrigidas (nota 10 ou 0);
        /// as dissertativas ficam sem nota. A entrega sempre fica aguardando o professor concluir a correção.
        /// </summary>
        public static ResultadoProva Entregar(Prova prova, string alunoId, IReadOnlyList<int> respostas,
            IReadOnlyList<string> textos, bool tempoEsgotado)
        {
            var notas = new List<double?>();
            var acertos = 0;

            for (var i = 0; i < prova.Questoes.Count; i++)
            {
                var questao = prova.Questoes[i];
                if (questao.IsDissertativa)
                {
                    notas.Add(null);
                    continue;
                }

                var acertou = i < respostas.Count && respostas[i] == questao.IndiceCorreta;
                if (acertou)
                    acertos++;
                notas.Add(acertou ? 10 : 0);
            }

            var resultado = new ResultadoProva
            {
                ProvaId = prova.Id,
                AlunoId = alunoId,
                CursoId = prova.CursoId,
                MateriaId = prova.MateriaId,
                Respostas = respostas.ToList(),
                RespostasTexto = textos.Select(t => t ?? string.Empty).ToList(),
                NotasQuestoes = notas,
                Comentarios = prova.Questoes.Select(_ => string.Empty).ToList(),
                Acertos = acertos,
                TotalQuestoes = prova.Questoes.Count,
                NotaMaxima = prova.NotaMaxima,
                TempoEsgotado = tempoEsgotado,
                DataRealizacao = DateTime.Now,
                Status = StatusProva.Realizada
            };
            resultado.NotaObtida = CalcularNota(prova, resultado);
            return resultado;
        }

        /// <summary>Cada questão vale o seu peso; a nota (0 a 10) de cada uma é proporcional e o total vai até a nota máxima da prova.</summary>
        public static double CalcularNota(Prova prova, ResultadoProva resultado)
        {
            double pesoTotal = 0, pontos = 0;
            for (var i = 0; i < prova.Questoes.Count; i++)
            {
                var peso = prova.Questoes[i].Peso > 0 ? prova.Questoes[i].Peso : 1;
                pesoTotal += peso;
                var nota = i < resultado.NotasQuestoes.Count ? resultado.NotasQuestoes[i] ?? 0 : 0;
                pontos += peso * Math.Clamp(nota, 0, 10) / 10;
            }

            return pesoTotal <= 0 ? 0 : Math.Round(pontos / pesoTotal * prova.NotaMaxima, 2);
        }

        /// <summary>
        /// Ordem e número fixos para a correção às cegas: não seguem a ordem de entrega nem o nome,
        /// para o professor não conseguir deduzir quem é o aluno.
        /// </summary>
        public static List<ResultadoProva> OrdemAnonima(IEnumerable<ResultadoProva> resultados)
            => resultados.OrderBy(r => Embaralhar(r.Id)).ToList();

        private static uint Embaralhar(string texto)
        {
            // FNV-1a: estável entre execuções (string.GetHashCode muda a cada execução)
            var hash = 2166136261u;
            foreach (var c in texto)
                hash = (hash ^ c) * 16777619u;
            return hash;
        }
    }
}
