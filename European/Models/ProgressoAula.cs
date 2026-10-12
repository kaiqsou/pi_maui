namespace European.Models
{
    public class ProgressoAula
    {
        public string Id { get; set; } = string.Empty;
        public string MatriculaId { get; set; } = string.Empty;
        public string AulaId { get; set; } = string.Empty;
        public double PercentualAssistido { get; set; }
        public double UltimaPosicaoSegundos { get; set; }
        public bool Concluida { get; set; }
        public DateTime AtualizadoEm { get; set; } = DateTime.Now;
    }
}
