using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace European.Models
{
    public class Materia
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public Curso Curso { get; set; }
        public int Semestre { get; set; }
        public List<Usuario> Alunos { get; set; } = new List<Usuario>();
    }
}
