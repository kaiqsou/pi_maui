using European.Enums;

namespace European.Models
{
    public class Prova : IAuditavel
    {
        public string Id { get; set; } = string.Empty;
        public string CursoId { get; set; } = string.Empty;
        public string MateriaId { get; set; } = string.Empty;
        public Materia? Materia { get; set; }
        public string ProfessorId { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;
        public string Instrucoes { get; set; } = string.Empty;
        public double NotaMaxima { get; set; } = 10;
        public double Peso { get; set; } = 1;
        public double NotaCorte { get; set; } = 6;

        /// <summary>
        /// Dias que o aluno tem para fazer a prova depois que ela é liberada para ele. As provas do curso são liberadas
        /// uma de cada vez, na ordem das matérias: a primeira quando o acesso do aluno é liberado, e cada uma das
        /// seguintes quando termina o prazo da anterior (ProvaService.AgendaAsync).
        /// </summary>
        public int PrazoDias { get; set; } = PrazoDiasPadrao;

        public const int PrazoDiasPadrao = 30;

        /// <summary>Tempo para realizar a prova; 0 = sem limite.</summary>
        public int DuracaoMinutos { get; set; }

        public bool IsPublicada { get; set; }

        /// <summary>
        /// Posição da prova no curso, definida pelo professor (setas na aba Provas). É a ordem em que as provas são
        /// liberadas para o aluno. Provas antigas, sem posição (0), seguem a ordem das matérias.
        /// </summary>
        public int Ordem { get; set; }

        public List<Questao> Questoes { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public string DuracaoTexto => DuracaoMinutos > 0 ? $"{DuracaoMinutos} min" : "Sem limite";
        public string PrazoDiasTexto => PrazoDias == 1 ? "1 dia" : $"{PrazoDias} dias";
    }

    public class Questao
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public TipoQuestao Tipo { get; set; } = TipoQuestao.Alternativas;
        public string Enunciado { get; set; } = string.Empty;
        public double Peso { get; set; } = 1;
        public List<string> Alternativas { get; set; } = new();
        public int IndiceCorreta { get; set; }

        /// <summary>Dissertativa: resposta de referência, visível só para o professor na correção.</summary>
        public string RespostaEsperada { get; set; } = string.Empty;

        public bool IsDissertativa => Tipo == TipoQuestao.Dissertativa;
    }
}
