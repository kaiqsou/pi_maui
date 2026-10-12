namespace European.Models
{
    public class Modulo
    {
        public string Id { get; set; } = string.Empty;
        public string CursoId { get; set; } = string.Empty;
        public Curso? Curso { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public List<Materia> Materias { get; set; } = new();
    }
}
