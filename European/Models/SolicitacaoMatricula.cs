using European.Enums;

namespace European.Models
{
    /// <summary>
    /// Pedido de matrícula feito pelo app. Enquanto a coordenação não libera, o candidato NÃO é um usuário: os dados
    /// ficam só aqui. Etapas: documentos (fotos) → pagamento → análise da coordenação, que libera (vira
    /// Usuario + Matricula e a solicitação é apagada), devolve para correção ou cancela com reembolso.
    /// </summary>
    public class SolicitacaoMatricula : IEntidade
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public string CursoId { get; set; } = string.Empty;

        public StatusSolicitacao Status { get; set; } = StatusSolicitacao.Documentos;

        /// <summary>Arquivos (fotos ou PDF) dos documentos exigidos (DocumentosMatricula.Exigidos), guardados no GridFS.</summary>
        public List<DocumentoEnviado> Documentos { get; set; } = new();

        // ---- Pagamento. Os dados do cartão nunca são gravados. ----

        public FormaPagamento? FormaPagamento { get; set; }
        public int? Parcelas { get; set; }
        public DateTime? PagamentoEm { get; set; }
        public decimal? ValorPago { get; set; }

        /// <summary>Código da transação (comprovante), usado para conferir e reembolsar.</summary>
        public string? TransacaoId { get; set; }

        // ---- Devolução e cancelamento ----

        /// <summary>Motivo informado pela coordenação ao devolver; some quando o candidato reenvia.</summary>
        public string? PendenciaMotivo { get; set; }
        public DateTime? PendenciaEm { get; set; }

        public DateTime? CanceladaEm { get; set; }
        public string? CanceladaMotivo { get; set; }

        /// <summary>Código do reembolso feito ao cancelar uma solicitação paga.</summary>
        public string? ReembolsoId { get; set; }
        public DateTime? ReembolsadaEm { get; set; }

        public DateTime CriadaEm { get; set; } = DateTime.Now;

        public bool Pago => PagamentoEm != null;
        public bool Reembolsada => ReembolsoId != null;
        public bool TemPendencia => Status == StatusSolicitacao.Devolvida && !string.IsNullOrWhiteSpace(PendenciaMotivo);
        public bool Cancelada => Status == StatusSolicitacao.Cancelada;
        public string Iniciais => Usuario.IniciaisDe(Nome);

        public DocumentoEnviado? Documento(string tipo) => Documentos.FirstOrDefault(d => d.Tipo == tipo);

        /// <summary>Todos os documentos exigidos têm arquivo e nenhum está recusado.</summary>
        public bool DocumentosCompletos => DocumentosMatricula.Exigidos.All(e => Documento(e.Tipo) is { Enviado: true, Recusado: false });

        /// <summary>Todos os documentos exigidos foram aprovados pela coordenação.</summary>
        public bool DocumentosAprovados => DocumentosMatricula.Exigidos.All(e => Documento(e.Tipo) is { Aprovado: true });
    }

    /// <summary>Um documento do candidato: um ou mais arquivos (ex.: frente e verso), em foto ou PDF.</summary>
    public class DocumentoEnviado
    {
        /// <summary>Até quantos arquivos o candidato pode mandar por documento.</summary>
        public const int MaximoArquivos = 4;

        /// <summary>Chave do documento em DocumentosMatricula.Exigidos (ex.: "identidade").</summary>
        public string Tipo { get; set; } = string.Empty;

        public List<ArquivoDocumento> Arquivos { get; set; } = new();

        public bool Enviado => Arquivos.Count > 0;

        /// <summary>Conferido e aceito pela coordenação na análise.</summary>
        public bool Aprovado { get; set; }

        /// <summary>Marcado pela coordenação na análise; o candidato precisa enviar outra foto.</summary>
        public bool Recusado { get; set; }
        public string? Observacao { get; set; }
    }

    /// <summary>Arquivo de um documento, guardado no GridFS.</summary>
    public class ArquivoDocumento
    {
        public string ArquivoId { get; set; } = string.Empty;
        public string NomeArquivo { get; set; } = string.Empty;
        public DateTime EnviadoEm { get; set; } = DateTime.Now;

        public bool IsPdf => NomeArquivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Documentos exigidos para a matrícula no mestrado.</summary>
    public static class DocumentosMatricula
    {
        public sealed record Exigido(string Tipo, string Nome, string Dica);

        public static readonly IReadOnlyList<Exigido> Exigidos = new List<Exigido>
        {
            new("identidade", "Documento de identidade", "RG ou CNH: frente e verso"),
            new("cpf", "CPF", "Pode ser o próprio RG ou CNH, se tiver o número do CPF"),
            new("diploma", "Diploma de graduação", "Frente e verso"),
            new("historico", "Histórico escolar da graduação", "Todas as páginas"),
            new("foto", "Foto 3x4", "Recente, com fundo claro")
        };

        public static string NomeDe(string tipo) => Exigidos.FirstOrDefault(e => e.Tipo == tipo)?.Nome ?? tipo;
    }
}
