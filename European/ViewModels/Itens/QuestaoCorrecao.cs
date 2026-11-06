using CommunityToolkit.Mvvm.ComponentModel;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Questão de uma entrega na tela de correção.</summary>
    public partial class QuestaoCorrecao : ObservableObject
    {
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

        [ObservableProperty]
        public partial string Nota { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Comentario { get; set; } = string.Empty;

        /// <summary>Avisa a tela para recalcular a nota total enquanto o professor digita.</summary>
        public event Action? NotaAlterada;

        partial void OnNotaChanged(string value) => NotaAlterada?.Invoke();

        /// <summary>Nota 0–10 digitada (aceita vírgula); null se vazia ou inválida.</summary>
        public double? NotaValor => Formatos.LerNumero(Nota) is { } n && n is >= 0 and <= 10 ? n : null;

        public static QuestaoCorrecao De(Questao questao, int indice, ResultadoProva resultado)
        {
            var marcada = indice < resultado.Respostas.Count ? resultado.Respostas[indice] : -1;
            var nota = indice < resultado.NotasQuestoes.Count ? resultado.NotasQuestoes[indice] : null;

            return new QuestaoCorrecao
            {
                Indice = indice,
                Enunciado = questao.Enunciado,
                PesoTexto = Formatos.PesoQuestao(questao.Peso),
                IsDissertativa = questao.IsDissertativa,
                MarcadaTexto = questao.IsDissertativa ? string.Empty : Alternativa(questao, marcada) ?? "Em branco",
                CorretaTexto = questao.IsDissertativa ? string.Empty : Alternativa(questao, questao.IndiceCorreta) ?? string.Empty,
                Acertou = !questao.IsDissertativa && marcada == questao.IndiceCorreta,
                RespostaAluno = Item(resultado.RespostasTexto, indice) is { Length: > 0 } texto ? texto : "(em branco)",
                RespostaEsperada = questao.RespostaEsperada,
                Nota = questao.IsDissertativa && nota.HasValue ? Formatos.Numero(nota.Value) : string.Empty,
                Comentario = Item(resultado.Comentarios, indice)
            };
        }

        private static string? Alternativa(Questao questao, int indice)
            => indice >= 0 && indice < questao.Alternativas.Count ? $"{Formatos.Letra(indice)}) {questao.Alternativas[indice]}" : null;

        private static string Item(List<string> lista, int indice) => indice < lista.Count ? lista[indice] ?? string.Empty : string.Empty;
    }
}
