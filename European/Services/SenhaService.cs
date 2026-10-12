using System.Security.Cryptography;
using System.Text;

namespace European.Services
{
    public static class SenhaService
    {
        public const int TamanhoMinimo = 6;

        private const int Iteracoes = 100_000;
        private const int TamanhoSalt = 16;
        private const int TamanhoChave = 32;

        public static string GerarHash(string senha)
        {
            var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
            var chave = Rfc2898DeriveBytes.Pbkdf2(
                senha ?? string.Empty, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoChave);

            return $"{Iteracoes}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(chave)}";
        }

        public static bool Verificar(string senha, string armazenado)
        {
            if (string.IsNullOrEmpty(armazenado))
                return false;

            // Formato atual: iteracoes.saltBase64.chaveBase64 (salt aleatório por senha).
            var partes = armazenado.Split('.');
            if (partes.Length == 3 && int.TryParse(partes[0], out var iteracoes))
            {
                try
                {
                    var salt = Convert.FromBase64String(partes[1]);
                    var esperado = Convert.FromBase64String(partes[2]);
                    var chave = Rfc2898DeriveBytes.Pbkdf2(
                        senha ?? string.Empty, salt, iteracoes, HashAlgorithmName.SHA256, esperado.Length);

                    return CryptographicOperations.FixedTimeEquals(chave, esperado);
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            // Compatibilidade: hash SHA-256 antigo (sem salt).
            if (EhHashLegado(armazenado))
                return string.Equals(GerarHashLegado(senha), armazenado, StringComparison.OrdinalIgnoreCase);

            // Compatibilidade: valores antigos salvos em texto puro.
            return string.Equals(senha, armazenado, StringComparison.Ordinal);
        }

        private static bool EhHashLegado(string valor)
            => valor.Length == 64 && valor.All(c => "0123456789abcdefABCDEF".Contains(c));

        private static string GerarHashLegado(string senha)
        {
            var bytes = Encoding.UTF8.GetBytes(senha ?? string.Empty);
            return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        }

        public static bool EhValida(string senha, out string mensagem)
        {
            if (string.IsNullOrWhiteSpace(senha) || senha.Length < TamanhoMinimo)
            {
                mensagem = $"A senha deve ter pelo menos {TamanhoMinimo} caracteres.";
                return false;
            }

            if (senha.Any(char.IsWhiteSpace))
            {
                mensagem = "A senha não pode conter espaços.";
                return false;
            }

            if (!senha.Any(char.IsLetter) || !senha.Any(char.IsDigit))
            {
                mensagem = "A senha deve conter letras e números.";
                return false;
            }

            mensagem = string.Empty;
            return true;
        }
    }
}
