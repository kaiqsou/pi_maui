using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace European.Models
{
    public class Curso
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco {  get; set; }
        public List<Materia> Materias { get; set; } = new List<Materia>();
    }
}
