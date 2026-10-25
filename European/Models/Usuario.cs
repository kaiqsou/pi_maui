using European.Enums;

namespace European.Models
{
    public class Usuario
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public Roles Roles { get; set; }
        public StatusAcesso StatusAcesso { get; set; } = StatusAcesso.Pendente;
        public string? AvatarUrl { get; set; }
        public DateTime? DataNascimento { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool IsCoordenacao => Roles is Roles.Coordenador or Roles.Administrador;
        public bool AcessoLiberado => StatusAcesso == StatusAcesso.Liberado;

        public string Iniciais
        {
            get
            {
                var partes = Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return partes.Length switch
                {
                    0 => "?",
                    1 => partes[0][..1].ToUpper(),
                    _ => $"{partes[0][..1]}{partes[^1][..1]}".ToUpper()
                };
            }
        }
    }
}
