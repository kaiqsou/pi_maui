namespace European.Models
{
    public class Materia
    {
        public string Id { get; set; } = string.Empty;
        public string ModuloId { get; set; } = string.Empty;
        public Modulo? Modulo { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public List<Aula> Aulas { get; set; } = new();
    }
}
