using CommunityToolkit.Mvvm.ComponentModel;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Arquivo de um documento na tela (miniatura da foto, ou o ícone de PDF).</summary>
    public partial class ArquivoItem : ObservableObject
    {
        public ArquivoDocumento Arquivo { get; init; } = null!;

        /// <summary>Documento a que o arquivo pertence (para remover).</summary>
        public string Tipo { get; init; } = string.Empty;

        public bool IsPdf => Arquivo.IsPdf;
        public bool IsImagem => !IsPdf;
        public string Nome => Arquivo.NomeArquivo;

        /// <summary>Cópia local (cache), usada na miniatura e para abrir/ampliar.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemMiniatura))]
        public partial string? Caminho { get; set; }

        public bool TemMiniatura => IsImagem && !string.IsNullOrEmpty(Caminho);

        /// <summary>Baixa o arquivo para o cache (as fotos aparecem na miniatura).</summary>
        public async Task CarregarAsync()
        {
            try
            {
                Caminho = await SolicitacaoRepositorio.BaixarArquivoAsync(Arquivo);
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(ArquivoItem), ex);
            }
        }

        public static async Task<List<ArquivoItem>> DoDocumentoAsync(DocumentoEnviado? documento)
        {
            var itens = (documento?.Arquivos ?? new List<ArquivoDocumento>())
                .Select(a => new ArquivoItem { Arquivo = a, Tipo = documento!.Tipo })
                .ToList();
            foreach (var item in itens.Where(i => i.IsImagem))
                await item.CarregarAsync();
            return itens;
        }
    }
}
