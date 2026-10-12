using European.Enums;

namespace European.Models
{
    public class Curso
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public CategoriaEnum Categoria { get; set; }
        public decimal Preco { get; set; }
        public string? CapaUrl { get; set; }
        public int CargaHorariaHoras { get; set; }
        public bool IsPublicado { get; set; }
        public string? ProfessorId { get; set; }
        public string? Coordenador { get; set; }
        public List<string> Docentes { get; set; } = new();
        public List<Modulo> Modulos { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
