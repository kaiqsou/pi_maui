using European.Models;

namespace European.Services
{
    public static class SessionService
    {
        public static Usuario? UsuarioAtual { get; private set; }

        /// <summary>Candidato logado com uma solicitação de matrícula ainda não liberada (não é usuário).</summary>
        public static SolicitacaoMatricula? SolicitacaoAtual { get; private set; }

        public static event Action? UsuarioAlterado;

        public static bool EstaLogado => UsuarioAtual is not null || SolicitacaoAtual is not null;

        public static string NomeAtual => UsuarioAtual?.Nome ?? SolicitacaoAtual?.Nome ?? string.Empty;
        public static string EmailAtual => UsuarioAtual?.Email ?? SolicitacaoAtual?.Email ?? string.Empty;
        public static string IniciaisAtuais => UsuarioAtual?.Iniciais ?? SolicitacaoAtual?.Iniciais ?? "?";

        public static void Entrar(Usuario usuario)
        {
            UsuarioAtual = usuario;
            SolicitacaoAtual = null;
            UsuarioAlterado?.Invoke();
        }

        public static void EntrarCandidato(SolicitacaoMatricula solicitacao)
        {
            UsuarioAtual = null;
            SolicitacaoAtual = solicitacao;
            UsuarioAlterado?.Invoke();
        }

        public static void Sair()
        {
            UsuarioAtual = null;
            SolicitacaoAtual = null;
            UsuarioAlterado?.Invoke();
        }
    }
}
