using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace European.Models
{
    public class Matricula
    {
        public int Id { get; set; }
        public Usuario Aluno { get; set; }
        public int AlunoId { get; set; }
        public Curso Curso { get; set; }
        public int CursoId { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set;} = DateTime.Now;
    }
}
