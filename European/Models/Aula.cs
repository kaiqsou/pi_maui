namespace European.Models
{
    public class Aula
    {
        public string Id { get; set; } = string.Empty;
        public string MateriaId { get; set; } = string.Empty;
        public Materia? Materia { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? VideoId { get; set; }
        public int DuracaoSegundos { get; set; }
        public int Ordem { get; set; }
        public bool IsGratuita { get; set; }
    }
}
