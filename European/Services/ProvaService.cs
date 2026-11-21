using European.Data;
using European.Enums;
using European.Models;

namespace European.Services
{
    /// <summary>Uma prova no calendário do aluno, com a janela em que ele pode fazê-la.</summary>
    public sealed record ProvaDoAluno(Prova Prova, Matricula Matricula, DateTime LiberadaEm, DateTime PrazoAte)
    {
        public bool Liberada => LiberadaEm <= DateTime.Now;
        public bool Encerrada => PrazoAte <= DateTime.Now;
        public bool Aberta => Liberada && !Encerrada;
        public string PrazoTexto => PrazoAte.ToString("dd/MM/yyyy HH:mm");
        public string LiberacaoTexto => LiberadaEm.ToString("dd/MM/yyyy");
    }

    public static class ProvaService
    {
        public static bool PublicadaComQuestoes(Prova prova) => prova.IsPublicada && prova.Questoes.Count > 0;

        /// <summary>
        /// Calendário de provas da matrícula. As provas publicadas do curso entram na ordem das matérias (e, na mesma
        /// matéria, na ordem de criação). A primeira é liberada quando o acesso do aluno é liberado (o calendário é
        /// montado nessa hora, em ContaService); cada uma das seguintes é liberada quando termina o prazo da anterior,
        /// e fica aberta por Prova.PrazoDias.
        ///
        /// O calendário é gravado na matrícula. As provas já liberadas mantêm as datas; as que ainda não foram liberadas
        /// são recalculadas a cada vez, então uma prova que o professor publicar depois entra na fila no lugar certo,
        /// e uma prova despublicada sai dela.
        /// </summary>
        public static async Task<List<ProvaDoAluno>> AgendaAsync(Matricula matricula)
        {
            var curso = matricula.Curso ?? await CursoRepositorio.PorIdAsync(matricula.CursoId);
            if (curso == null)
                return new();

            var provas = OrdenarDoCurso(await ProvaRepositorio.DosCursosAsync(new[] { curso.Id }), curso)
                .Where(PublicadaComQuestoes)
                .ToList();
            var porId = provas.ToDictionary(p => p.Id);

            var agora = DateTime.Now;
            var agenda = matricula.AgendaProvas
                .Where(a => a.LiberadaEm <= agora && porId.ContainsKey(a.ProvaId))
                .OrderBy(a => a.LiberadaEm)
                .ToList();

            // nenhuma prova nova é liberada com data no passado: se o professor publicar depois que o calendário do
            // aluno acabou (ou num curso que não tinha provas), ela abre agora, com o prazo inteiro
            var inicio = agenda.Count == 0 ? matricula.DataMatricula : agenda[^1].PrazoAte;
            if (inicio < agora)
                inicio = agora;
            foreach (var prova in provas.Where(p => agenda.All(a => a.ProvaId != p.Id)))
            {
                var fim = inicio.AddDays(Math.Max(1, prova.PrazoDias));
                agenda.Add(new ProvaAgendada { ProvaId = prova.Id, LiberadaEm = inicio, PrazoAte = fim });
                inicio = fim;
            }

            if (!MesmaAgenda(agenda, matricula.AgendaProvas))
            {
                matricula.AgendaProvas = agenda;
                await MatriculaRepositorio.AtualizarAgendaAsync(matricula);
            }

            return agenda.Select(a =>
            {
                var prova = porId[a.ProvaId];
                prova.Materia = curso.TodasMaterias.FirstOrDefault(m => m.Id == prova.MateriaId);
                return new ProvaDoAluno(prova, matricula, a.LiberadaEm, a.PrazoAte);
            }).ToList();
        }

        /// <summary>
        /// Ordem das provas do curso (a da aba Provas do professor e a de liberação para o aluno): a posição definida
        /// pelo professor; provas sem posição vão para o fim, na ordem das matérias e de criação.
        /// Provas de matérias que não existem mais ficam de fora.
        /// </summary>
        public static List<Prova> OrdenarDoCurso(IEnumerable<Prova> provas, Curso curso)
        {
            var ordemMaterias = curso.TodasMaterias.Select((m, i) => (m.Id, i)).ToDictionary(x => x.Id, x => x.i);
            return provas
                .Where(p => p.CursoId == curso.Id && ordemMaterias.ContainsKey(p.MateriaId))
                .OrderBy(p => p.Ordem > 0 ? p.Ordem : int.MaxValue)
                .ThenBy(p => ordemMaterias[p.MateriaId])
                .ThenBy(p => p.CreatedAt)
                .ToList();
        }

        /// <summary>Calendário de todas as matrículas informadas, em ordem de liberação.</summary>
        public static async Task<List<ProvaDoAluno>> AgendaAsync(IEnumerable<Matricula> matriculas)
        {
            var todas = new List<ProvaDoAluno>();
            foreach (var matricula in matriculas)
                todas.AddRange(await AgendaAsync(matricula));
            return todas.OrderBy(p => p.LiberadaEm).ToList();
        }

        private static bool MesmaAgenda(IReadOnlyList<ProvaAgendada> a, IReadOnlyList<ProvaAgendada> b)
            => a.Count == b.Count && a.Zip(b).All(x => x.First.ProvaId == x.Second.ProvaId
                                                      && x.First.LiberadaEm == x.Second.LiberadaEm
                                                      && x.First.PrazoAte == x.Second.PrazoAte);

        /// <summary>
        /// Resultados do aluno somados às provas que ele perdeu (prazo encerrado sem entrega), que contam como nota 0.
        /// Assim a matéria fecha mesmo com prova perdida, em vez de travar o certificado para sempre.
        /// </summary>
        public static List<ResultadoProva> ComPerdidas(IEnumerable<ProvaDoAluno> agenda, IEnumerable<ResultadoProva> resultados)
        {
            var lista = resultados.ToList();
            var entregues = lista.Select(r => r.ProvaId).ToHashSet();
            lista.AddRange(agenda
                .Where(p => p.Encerrada && !entregues.Contains(p.Prova.Id))
                .Select(p => new ResultadoProva
                {
                    ProvaId = p.Prova.Id,
                    AlunoId = p.Matricula.AlunoId,
                    CursoId = p.Prova.CursoId,
                    MateriaId = p.Prova.MateriaId,
                    NotaObtida = 0,
                    NotaMaxima = p.Prova.NotaMaxima,
                    TotalQuestoes = p.Prova.Questoes.Count,
                    Status = StatusProva.Corrigida
                }));
            return lista;
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

        /// <summary>Entregas antigas (antes das dissertativas) não guardavam a nota por questão: refaz pelas alternativas.</summary>
        public static void CompletarNotasAntigas(Prova prova, ResultadoProva resultado)
        {
            if (resultado.NotasQuestoes.Count > 0)
                return;

            resultado.NotasQuestoes = prova.Questoes
                .Select((q, i) => q.IsDissertativa
                    ? (double?)null
                    : i < resultado.Respostas.Count && resultado.Respostas[i] == q.IndiceCorreta ? 10 : 0)
                .ToList();
        }

        /// <summary>
        /// Grava a correção (notas e comentários já preenchidos no resultado). Ao concluir, a nota é liberada ao aluno
        /// e a média/aprovação dele é recalculada (o que pode emitir o certificado).
        /// </summary>
        public static async Task SalvarCorrecaoAsync(Prova prova, ResultadoProva resultado, bool concluir)
        {
            resultado.NotaObtida = CalcularNota(prova, resultado);
            if (concluir)
            {
                resultado.Status = StatusProva.Corrigida;
                resultado.CorrigidaEm = DateTime.Now;
            }

            await ResultadoRepositorio.AtualizarAsync(resultado);

            if (concluir)
                await NotificacaoService.ParaUsuarioAsync(resultado.AlunoId, "Nota liberada",
                    $"A correção da prova \"{prova.Titulo}\" foi concluída. Veja a nota em Provas › Realizadas.");

            if (resultado.Corrigida)
            {
                var matricula = await MatriculaRepositorio.PorAlunoECursoAsync(resultado.AlunoId, resultado.CursoId);
                if (matricula != null)
                    await ProgressoService.AtualizarAsync(matricula);
            }
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
