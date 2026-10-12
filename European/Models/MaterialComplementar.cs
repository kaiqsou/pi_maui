using European.Enums;

namespace European.Models
{
    public class MaterialComplementar
    {
        public string Id { get; set; } = string.Empty;
        public string AulaId { get; set; } = string.Empty;
        public Aula? Aula { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public TipoMaterial Tipo { get; set; }
        public int Ordem { get; set; }
    }
}
