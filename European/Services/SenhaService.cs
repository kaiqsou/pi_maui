using System.Security.Cryptography;
using System.Text;

namespace European.Services
{
    /// <summary>Hash e regras de senha. As senhas são guardadas com PBKDF2-SHA256 e salt aleatório.</summary>
    public static class SenhaService
    {
        public const int TamanhoMinimo = 6;

        private const int Iteracoes = 100_000;
        private const int TamanhoSalt = 16;
        private const int TamanhoChave = 32;

        /// <summary>Formato: iteracoes.saltBase64.chaveBase64.</summary>
        public static string GerarHash(string senha)
        {
            var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
            var chave = Rfc2898DeriveBytes.Pbkdf2(senha ?? string.Empty, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoChave);
            return $"{Iteracoes}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(chave)}";
        }

        /// <summary>
        /// Confere a senha com o hash guardado. Aceita o formato atual (PBKDF2) e o SHA-256 sem salt de versões
        /// antigas, que deve ser trocado no login (<see cref="PrecisaAtualizar"/>). Senha em texto puro nunca é aceita.
        /// </summary>
        public static bool Verificar(string senha, string armazenado)
        {
            if (EhPbkdf2(armazenado, out var iteracoes, out var salt, out var esperado))
            {
                var chave = Rfc2898DeriveBytes.Pbkdf2(senha ?? string.Empty, salt, iteracoes, HashAlgorithmName.SHA256, esperado.Length);
                return CryptographicOperations.FixedTimeEquals(chave, esperado);
            }

            if (EhSha256Antigo(armazenado))
            {
                var calculado = SHA256.HashData(Encoding.UTF8.GetBytes(senha ?? string.Empty));
                return CryptographicOperations.FixedTimeEquals(calculado, Convert.FromHexString(armazenado));
            }

            return false;
        }

        /// <summary>Hash em formato antigo (SHA-256 sem salt): gere um novo com <see cref="GerarHash"/> após o login.</summary>
        public static bool PrecisaAtualizar(string armazenado) => !EhPbkdf2(armazenado, out _, out _, out _);

        /// <summary>Valor que não é hash nenhum (senha gravada em texto por versões antigas do app).</summary>
        public static bool EhTextoPuro(string? armazenado)
            => !string.IsNullOrEmpty(armazenado) && !EhPbkdf2(armazenado, out _, out _, out _) && !EhSha256Antigo(armazenado);

        public static bool EhValida(string senha, out string mensagem)
        {
            mensagem = string.IsNullOrWhiteSpace(senha) || senha.Length < TamanhoMinimo ? $"A senha deve ter pelo menos {TamanhoMinimo} caracteres."
                : senha.Any(char.IsWhiteSpace) ? "A senha não pode conter espaços."
                : !senha.Any(char.IsLetter) || !senha.Any(char.IsDigit) ? "A senha deve conter letras e números."
                : string.Empty;
            return mensagem.Length == 0;
        }

        private static bool EhPbkdf2(string? armazenado, out int iteracoes, out byte[] salt, out byte[] chave)
        {
            iteracoes = 0;
            salt = chave = Array.Empty<byte>();
            var partes = (armazenado ?? string.Empty).Split('.');
            if (partes.Length != 3 || !int.TryParse(partes[0], out iteracoes) || iteracoes <= 0)
                return false;

            try
            {
                salt = Convert.FromBase64String(partes[1]);
                chave = Convert.FromBase64String(partes[2]);
                return salt.Length > 0 && chave.Length > 0;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool EhSha256Antigo(string? valor) => valor is { Length: 64 } && valor.All(Uri.IsHexDigit);
    }
}
