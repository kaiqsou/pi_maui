using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class QuestaoCorrecao : BaseViewModel
    {
        private string _nota = string.Empty;
        private string _comentario = string.Empty;

        public int Indice { get; init; }
        public int Numero => Indice + 1;
        public string Enunciado { get; init; } = string.Empty;
        public string PesoTexto { get; init; } = string.Empty;
        public bool IsDissertativa { get; init; }
        public bool IsAlternativas => !IsDissertativa;

        // alternativas: já corrigidas automaticamente
        public string MarcadaTexto { get; init; } = string.Empty;
        public string CorretaTexto { get; init; } = string.Empty;
        public bool Acertou { get; init; }
        public bool Errou => IsAlternativas && !Acertou;

        // dissertativa: o professor dá a nota (0 a 10) e pode comentar
        public string RespostaAluno { get; init; } = string.Empty;
        public string RespostaEsperada { get; init; } = string.Empty;
        public bool TemRespostaEsperada => !string.IsNullOrWhiteSpace(RespostaEsperada);

        public string Nota
        {
            get => _nota;
            set
            {
                if (SetProperty(ref _nota, value))
                    NotaAlterada?.Invoke();
            }
        }

        public string Comentario { get => _comentario; set => SetProperty(ref _comentario, value); }

        public event Action? NotaAlterada;

        /// <summary>Nota 0–10 digitada (aceita vírgula); null se vazia ou inválida.</summary>
        public double? NotaValor =>
            double.TryParse(Nota.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var n) && n is >= 0 and <= 10 ? n : null;
    }

    /// <summary>
    /// Correção de uma entrega. Às cegas: a tela não mostra nome, e-mail nem horário de entrega.
    /// Alternativas já chegam corrigidas; nas dissertativas o professor dá a nota e pode comentar.
    /// </summary>
    public class CorrecaoViewModel : BaseViewModel
    {
        private Prova? _prova;
        private ResultadoProva? _resultado;
        private string _notaTotalTexto = string.Empty;

        public CorrecaoViewModel()
        {
            SalvarCommand = new Command(async () => await SalvarAsync(concluir: false));
            ConcluirCommand = new Command(async () => await SalvarAsync(concluir: true));
        }

        public ObservableCollection<QuestaoCorrecao> Questoes { get; } = new();

        public string NotaTotalTexto { get => _notaTotalTexto; private set => SetProperty(ref _notaTotalTexto, value); }
        public bool JaCorrigida => _resultado?.Corrigida == true;
        public string ConcluirTexto => JaCorrigida ? "Salvar correção" : "Concluir correção";

        public ICommand SalvarCommand { get; }
        public ICommand ConcluirCommand { get; }

        public Task CarregarAsync(Prova prova, string resultadoId, string rotulo) => ExecutarAsync(async () =>
        {
            _prova = prova;
            _resultado = await ResultadoRepositorio.PorIdAsync(resultadoId);
            Title = rotulo;
            Questoes.Clear();
            if (_resultado == null)
                return;

            // entregas antigas (antes das dissertativas) não guardavam a nota por questão: refaz pelas alternativas
            if (_resultado.NotasQuestoes.Count == 0)
            {
                _resultado.NotasQuestoes = prova.Questoes
                    .Select((q, i) => q.IsDissertativa
                        ? (double?)null
                        : i < _resultado.Respostas.Count && _resultado.Respostas[i] == q.IndiceCorreta ? 10 : 0)
                    .ToList();
            }

            for (var i = 0; i < prova.Questoes.Count; i++)
            {
                var questao = prova.Questoes[i];
                var marcada = i < _resultado.Respostas.Count ? _resultado.Respostas[i] : -1;
                var nota = i < _resultado.NotasQuestoes.Count ? _resultado.NotasQuestoes[i] : null;

                var item = new QuestaoCorrecao
                {
                    Indice = i,
                    Enunciado = questao.Enunciado,
                    PesoTexto = questao.Peso == 1 ? string.Empty : $"Peso {questao.Peso:0.#}",
                    IsDissertativa = questao.IsDissertativa,
                    MarcadaTexto = questao.IsDissertativa ? string.Empty : Alternativa(questao, marcada) ?? "Em branco",
                    CorretaTexto = questao.IsDissertativa ? string.Empty : Alternativa(questao, questao.IndiceCorreta) ?? string.Empty,
                    Acertou = !questao.IsDissertativa && marcada == questao.IndiceCorreta,
                    RespostaAluno = Texto(_resultado.RespostasTexto, i) is { Length: > 0 } t ? t : "(em branco)",
                    RespostaEsperada = questao.RespostaEsperada,
                    Nota = questao.IsDissertativa && nota.HasValue ? nota.Value.ToString("0.#", CultureInfo.InvariantCulture) : string.Empty,
                    Comentario = Texto(_resultado.Comentarios, i)
                };
                item.NotaAlterada += AtualizarTotal;
                Questoes.Add(item);
            }

            AtualizarTotal();
            OnPropertyChanged(nameof(JaCorrigida));
            OnPropertyChanged(nameof(ConcluirTexto));
        });

        private static string? Alternativa(Questao questao, int indice)
            => indice >= 0 && indice < questao.Alternativas.Count ? $"{(char)('A' + indice)}) {questao.Alternativas[indice]}" : null;

        private static string Texto(List<string> lista, int i) => i < lista.Count ? lista[i] ?? string.Empty : string.Empty;

        /// <summary>Atualiza a nota da entrega enquanto o professor digita as notas das dissertativas.</summary>
        private void AtualizarTotal()
        {
            if (_prova == null || _resultado == null)
                return;

            var nota = ProvaService.CalcularNota(_prova, ComNotasDigitadas());
            var faltam = Questoes.Count(q => q.IsDissertativa && q.NotaValor == null);
            NotaTotalTexto = faltam > 0
                ? $"Nota parcial: {nota:0.0} de {_prova.NotaMaxima:0.#} · {faltam} dissertativa(s) sem nota"
                : $"Nota final: {nota:0.0} de {_prova.NotaMaxima:0.#}";
        }

        /// <summary>Cópia das notas por questão com o que está digitado na tela.</summary>
        private ResultadoProva ComNotasDigitadas()
        {
            var notas = _resultado!.NotasQuestoes.ToList();
            while (notas.Count < Questoes.Count)
                notas.Add(null);
            foreach (var q in Questoes.Where(q => q.IsDissertativa))
                notas[q.Indice] = q.NotaValor;
            return new ResultadoProva { NotasQuestoes = notas };
        }

        private async Task SalvarAsync(bool concluir)
        {
            if (_prova == null || _resultado == null)
                return;

            var invalida = Questoes.FirstOrDefault(q => q.IsDissertativa && !string.IsNullOrWhiteSpace(q.Nota) && q.NotaValor == null);
            if (invalida != null)
            {
                await AvisarAsync("Correção", $"A nota da questão {invalida.Numero} deve ser um número de 0 a 10.");
                return;
            }

            if (concluir)
            {
                var semNota = Questoes.FirstOrDefault(q => q.IsDissertativa && q.NotaValor == null);
                if (semNota != null)
                {
                    await AvisarAsync("Correção", $"Dê a nota da questão {semNota.Numero} para concluir.");
                    return;
                }
            }

            var resultado = _resultado;
            resultado.NotasQuestoes = ComNotasDigitadas().NotasQuestoes;
            resultado.Comentarios = Questoes.Select(q => q.IsDissertativa ? q.Comentario.Trim() : string.Empty).ToList();
            resultado.NotaObtida = ProvaService.CalcularNota(_prova, resultado);
            if (concluir)
            {
                resultado.Status = StatusProva.Corrigida;
                resultado.CorrigidaEm = DateTime.Now;
            }

            var ok = await ExecutarAsync(async () =>
            {
                await ResultadoRepositorio.AtualizarAsync(resultado);

                // nota liberada: atualiza média/aprovação do aluno (e o certificado, se for o caso)
                if (resultado.Corrigida)
                {
                    var matricula = await MatriculaRepositorio.PorAlunoECursoAsync(resultado.AlunoId, resultado.CursoId);
                    if (matricula != null)
                        await ProgressoService.AtualizarAsync(matricula);
                }
            });

            if (!ok)
                return;

            await AvisarAsync("Correção", concluir
                ? $"Correção concluída. Nota: {resultado.NotaObtida:0.0} de {_prova.NotaMaxima:0.#}. O aluno já pode ver a nota."
                : "Correção salva. Você pode concluir depois.");
            await VoltarAsync();
        }
    }
}
