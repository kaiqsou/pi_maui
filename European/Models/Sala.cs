namespace European.Models
{
    public class Sala
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string ProfessorId { get; set; } = string.Empty;
        public string? CursoId { get; set; }
        public List<string> AlunoIds { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
