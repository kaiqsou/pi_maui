using European.Models;

namespace European.Services
{
    public class DadosCadastro
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    public static class SessionService
    {
        public static Usuario? UsuarioAtual { get; private set; }

        public static DadosCadastro? CadastroPendente { get; private set; }

        public static event Action? UsuarioAlterado;

        public static bool EstaLogado => UsuarioAtual is not null;

        public static void DefinirCadastroPendente(DadosCadastro dados) => CadastroPendente = dados;

        public static void LimparCadastroPendente() => CadastroPendente = null;

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
