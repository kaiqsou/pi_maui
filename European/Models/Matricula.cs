using European.Enums;

namespace European.Models
{
    public class Matricula : IAuditavel
    {
        public string Id { get; set; } = string.Empty;
        public string AlunoId { get; set; } = string.Empty;
        public string CursoId { get; set; } = string.Empty;
        public Curso? Curso { get; set; }
        public StatusMatricula Status { get; set; } = StatusMatricula.Ativa;
        public double ProgressoPercentual { get; set; }

        /// <summary>Coordenador que liberou o acesso.</summary>
        public string? LiberadaPorId { get; set; }
        public DateTime DataMatricula { get; set; } = DateTime.Now;
        public DateTime? DataConclusao { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public string CursoNome => Curso?.Nome ?? string.Empty;

        /// <summary>Depois de concluir o curso (emissão do certificado), o aluno ainda pode reassistir as aulas por este período.</summary>
        public const int MesesAcessoAposConclusao = 6;

        public DateTime? AcessoAte => Status == StatusMatricula.Concluida ? DataConclusao?.AddMonths(MesesAcessoAposConclusao) : null;

        /// <summary>Ativa, ou concluída e ainda dentro da validade de acesso às aulas.</summary>
        public bool AcessoAulasVigente => Status == StatusMatricula.Ativa
            || (Status == StatusMatricula.Concluida && AcessoAte.HasValue && AcessoAte.Value > DateTime.Now);
    }
}
