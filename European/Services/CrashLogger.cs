using System.Text;

namespace European.Services
{
    public static class CrashLogger
    {
        private static readonly object Bloqueio = new();

        public static string CaminhoArquivo { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "european-crash.log");

        public static void Registrar(string origem, Exception? excecao)
        {
            try
            {
                var texto = new StringBuilder()
                    .AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {origem}")
                    .AppendLine(excecao?.ToString() ?? "(sem exceção)")
                    .AppendLine(new string('-', 70))
                    .ToString();

                lock (Bloqueio)
                {
                    File.AppendAllText(CaminhoArquivo, texto);
                }
            }
            catch
            {
                // nunca deixar o log derrubar o app
            }
        }
    }
}