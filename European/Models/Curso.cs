using European.Enums;

namespace European.Models
{
    public class Curso : IAuditavel
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public CategoriaEnum Categoria { get; set; }
        public decimal Preco { get; set; }
        public string? CapaUrl { get; set; }
        public int CargaHorariaHoras { get; set; }
        public bool IsPublicado { get; set; }
        public List<string> ProfessorIds { get; set; } = new();
        public List<Modulo> Modulos { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public IEnumerable<Materia> TodasMaterias => Modulos.OrderBy(m => m.Ordem).SelectMany(m => m.Materias.OrderBy(x => x.Ordem));
        public IEnumerable<Aula> TodasAulas => TodasMaterias.SelectMany(m => m.Aulas.OrderBy(a => a.Ordem));
        public int TotalMaterias => Modulos.Sum(m => m.Materias.Count);
        public int TotalAulas => Modulos.Sum(m => m.Materias.Sum(x => x.Aulas.Count));

        /// <summary>Restaura as referências Modulo.Curso / Materia.Modulo / Aula.Materia, que não são gravadas no banco.</summary>
        public Curso Vincular()
        {
            foreach (var modulo in Modulos)
            {
                modulo.Curso = this;
                modulo.CursoId = Id;
                foreach (var materia in modulo.Materias)
                {
                    materia.Modulo = modulo;
                    materia.ModuloId = modulo.Id;
                    foreach (var aula in materia.Aulas)
                    {
                        aula.Materia = materia;
                        aula.MateriaId = materia.Id;
                    }
                }
            }

            return this;
        }
    }
}
