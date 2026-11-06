namespace European.Models
{
    public class Certificado : IEntidade
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

        public string TipoTexto => "Certificado de conclusão do mestrado";

        /// <summary>Até quando o aluno pode reassistir as aulas: a data da matrícula concluída ou, sem ela, a contada da emissão.</summary>
        public DateTime AcessoAulasAte(Matricula? matricula)
            => matricula?.AcessoAte ?? EmitidoEm.AddMonths(Matricula.MesesAcessoAposConclusao);
    }
}
