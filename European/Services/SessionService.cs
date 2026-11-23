using System.IdentityModel.Tokens.Jwt;
using European.Models;

namespace European.Services
{
    public static class SessionService
    {
        public const string PerfilCandidato = "Candidato";

        public static Usuario? UsuarioAtual { get; private set; }

        /// <summary>Candidato logado com uma solicitação de matrícula ainda não liberada (não é usuário).</summary>
        public static SolicitacaoMatricula? SolicitacaoAtual { get; private set; }

        /// <summary>Token JWT da sessão (TokenService), emitido no login.</summary>
        public static string? Token { get; private set; }

        public static bool EstaLogado => UsuarioAtual is not null || SolicitacaoAtual is not null;

        /// <summary>
        /// O token é válido (assinatura e prazo) e pertence à conta logada, com o mesmo perfil.
        /// Sem isso nenhuma tela protegida abre.
        /// </summary>
        public static bool SessaoValida
        {
            get
            {
                var claims = TokenService.Validar(Token);
                if (claims == null)
                    return false;

                var id = UsuarioAtual?.Id ?? SolicitacaoAtual?.Id;
                var perfil = UsuarioAtual?.Roles.ToString() ?? PerfilCandidato;
                return id != null
                       && claims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value == id
                       && claims.FindFirst(TokenService.ClaimPerfil)?.Value == perfil;
            }
        }

        /// <summary>Há alguém logado, mas o token venceu (ou não confere): é preciso entrar de novo.</summary>
        public static bool SessaoExpirada => EstaLogado && !SessaoValida;

        public static string NomeAtual => UsuarioAtual?.Nome ?? SolicitacaoAtual?.Nome ?? string.Empty;
        public static string EmailAtual => UsuarioAtual?.Email ?? SolicitacaoAtual?.Email ?? string.Empty;
        public static string IniciaisAtuais => UsuarioAtual?.Iniciais ?? SolicitacaoAtual?.Iniciais ?? "?";

        public static void Entrar(Usuario usuario)
        {
            UsuarioAtual = usuario;
            SolicitacaoAtual = null;
            Token = TokenService.Emitir(usuario.Id, usuario.Email, usuario.Roles.ToString());
        }

        public static void EntrarCandidato(SolicitacaoMatricula solicitacao)
        {
            UsuarioAtual = null;
            SolicitacaoAtual = solicitacao;
            Token = TokenService.Emitir(solicitacao.Id, solicitacao.Email, PerfilCandidato);
        }

        public static void Sair()
        {
            UsuarioAtual = null;
            SolicitacaoAtual = null;
            Token = null;
        }
    }
}
