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
        public double NotaObtida { get; set; }
        public DateTime DataRealizacao { get; set; } = DateTime.Now;
        public StatusProva Status { get; set; } = StatusProva.Corrigida;
    }
}
