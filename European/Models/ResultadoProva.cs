using European.Enums;

namespace European.Models
{
    public class ResultadoProva
    {
        public string Id { get; set; } = string.Empty;
        public string ProvaId { get; set; } = string.Empty;
        public Prova? Prova { get; set; }
        public string AlunoId { get; set; } = string.Empty;
        public Usuario? Aluno { get; set; }
        public string CursoId { get; set; } = string.Empty;
        public string MateriaId { get; set; } = string.Empty;

        /// <summary>Índice da alternativa marcada em cada questão (-1 = em branco), na ordem da prova.</summary>
        public List<int> Respostas { get; set; } = new();
        public int Acertos { get; set; }
        public int TotalQuestoes { get; set; }
        public double NotaObtida { get; set; }
        public double NotaMaxima { get; set; } = 10;
        public bool TempoEsgotado { get; set; }
        public DateTime DataRealizacao { get; set; } = DateTime.Now;
        public StatusProva Status { get; set; } = StatusProva.Corrigida;

        /// <summary>Nota normalizada de 0 a 10, usada nas médias.</summary>
        public double NotaNormalizada => NotaMaxima <= 0 ? 0 : NotaObtida / NotaMaxima * 10;
    }
}
