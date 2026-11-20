using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public partial class AulaEditViewModel : BaseViewModel
    {
        private const long TamanhoMaximoArquivo = 50L * 1024 * 1024;

        private Materia? _materia;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEdicao))]
        private partial Aula? AulaAtual { get; set; }

        [ObservableProperty]
        public partial string Titulo { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Descricao { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string DuracaoMinutos { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VideoInvalido))]
        public partial string VideoUrl { get; set; } = string.Empty;

        public bool VideoInvalido => !string.IsNullOrWhiteSpace(VideoUrl) && YouTubePlayer.ExtrairId(VideoUrl) == null;
        public string MateriaNome => _materia?.Nome ?? string.Empty;

        /// <summary>Materiais só podem ser anexados depois que a aula existe.</summary>
        public bool IsEdicao => AulaAtual != null;

        public ObservableCollection<MaterialComplementar> Materiais { get; } = new();

        private string? CursoId => _materia?.Modulo?.Curso?.Id;

        protected override async Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            AulaAtual = parametros.Obter<Aula>("Aula");
            _materia = parametros.Obter<Materia>("Materia") ?? AulaAtual?.Materia;
            Title = AulaAtual == null ? "Nova aula" : "Editar aula";
            OnPropertyChanged(nameof(MateriaNome));

            Titulo = AulaAtual?.Titulo ?? string.Empty;
            Descricao = AulaAtual?.Descricao ?? string.Empty;
            VideoUrl = AulaAtual?.VideoId ?? string.Empty;
            DuracaoMinutos = AulaAtual == null || AulaAtual.DuracaoSegundos == 0 ? string.Empty : Math.Round(AulaAtual.DuracaoSegundos / 60.0).ToString();

            await CarregarMateriaisAsync();
        }

        private async Task CarregarMateriaisAsync()
        {
            Materiais.Clear();
            if (AulaAtual == null)
                return;

            await ExecutarAsync(async () =>
            {
                foreach (var material in await MaterialRepositorio.DaAulaAsync(AulaAtual.Id))
                    Materiais.Add(material);
            });
        }

        [RelayCommand]
        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Titulo))
            {
                await AvisarAsync("Aula", "Informe o título.");
                return;
            }

            if (string.IsNullOrWhiteSpace(VideoUrl) || VideoInvalido)
            {
                await AvisarAsync("Aula", "Informe um link válido do YouTube (watch, youtu.be, shorts ou embed).");
                return;
            }

            var cursoId = CursoId;
            if (_materia == null || cursoId == null)
                return;

            var novaAula = AulaAtual == null;
            var segundos = int.TryParse(DuracaoMinutos, out var minutos) ? Math.Max(0, minutos) * 60 : 0;
            Aula? salva = null;

            var ok = await ExecutarAsync(async () => salva = await CursoRepositorio.SalvarAulaAsync(cursoId, _materia.Id, AulaAtual?.Id, aula =>
            {
                aula.Titulo = Titulo.Trim();
                aula.Descricao = Descricao.Trim();
                aula.VideoId = VideoUrl.Trim();
                if (segundos > 0 || aula.DuracaoSegundos == 0)
                    aula.DuracaoSegundos = segundos;
            }));

            if (!ok)
                return;

            if (novaAula && salva != null)
            {
                AulaAtual = salva;
                Title = "Editar aula";

                if (await ConfirmarAsync("Aula criada. Deseja anexar materiais (PDF, slides, links) agora?", "Anexar"))
                    return;
            }

            await VoltarAsync();
        }

        [RelayCommand]
        private async Task AdicionarLinkAsync()
        {
            if (AulaAtual == null || CursoId == null)
                return;

            var titulo = await PerguntarAsync("Novo link", "Título do material", "Continuar", "Ex.: Artigo de apoio");
            if (string.IsNullOrWhiteSpace(titulo))
                return;

            var url = await PerguntarAsync("Novo link", "Endereço (URL)", "Adicionar", "https://", teclado: Keyboard.Url);
            if (string.IsNullOrWhiteSpace(url))
                return;

            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                await AvisarAsync("Link", "Informe um endereço começando com http:// ou https://.");
                return;
            }

            await ExecutarAsync(() => MaterialRepositorio.InserirAsync(new MaterialComplementar
            {
                CursoId = CursoId,
                AulaId = AulaAtual.Id,
                Titulo = titulo.Trim(),
                Url = uri.ToString(),
                Tipo = TipoMaterial.Link
            }));
            await CarregarMateriaisAsync();
        }

        [RelayCommand]
        private async Task AdicionarArquivoAsync()
        {
            if (AulaAtual == null || CursoId == null)
                return;

            FileResult? arquivo;
            try
            {
                arquivo = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Selecione o material da aula" });
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(AdicionarArquivoAsync), ex);
                return;
            }

            if (arquivo == null)
                return;

            await ExecutarAsync(async () =>
            {
                await using var stream = await arquivo.OpenReadAsync();
                if (stream.CanSeek && stream.Length > TamanhoMaximoArquivo)
                {
                    await AvisarAsync("Arquivo", "O arquivo deve ter no máximo 50 MB.");
                    return;
                }

                await MaterialRepositorio.InserirAsync(new MaterialComplementar
                {
                    CursoId = CursoId,
                    AulaId = AulaAtual.Id,
                    Titulo = Path.GetFileNameWithoutExtension(arquivo.FileName),
                    NomeArquivo = arquivo.FileName,
                    TamanhoBytes = stream.CanSeek ? stream.Length : 0,
                    Tipo = TipoPorExtensao(arquivo.FileName)
                }, stream);
            });
            await CarregarMateriaisAsync();
        }

        private static TipoMaterial TipoPorExtensao(string nomeArquivo) => Path.GetExtension(nomeArquivo).ToLowerInvariant() switch
        {
            ".pdf" => TipoMaterial.Pdf,
            ".ppt" or ".pptx" or ".key" or ".odp" => TipoMaterial.Slide,
            ".mp4" or ".mov" or ".avi" or ".mkv" => TipoMaterial.Video,
            _ => TipoMaterial.Outro
        };

        [RelayCommand]
        private async Task RemoverMaterialAsync(MaterialComplementar? material)
        {
            if (material == null || !await ConfirmarAsync($"Remover o material \"{material.Titulo}\"?", "Remover"))
                return;

            await ExecutarAsync(() => MaterialRepositorio.ExcluirAsync(material));
            await CarregarMateriaisAsync();
        }
    }
}
