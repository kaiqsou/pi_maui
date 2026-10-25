namespace European.Models
{
    public class Certificado
    {
        public string Id { get; set; } = string.Empty;
        public string MatriculaId { get; set; } = string.Empty;
        public string CursoId { get; set; } = string.Empty;
        public string AlunoId { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string AlunoNome { get; set; } = string.Empty;
        public string CursoNome { get; set; } = string.Empty;
        public int CargaHorariaHoras { get; set; }
        public double MediaFinal { get; set; }
        public string CodigoVerificacao { get; set; } = string.Empty;
        public DateTime EmitidoEm { get; set; } = DateTime.Now;
        public string? UrlPdf { get; set; }

        public string TipoTexto => "Certificado de conclusão do mestrado";
    }
}
