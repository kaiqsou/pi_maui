using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public partial class AlternativaEdicao : ObservableObject
    {
        public QuestaoEdicao Questao { get; init; } = null!;

        [ObservableProperty]
        public partial string Texto { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool Correta { get; set; }

        [ObservableProperty]
        public partial string Letra { get; set; } = "A";
    }

    /// <summary>Questão sendo editada na tela de prova (os números ficam em texto enquanto o professor digita).</summary>
    public partial class QuestaoEdicao : ObservableObject
    {
        [ObservableProperty]
        public partial int Numero { get; set; }

        [ObservableProperty]
        public partial string Enunciado { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Peso { get; set; } = "1";

        [ObservableProperty]
        public partial string RespostaEsperada { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDissertativa), nameof(IsAlternativas))]
        public partial TipoQuestao Tipo { get; set; } = TipoQuestao.Alternativas;

        public bool IsDissertativa => Tipo == TipoQuestao.Dissertativa;
        public bool IsAlternativas => Tipo == TipoQuestao.Alternativas;
        public ObservableCollection<AlternativaEdicao> Alternativas { get; } = new();

        public double PesoValor => Formatos.LerNumero(Peso, 1);

        /// <summary>Questão nova: quatro alternativas, a primeira marcada como correta.</summary>
        public static QuestaoEdicao Nova()
        {
            var questao = new QuestaoEdicao();
            for (var i = 0; i < 4; i++)
                questao.NovaAlternativa(correta: i == 0);
            return questao;
        }

        public static QuestaoEdicao De(Questao questao)
        {
            var edicao = new QuestaoEdicao
            {
                Enunciado = questao.Enunciado,
                Peso = Formatos.Numero(questao.Peso),
                Tipo = questao.Tipo,
                RespostaEsperada = questao.RespostaEsperada
            };
            for (var i = 0; i < questao.Alternativas.Count; i++)
                edicao.NovaAlternativa(questao.Alternativas[i], i == questao.IndiceCorreta);

            // dissertativa salva sem alternativas: deixa alternativas em branco caso o professor troque o tipo
            while (edicao.Alternativas.Count < 2)
                edicao.NovaAlternativa(correta: edicao.Alternativas.Count == 0);
            return edicao;
        }

        public Questao ParaQuestao() => new()
        {
            Tipo = Tipo,
            Enunciado = Enunciado.Trim(),
            Peso = PesoValor,
            Alternativas = IsDissertativa ? new List<string>() : Alternativas.Select(a => a.Texto.Trim()).ToList(),
            IndiceCorreta = IsDissertativa ? -1 : Alternativas.ToList().FindIndex(a => a.Correta),
            RespostaEsperada = IsDissertativa ? RespostaEsperada.Trim() : string.Empty
        };

        /// <summary>Primeiro problema encontrado na questão, ou null se está pronta para salvar.</summary>
        public string? Validar()
        {
            if (string.IsNullOrWhiteSpace(Enunciado))
                return $"Escreva o enunciado da questão {Numero}.";
            if (IsAlternativas && (Alternativas.Count < 2 || Alternativas.Any(a => string.IsNullOrWhiteSpace(a.Texto))))
                return $"Preencha todas as alternativas da questão {Numero} (mínimo 2).";
            if (IsAlternativas && Alternativas.Count(a => a.Correta) != 1)
                return $"Marque a alternativa correta da questão {Numero}.";
            if (PesoValor <= 0)
                return $"O peso da questão {Numero} deve ser maior que zero.";
            return null;
        }

        public AlternativaEdicao NovaAlternativa(string texto = "", bool correta = false)
        {
            var alternativa = new AlternativaEdicao { Questao = this, Texto = texto, Correta = correta };
            Alternativas.Add(alternativa);
            Renumerar();
            return alternativa;
        }

        public void RemoverAlternativa(AlternativaEdicao alternativa)
        {
            if (Alternativas.Count <= 2)
                return;

            Alternativas.Remove(alternativa);
            if (alternativa.Correta)
                Alternativas[0].Correta = true;
            Renumerar();
        }

        public void MarcarCorreta(AlternativaEdicao alternativa)
        {
            foreach (var outra in Alternativas)
                outra.Correta = ReferenceEquals(outra, alternativa);
        }

        private void Renumerar()
        {
            for (var i = 0; i < Alternativas.Count; i++)
                Alternativas[i].Letra = Formatos.Letra(i);
        }
    }
}
