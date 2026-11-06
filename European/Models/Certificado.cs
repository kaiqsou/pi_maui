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
        /// <summary>Guardada para consulta (histórico); não é impressa no certificado.</summary>
        public double MediaFinal { get; set; }

        /// <summary>Início e fim do curso (período impresso no certificado). Vazios em certificados antigos.</summary>
        public DateTime? DataInicio { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string CodigoVerificacao { get; set; } = string.Empty;
        public DateTime EmitidoEm { get; set; } = DateTime.Now;

        public string TipoTexto => "Certificado de conclusão do mestrado";

        /// <summary>"de 05/03/2025 a 10/11/2026", quando as datas são conhecidas.</summary>
        public string PeriodoTexto => DataInicio is { } inicio
            ? $"de {inicio:dd/MM/yyyy} a {(DataConclusao ?? EmitidoEm):dd/MM/yyyy}"
            : string.Empty;

        /// <summary>Até quando o aluno pode reassistir as aulas: a data da matrícula concluída ou, sem ela, a contada da emissão.</summary>
        public DateTime AcessoAulasAte(Matricula? matricula)
            => matricula?.AcessoAte ?? EmitidoEm.AddMonths(Matricula.MesesAcessoAposConclusao);
    }
}
