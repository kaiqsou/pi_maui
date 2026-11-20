using CommunityToolkit.Mvvm.ComponentModel;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public partial class AlternativaOpcao : ObservableObject
    {
        public QuestaoResposta Questao { get; init; } = null!;
        public int Indice { get; init; }
        public string Letra => Formatos.Letra(Indice);
        public string Texto { get; init; } = string.Empty;

        [ObservableProperty]
        public partial bool Selecionada { get; set; }
    }

    /// <summary>Questão sendo respondida pelo aluno.</summary>
    public partial class QuestaoResposta : ObservableObject
    {
        public int Numero { get; init; }
        public string Enunciado { get; init; } = string.Empty;
        public string PesoTexto { get; init; } = string.Empty;
        public bool IsDissertativa { get; init; }
        public bool IsAlternativas => !IsDissertativa;
        public List<AlternativaOpcao> Alternativas { get; } = new();

        /// <summary>Resposta escrita (questão dissertativa).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Respondida))]
        public partial string Texto { get; set; } = string.Empty;

        /// <summary>Índice da alternativa marcada (-1 = nenhuma).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Respondida))]
        public partial int Marcada { get; set; } = -1;

        /// <summary>Questão aberta agora (destaca o número no mapa de questões).</summary>
        [ObservableProperty]
        public partial bool Atual { get; set; }

        public bool EmBranco => IsDissertativa ? string.IsNullOrWhiteSpace(Texto) : Marcada < 0;
        public bool Respondida => !EmBranco;

        partial void OnMarcadaChanged(int value)
        {
            foreach (var alternativa in Alternativas)
                alternativa.Selecionada = alternativa.Indice == value;
        }

        public static QuestaoResposta De(Questao questao, int indice)
        {
            var resposta = new QuestaoResposta
            {
                Numero = indice + 1,
                Enunciado = questao.Enunciado,
                PesoTexto = Formatos.PesoQuestao(questao.Peso),
                IsDissertativa = questao.IsDissertativa
            };

            for (var j = 0; j < questao.Alternativas.Count; j++)
                resposta.Alternativas.Add(new AlternativaOpcao { Questao = resposta, Indice = j, Texto = questao.Alternativas[j] });

            return resposta;
        }
    }
}
