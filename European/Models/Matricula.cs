using European.Enums;

namespace European.Models
{
    public class Matricula
    {
        public string Id { get; set; } = string.Empty;
        public string AlunoId { get; set; } = string.Empty;
        public Usuario? Aluno { get; set; }
        public string CursoId { get; set; } = string.Empty;
        public Curso? Curso { get; set; }
        public StatusMatricula Status { get; set; } = StatusMatricula.Ativa;
        public double ProgressoPercentual { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime DataMatricula { get; set; } = DateTime.Now;
        public DateTime? DataConclusao { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
