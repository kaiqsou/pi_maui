using European.Enums;

namespace European.Models
{
    /// <summary>
    /// Pedido de matrícula feito pelo app. Enquanto a coordenação não libera, o candidato NÃO é um
    /// usuário: os dados ficam só aqui. Ao liberar, vira Usuario + Matricula e a solicitação é apagada.
    /// </summary>
    public class SolicitacaoMatricula
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public string CursoId { get; set; } = string.Empty;

        /// <summary>Pagamento (simulado). Dados de cartão nunca são guardados.</summary>
        public FormaPagamento? FormaPagamento { get; set; }
        public DateTime? PagamentoEm { get; set; }

        /// <summary>Motivo informado pela coordenação ao não liberar; some ao refazer o pagamento.</summary>
        public string? PendenciaMotivo { get; set; }
        public DateTime? PendenciaEm { get; set; }

        public DateTime CriadaEm { get; set; } = DateTime.Now;

        public bool Pago => PagamentoEm != null;
        public bool TemPendencia => !string.IsNullOrWhiteSpace(PendenciaMotivo);
        public string Iniciais => Usuario.IniciaisDe(Nome);
    }
}
