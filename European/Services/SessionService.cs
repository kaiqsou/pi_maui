using European.Models;

namespace European.Services
{
    public static class SessionService
    {
        public static Usuario? UsuarioAtual { get; private set; }

        public static event Action? UsuarioAlterado;

        public static bool EstaLogado => UsuarioAtual is not null;

        public static void Entrar(Usuario usuario)
        {
            UsuarioAtual = usuario;
            UsuarioAlterado?.Invoke();
        }

        public static void Sair()
        {
            UsuarioAtual = null;
            UsuarioAlterado?.Invoke();
        }
    }
}
