using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace European.Services
{
    /// <summary>
    /// Token de sessão JWT (HS256). É emitido no login com o id, o e-mail e o perfil da conta e vale por
    /// <see cref="Validade"/>. Toda navegação para uma tela protegida confere a assinatura, o emissor e a validade
    /// (AcessoService.PodeAbrir); vencido, o usuário volta para o login.
    /// A chave de assinatura é gerada aleatoriamente a cada execução do app e fica só na memória.
    /// </summary>
    public static class TokenService
    {
        public static readonly TimeSpan Validade = TimeSpan.FromHours(8);

        private const string Emissor = "european-app";
        private const string Publico = "european-app";
        public const string ClaimPerfil = "perfil";

        private static readonly SymmetricSecurityKey Chave = new(RandomNumberGenerator.GetBytes(32));
        private static readonly JwtSecurityTokenHandler Handler = new() { MapInboundClaims = false };

        public static string Emitir(string id, string email, string perfil)
        {
            var agora = DateTime.UtcNow;
            var descritor = new SecurityTokenDescriptor
            {
                Issuer = Emissor,
                Audience = Publico,
                IssuedAt = agora,
                NotBefore = agora,
                Expires = agora.Add(Validade),
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, id),
                    new Claim(JwtRegisteredClaimNames.Email, email),
                    new Claim(ClaimPerfil, perfil),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
                }),
                SigningCredentials = new SigningCredentials(Chave, SecurityAlgorithms.HmacSha256)
            };
            return Handler.WriteToken(Handler.CreateToken(descritor));
        }

        /// <summary>Confere assinatura, emissor, público e validade. Retorna as claims, ou null se o token não vale.</summary>
        public static ClaimsPrincipal? Validar(string? token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                return Handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = Chave,
                    ValidIssuer = Emissor,
                    ValidAudience = Publico,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);
            }
            catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
            {
                return null;
            }
        }
    }
}
