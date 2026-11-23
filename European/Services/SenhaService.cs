using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace European.Services
{
    /// <summary>
    /// Hash e regras de senha. As senhas são guardadas com Argon2id (salt aleatório), no formato PHC:
    /// $argon2id$v=19$m=19456,t=2,p=1$salt$hash — os parâmetros mínimos recomendados pela OWASP.
    /// </summary>
    public static class SenhaService
    {
        public const int TamanhoMinimo = 6;

        private const int MemoriaKiB = 19_456;
        private const int Iteracoes = 2;
        private const int Paralelismo = 1;
        private const int TamanhoSalt = 16;
        private const int TamanhoChave = 32;
        private const string Prefixo = "$argon2id$v=19$";

        public static string GerarHash(string senha)
        {
            var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
            var chave = Argon2id(senha, salt, MemoriaKiB, Iteracoes, Paralelismo, TamanhoChave);
            return $"{Prefixo}m={MemoriaKiB},t={Iteracoes},p={Paralelismo}${B64(salt)}${B64(chave)}";
        }

        /// <summary>
        /// Confere a senha com o hash guardado. Aceita o formato atual (Argon2id) e os de versões antigas
        /// (PBKDF2-SHA256 e SHA-256 sem salt), que são trocados por Argon2id no login (<see cref="PrecisaAtualizar"/>).
        /// Senha em texto puro nunca é aceita.
        /// </summary>
        public static bool Verificar(string senha, string armazenado)
        {
            if (EhArgon2id(armazenado, out var m, out var t, out var p, out var salt, out var esperado))
            {
                var chave = Argon2id(senha, salt, m, t, p, esperado.Length);
                return CryptographicOperations.FixedTimeEquals(chave, esperado);
            }

            if (EhPbkdf2(armazenado, out var iteracoes, out salt, out esperado))
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

        /// <summary>Hash em formato antigo (PBKDF2 ou SHA-256): gere um novo com <see cref="GerarHash"/> após o login.</summary>
        public static bool PrecisaAtualizar(string armazenado) => !EhArgon2id(armazenado, out _, out _, out _, out _, out _);

        /// <summary>Valor que não é hash nenhum (senha gravada em texto por versões antigas do app).</summary>
        public static bool EhTextoPuro(string? armazenado)
            => !string.IsNullOrEmpty(armazenado)
               && !EhArgon2id(armazenado, out _, out _, out _, out _, out _)
               && !EhPbkdf2(armazenado, out _, out _, out _)
               && !EhSha256Antigo(armazenado);

        public static bool EhValida(string senha, out string mensagem)
        {
            mensagem = string.IsNullOrWhiteSpace(senha) || senha.Length < TamanhoMinimo ? $"A senha deve ter pelo menos {TamanhoMinimo} caracteres."
                : senha.Any(char.IsWhiteSpace) ? "A senha não pode conter espaços."
                : !senha.Any(char.IsLetter) || !senha.Any(char.IsDigit) ? "A senha deve conter letras e números."
                : string.Empty;
            return mensagem.Length == 0;
        }

        private static byte[] Argon2id(string? senha, byte[] salt, int memoriaKiB, int iteracoes, int paralelismo, int tamanho)
        {
            using var argon = new Argon2id(Encoding.UTF8.GetBytes(senha ?? string.Empty))
            {
                Salt = salt,
                MemorySize = memoriaKiB,
                Iterations = iteracoes,
                DegreeOfParallelism = paralelismo
            };
            return argon.GetBytes(tamanho);
        }

        private static bool EhArgon2id(string? armazenado, out int memoria, out int iteracoes, out int paralelismo, out byte[] salt, out byte[] chave)
        {
            memoria = iteracoes = paralelismo = 0;
            salt = chave = Array.Empty<byte>();
            if (armazenado == null || !armazenado.StartsWith(Prefixo, StringComparison.Ordinal))
                return false;

            // [parâmetros, salt, hash]
            var partes = armazenado[Prefixo.Length..].Split('$');
            if (partes.Length != 3)
                return false;

            foreach (var par in partes[0].Split(','))
            {
                var kv = par.Split('=');
                if (kv.Length != 2 || !int.TryParse(kv[1], out var valor) || valor <= 0)
                    return false;
                switch (kv[0])
                {
                    case "m": memoria = valor; break;
                    case "t": iteracoes = valor; break;
                    case "p": paralelismo = valor; break;
                    default: return false;
                }
            }

            try
            {
                salt = DeB64(partes[1]);
                chave = DeB64(partes[2]);
            }
            catch (FormatException)
            {
                return false;
            }
            return memoria > 0 && iteracoes > 0 && paralelismo > 0 && salt.Length > 0 && chave.Length > 0;
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

        // O formato PHC usa Base64 sem o preenchimento "=".
        private static string B64(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=');

        private static byte[] DeB64(string texto)
            => Convert.FromBase64String(texto.PadRight(texto.Length + (4 - texto.Length % 4) % 4, '='));
    }
}
