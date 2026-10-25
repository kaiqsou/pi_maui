namespace European.Models
{
    public class Prova
    {
        public string Id { get; set; } = string.Empty;
        public string CursoId { get; set; } = string.Empty;
        public string MateriaId { get; set; } = string.Empty;
        public Materia? Materia { get; set; }
        public string ProfessorId { get; set; } = string.Empty;

        /// <summary>Quando definida, só os alunos da sala veem a prova; senão, todos os alunos do curso.</summary>
        public string? SalaId { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public string Instrucoes { get; set; } = string.Empty;
        public double NotaMaxima { get; set; } = 10;
        public double Peso { get; set; } = 1;
        public double NotaCorte { get; set; } = 6;
        public DateTime? Prazo { get; set; }

        /// <summary>Tempo para realizar a prova; 0 = sem limite.</summary>
        public int DuracaoMinutos { get; set; }

        public bool IsPublicada { get; set; }
        public List<Questao> Questoes { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool Expirada => Prazo.HasValue && Prazo.Value < DateTime.Now;
        public string DuracaoTexto => DuracaoMinutos > 0 ? $"{DuracaoMinutos} min" : "Sem limite";
        public string PrazoTexto => Prazo?.ToString("dd/MM/yyyy HH:mm") ?? "Sem prazo";
    }

    public class Questao
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Enunciado { get; set; } = string.Empty;
        public double Peso { get; set; } = 1;
        public List<string> Alternativas { get; set; } = new();
        public int IndiceCorreta { get; set; }
    }
}
