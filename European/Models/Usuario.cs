using European.Enums;

namespace European.Models
{
    public class Usuario
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;

        /// <summary>Senha definida pela coordenação; no próximo login o usuário precisa criar a própria.</summary>
        public bool SenhaProvisoria { get; set; }
        public Roles Roles { get; set; }
        public StatusAcesso StatusAcesso { get; set; } = StatusAcesso.Pendente;
        public string? AvatarUrl { get; set; }
        public DateTime? DataNascimento { get; set; }

        /// <summary>Pagamento (simulado) feito na solicitação de matrícula, copiado ao liberar o acesso.</summary>
        public FormaPagamento? FormaPagamento { get; set; }
        public DateTime? PagamentoEm { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool IsCoordenacao => Roles is Roles.Coordenador or Roles.Administrador;
        public bool AcessoLiberado => StatusAcesso == StatusAcesso.Liberado;

        public string Iniciais => IniciaisDe(Nome);

        public static string IniciaisDe(string nome)
        {
            var partes = (nome ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return partes.Length switch
            {
                0 => "?",
                1 => partes[0][..1].ToUpper(),
                _ => $"{partes[0][..1]}{partes[^1][..1]}".ToUpper()
            };
        }
    }
}
