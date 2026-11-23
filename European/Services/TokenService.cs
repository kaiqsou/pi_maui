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
    /// A chave de assinatura é gerada uma vez no aparelho e guardada no SecureStorage (armazenamento criptografado
    /// do sistema), para o token salvo continuar valendo quando o app é aberto de novo.
    /// </summary>
    public static class TokenService
    {
        /// <summary>Quanto tempo a sessão dura sem abrir o app (a cada abertura o token é renovado).</summary>
        public static readonly TimeSpan Validade = TimeSpan.FromDays(30);

        private const string ChaveArmazenada = "european.sessao.chave";

        private const string Emissor = "european-app";
        private const string Publico = "european-app";
        public const string ClaimPerfil = "perfil";

        // até CarregarChaveAsync rodar (ou se o SecureStorage falhar), vale uma chave só desta execução
        private static SymmetricSecurityKey Chave = new(RandomNumberGenerator.GetBytes(32));
        private static bool _chaveCarregada;

        /// <summary>Lê a chave de assinatura do SecureStorage (ou cria e guarda, na primeira vez). Chamado ao abrir o app.</summary>
        public static async Task CarregarChaveAsync()
        {
            if (_chaveCarregada)
                return;

            try
            {
                var salva = await SecureStorage.Default.GetAsync(ChaveArmazenada);
                if (string.IsNullOrEmpty(salva))
                {
                    var nova = RandomNumberGenerator.GetBytes(32);
                    await SecureStorage.Default.SetAsync(ChaveArmazenada, Convert.ToBase64String(nova));
                    Chave = new SymmetricSecurityKey(nova);
                }
                else
                {
                    Chave = new SymmetricSecurityKey(Convert.FromBase64String(salva));
                }
            }
            catch (Exception ex)
            {
                // sem SecureStorage no aparelho: a sessão só não é lembrada depois de fechar o app
                CrashLogger.Registrar(nameof(TokenService), ex);
            }

            _chaveCarregada = true;
        }
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
