using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using European.Enums;

namespace European.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public Roles Roles { get; set; }
    }
}
