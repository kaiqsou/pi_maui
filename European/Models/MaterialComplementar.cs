using European.Enums;

namespace European.Models
{
    public class MaterialComplementar : IEntidade
    {
        public string Id { get; set; } = string.Empty;
        public string CursoId { get; set; } = string.Empty;
        public string AulaId { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        /// <summary>Arquivo enviado pelo professor e guardado no GridFS (quando não é um link).</summary>
        public string? ArquivoId { get; set; }
        public string? NomeArquivo { get; set; }
        public long TamanhoBytes { get; set; }

        public TipoMaterial Tipo { get; set; }
        public int Ordem { get; set; }

        public bool IsArquivo => !string.IsNullOrEmpty(ArquivoId);
        public string Detalhe => IsArquivo
            ? $"{NomeArquivo} · {Math.Max(1, TamanhoBytes / 1024):N0} KB"
            : Url;
    }
}
