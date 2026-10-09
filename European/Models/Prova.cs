using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace European.Models
{
    public class Prova
    {
        public int Id { get; set; }
        public Materia Materia { get; set; }
        public float TempoProva { get; set; }
        public Usuario? Aluno { get; set; }
        public int AlunoId { get; set; }
        public DateTime Prazo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
