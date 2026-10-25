using System.Collections.ObjectModel;
using System.Windows.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class AulaEditViewModel : BaseViewModel
    {
        private const long TamanhoMaximoArquivo = 50L * 1024 * 1024;

        private Materia? _materia;
        private Aula? _aula;
        private string _titulo = string.Empty;
        private string _descricao = string.Empty;
        private string _videoUrl = string.Empty;
        private string _duracaoMinutos = string.Empty;

        public AulaEditViewModel()
        {
            SalvarCommand = new Command(async () => await SalvarAsync());
            AdicionarLinkCommand = new Command(async () => await AdicionarLinkAsync());
            AdicionarArquivoCommand = new Command(async () => await AdicionarArquivoAsync());
            RemoverMaterialCommand = new Command<MaterialComplementar>(async m => await RemoverMaterialAsync(m));
        }

        public string Titulo { get => _titulo; set => SetProperty(ref _titulo, value); }
        public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
        public string DuracaoMinutos { get => _duracaoMinutos; set => SetProperty(ref _duracaoMinutos, value); }

        public string VideoUrl
        {
            get => _videoUrl;
            set
            {
                if (SetProperty(ref _videoUrl, value))
                    OnPropertyChanged(nameof(VideoInvalido));
            }
        }

        public bool VideoInvalido => !string.IsNullOrWhiteSpace(VideoUrl) && YouTubePlayer.ExtrairId(VideoUrl) == null;

        public string MateriaNome => _materia?.Nome ?? string.Empty;
        public bool IsEdicao => _aula != null;

        public ObservableCollection<MaterialComplementar> Materiais { get; } = new();

        public ICommand SalvarCommand { get; }
        public ICommand AdicionarLinkCommand { get; }
        public ICommand AdicionarArquivoCommand { get; }
        public ICommand RemoverMaterialCommand { get; }

        private string? CursoId => _materia?.Modulo?.Curso?.Id;

        public async Task CarregarAsync(Materia? materia, Aula? aula)
        {
            _materia = materia ?? aula?.Materia;
            _aula = aula;
            Title = aula == null ? "Nova aula" : "Editar aula";
            OnPropertyChanged(nameof(MateriaNome));
            OnPropertyChanged(nameof(IsEdicao));

            Titulo = aula?.Titulo ?? string.Empty;
            Descricao = aula?.Descricao ?? string.Empty;
            VideoUrl = aula?.VideoId ?? string.Empty;
            DuracaoMinutos = aula == null || aula.DuracaoSegundos == 0 ? string.Empty : Math.Round(aula.DuracaoSegundos / 60.0).ToString();

            await CarregarMateriaisAsync();
        }

        private async Task CarregarMateriaisAsync()
        {
            Materiais.Clear();
            if (_aula == null)
                return;

            await ExecutarAsync(async () =>
            {
                foreach (var material in await MaterialRepositorio.DaAulaAsync(_aula.Id))
                    Materiais.Add(material);
            });
        }

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

            var novaAula = _aula == null;
            var segundos = int.TryParse(DuracaoMinutos, out var minutos) ? Math.Max(0, minutos) * 60 : 0;
            Aula? salva = null;

            var ok = await ExecutarAsync(() => CursoRepositorio.AlterarAsync(cursoId, curso =>
            {
                var materia = curso.TodasMaterias.FirstOrDefault(m => m.Id == _materia.Id)
                    ?? throw new InvalidOperationException("A matéria foi removida.");

                var aula = _aula == null ? null : materia.Aulas.FirstOrDefault(a => a.Id == _aula.Id);
                if (aula == null)
                {
                    aula = new Aula { Id = MongoContext.NovoId(), Ordem = materia.Aulas.Count + 1 };
                    materia.Aulas.Add(aula);
                }

                aula.Titulo = Titulo.Trim();
                aula.Descricao = Descricao.Trim();
                aula.VideoId = VideoUrl.Trim();
                if (segundos > 0 || aula.DuracaoSegundos == 0)
                    aula.DuracaoSegundos = segundos;

                salva = aula;
            }));

            if (!ok)
                return;

            if (novaAula && salva != null)
            {
                _aula = salva;
                Title = "Editar aula";
                OnPropertyChanged(nameof(IsEdicao));

                if (await ConfirmarAsync("Aula criada. Deseja anexar materiais (PDF, slides, links) agora?", "Anexar"))
                    return;
            }

            await VoltarAsync();
        }

        private async Task AdicionarLinkAsync()
        {
            if (_aula == null || CursoId == null)
                return;

            var titulo = await Shell.Current.DisplayPromptAsync("Novo link", "Título do material", "Continuar", "Cancelar", "Ex.: Artigo de apoio");
            if (string.IsNullOrWhiteSpace(titulo))
                return;

            var url = await Shell.Current.DisplayPromptAsync("Novo link", "Endereço (URL)", "Adicionar", "Cancelar", "https://", keyboard: Keyboard.Url);
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
                AulaId = _aula.Id,
                Titulo = titulo.Trim(),
                Url = uri.ToString(),
                Tipo = TipoMaterial.Link
            }));
            await CarregarMateriaisAsync();
        }

        private async Task AdicionarArquivoAsync()
        {
            if (_aula == null || CursoId == null)
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

                var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
                await MaterialRepositorio.InserirAsync(new MaterialComplementar
                {
                    CursoId = CursoId,
                    AulaId = _aula.Id,
                    Titulo = Path.GetFileNameWithoutExtension(arquivo.FileName),
                    NomeArquivo = arquivo.FileName,
                    TamanhoBytes = stream.CanSeek ? stream.Length : 0,
                    Tipo = extensao switch
                    {
                        ".pdf" => TipoMaterial.Pdf,
                        ".ppt" or ".pptx" or ".key" or ".odp" => TipoMaterial.Slide,
                        ".mp4" or ".mov" or ".avi" or ".mkv" => TipoMaterial.Video,
                        _ => TipoMaterial.Outro
                    }
                }, stream);
            });
            await CarregarMateriaisAsync();
        }

        private async Task RemoverMaterialAsync(MaterialComplementar? material)
        {
            if (material == null || !await ConfirmarAsync($"Remover o material \"{material.Titulo}\"?", "Remover"))
                return;

            await ExecutarAsync(() => MaterialRepositorio.ExcluirAsync(material));
            await CarregarMateriaisAsync();
        }
    }
}
