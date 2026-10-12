namespace European.Models
{
    public class Prova
    {
        public string Id { get; set; } = string.Empty;
        public string MateriaId { get; set; } = string.Empty;
        public Materia? Materia { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public double NotaMaxima { get; set; } = 10;
        public double Peso { get; set; } = 1;
        public double NotaCorte { get; set; } = 6;
        public DateTime Prazo { get; set; }
        public int DuracaoMinutos { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
