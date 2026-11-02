using System.Collections.ObjectModel;
using System.Windows.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class AulaViewModel : BaseViewModel
    {
        private Aula? _aula;
        private Matricula? _matricula;
        private List<ProgressoAula> _progressos = new();
        private double _percentualAssistido;
        private bool _concluida;
        private string _abaSelecionada = "aulas";
        private double _duracaoSegundos;
        private double _segundosParaRetomar;
        private bool _salvandoPosicao;

        public AulaViewModel()
        {
            ConcluirCommand = new Command(async () => await ConcluirAsync());
            SelecionarAbaCommand = new Command<string>(SelecionarAba);
            AbrirAulaCommand = new Command<AulaItem>(async item => await AbrirAulaAsync(item));
            AbrirMaterialCommand = new Command<MaterialComplementar>(async m => await AbrirMaterialAsync(m));
            VoltarCommand = new Command(async () => await VoltarAsync());
        }

        public Aula? Aula
        {
            get => _aula;
            set
            {
                if (SetProperty(ref _aula, value))
                {
                    OnPropertyChanged(nameof(DuracaoFormatada));
                    OnPropertyChanged(nameof(MateriaNome));
                    // VideoSource é notificado em AtualizarProgresso, depois de saber o minuto em que o aluno parou
                }
            }
        }

        public Matricula? Matricula
        {
            get => _matricula;
            set => SetProperty(ref _matricula, value);
        }

        public ObservableCollection<MaterialComplementar> Materiais { get; } = new();
        public ObservableCollection<AulaItem> AulasDoModulo { get; } = new();

        public string DuracaoFormatada
        {
            get
            {
                if (Aula == null)
                    return "00:00";

                var total = _duracaoSegundos > 0 ? _duracaoSegundos : Aula.DuracaoSegundos;
                return TimeSpan.FromSeconds(total).ToString(total >= 3600 ? @"h\:mm\:ss" : @"mm\:ss");
            }
        }

        public string MateriaNome => Aula?.Materia?.Nome ?? string.Empty;

        public WebViewSource VideoSource
        {
            get
            {
                if (Aula == null)
                    return new UrlWebViewSource { Url = "about:blank" };

                return CriarFonteDoVideo(Aula);
            }
        }

        public double SegundosParaRetomar => _segundosParaRetomar;

        public bool PossuiMateriais => Materiais.Count > 0;

        public void RegistrarDuracao(double segundos)
        {
            if (segundos <= 0 || Math.Abs(segundos - _duracaoSegundos) < 1)
                return;

            _duracaoSegundos = segundos;
            OnPropertyChanged(nameof(DuracaoFormatada));

            if (Aula == null || Math.Abs(Aula.DuracaoSegundos - segundos) < 1)
                return;

            Aula.DuracaoSegundos = (int)Math.Round(segundos);
            var cursoId = Aula.Materia?.Modulo?.Curso?.Id;
            if (cursoId != null)
                _ = SalvarEmSegundoPlanoAsync(() => CursoRepositorio.AtualizarDuracaoAulaAsync(cursoId, Aula.Id, Aula.DuracaoSegundos));
        }

        public void RegistrarPosicao(double segundos)
        {
            if (Aula == null || Matricula == null || segundos < 0 || _salvandoPosicao)
                return;

            var total = _duracaoSegundos > 0 ? _duracaoSegundos : Aula.DuracaoSegundos;
            var progresso = ProgressoLocal(Aula.Id);
            progresso.UltimaPosicaoSegundos = segundos;

            if (total > 0)
            {
                progresso.PercentualAssistido = Math.Max(progresso.PercentualAssistido, Math.Min(100, segundos / total * 100));
                PercentualAssistido = progresso.Concluida ? 100 : progresso.PercentualAssistido;
            }

            _salvandoPosicao = true;
            _ = SalvarEmSegundoPlanoAsync(async () =>
            {
                try { await ProgressoRepositorio.SalvarAsync(progresso); }
                finally { _salvandoPosicao = false; }
            });
        }

        public void RecarregarVideo()
        {
            _segundosParaRetomar = Aula == null ? 0 : ProgressoLocal(Aula.Id).UltimaPosicaoSegundos;
            OnPropertyChanged(nameof(VideoSource));
        }

        private WebViewSource CriarFonteDoVideo(Aula aula)
        {
            var videoId = YouTubePlayer.ExtrairId(aula.VideoId) ?? YouTubePlayer.ExtrairId(YouTubePlayer.VideoPadrao)!;

            // No Windows (WebView2) o HTML é entregue pela interceptação de requisições configurada
            // em MauiProgram; nas demais plataformas o BaseUrl HTTPS já define a origem da página.
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
                return new UrlWebViewSource { Url = YouTubePlayer.UrlDoPlayer(videoId, _segundosParaRetomar) };

            return new HtmlWebViewSource
            {
                Html = YouTubePlayer.GerarHtml(videoId, _segundosParaRetomar),
                BaseUrl = $"{YouTubePlayer.Origem}/"
            };
        }

        public double PercentualAssistido
        {
            get => _percentualAssistido;
            set
            {
                if (SetProperty(ref _percentualAssistido, value))
                    OnPropertyChanged(nameof(ProgressoTexto));
            }
        }

        public string ProgressoTexto => $"{PercentualAssistido:0}% assistido";

        public bool Concluida
        {
            get => _concluida;
            set
            {
                if (SetProperty(ref _concluida, value))
                {
                    OnPropertyChanged(nameof(ConcluirTexto));
                    OnPropertyChanged(nameof(PodeConcluir));
                }
            }
        }

        public bool PodeConcluir => !Concluida;

        public string ConcluirTexto => Concluida ? "Aula concluída" : "Marcar como concluída";

        public string AbaSelecionada
        {
            get => _abaSelecionada;
            set
            {
                if (SetProperty(ref _abaSelecionada, value))
                {
                    OnPropertyChanged(nameof(MostrarAulas));
                    OnPropertyChanged(nameof(MostrarMateriais));
                }
            }
        }

        public bool MostrarAulas => AbaSelecionada == "aulas";
        public bool MostrarMateriais => AbaSelecionada == "materiais";

        public ICommand ConcluirCommand { get; }
        public ICommand SelecionarAbaCommand { get; }
        public ICommand AbrirAulaCommand { get; }
        public ICommand AbrirMaterialCommand { get; }
        public ICommand VoltarCommand { get; }

        public async Task CarregarAsync(Aula aula, Matricula? matricula)
        {
            Matricula = matricula;
            await ExecutarAsync(async () =>
            {
                _progressos = matricula == null ? new() : await ProgressoRepositorio.DaMatriculaAsync(matricula.Id);
                await SelecionarAulaAsync(aula);
            });
        }

        private async Task SelecionarAulaAsync(Aula aula)
        {
            Aula = aula;
            Title = aula.Titulo;
            AbaSelecionada = "aulas";

            if (Matricula != null)
            {
                var progresso = ProgressoLocal(aula.Id);
                await ProgressoRepositorio.SalvarAsync(progresso);
            }

            AtualizarProgresso();
            CarregarAulas();
            await CarregarMateriaisAsync();
        }

        private ProgressoAula ProgressoLocal(string aulaId)
        {
            var progresso = _progressos.FirstOrDefault(p => p.AulaId == aulaId);
            if (progresso == null)
            {
                progresso = new ProgressoAula { Id = MongoContext.NovoId(), MatriculaId = Matricula?.Id ?? string.Empty, AulaId = aulaId };
                _progressos.Add(progresso);
            }

            return progresso;
        }

        private async Task AbrirAulaAsync(AulaItem? item)
        {
            if (item == null)
                return;

            if (!item.Liberada)
            {
                await AvisarAsync("Aula bloqueada", "Conclua a aula anterior para liberar esta aula.");
                return;
            }

            await ExecutarAsync(() => SelecionarAulaAsync(item.Aula));
        }

        private void AtualizarProgresso()
        {
            var progresso = Aula == null ? null : _progressos.FirstOrDefault(p => p.AulaId == Aula.Id);

            PercentualAssistido = progresso?.Concluida == true ? 100 : progresso?.PercentualAssistido ?? 0;
            Concluida = progresso?.Concluida ?? false;
            _segundosParaRetomar = progresso?.UltimaPosicaoSegundos ?? 0;

            _duracaoSegundos = 0;
            OnPropertyChanged(nameof(DuracaoFormatada));
            OnPropertyChanged(nameof(VideoSource));
        }

        private async Task CarregarMateriaisAsync()
        {
            Materiais.Clear();
            if (Aula != null)
            {
                foreach (var material in await MaterialRepositorio.DaAulaAsync(Aula.Id))
                    Materiais.Add(material);
            }

            OnPropertyChanged(nameof(PossuiMateriais));
        }

        private void CarregarAulas()
        {
            AulasDoModulo.Clear();
            var curso = Aula?.Materia?.Modulo?.Curso;
            var modulo = Aula?.Materia?.Modulo;
            if (curso == null || modulo == null)
                return;

            foreach (var materia in modulo.Materias.OrderBy(m => m.Ordem))
            {
                foreach (var aula in materia.Aulas.OrderBy(a => a.Ordem))
                {
                    var progresso = _progressos.FirstOrDefault(p => p.AulaId == aula.Id);
                    AulasDoModulo.Add(new AulaItem
                    {
                        Aula = aula,
                        Concluida = progresso?.Concluida ?? false,
                        Liberada = Matricula == null || ProgressoService.AulaEstaLiberada(curso, _progressos, aula),
                        IsSelecionada = Aula != null && aula.Id == Aula.Id,
                        Percentual = progresso?.PercentualAssistido ?? 0
                    });
                }
            }
        }

        private void SelecionarAba(string aba) => AbaSelecionada = aba;

        private async Task AbrirMaterialAsync(MaterialComplementar? material)
        {
            if (material == null)
                return;

            await ExecutarAsync(async () =>
            {
                if (material.IsArquivo)
                {
                    var caminho = await MaterialRepositorio.BaixarAsync(material);
                    await Launcher.Default.OpenAsync(new OpenFileRequest(material.Titulo, new ReadOnlyFile(caminho)));
                }
                else if (!string.IsNullOrWhiteSpace(material.Url))
                {
                    await Launcher.Default.OpenAsync(material.Url);
                }
            });
        }

        private async Task ConcluirAsync()
        {
            if (Aula == null)
                return;

            await ExecutarAsync(async () =>
            {
                if (Matricula != null)
                {
                    await ProgressoService.ConcluirAulaAsync(Matricula, Aula);
                    _progressos = await ProgressoRepositorio.DaMatriculaAsync(Matricula.Id);
                }

                PercentualAssistido = 100;
                Concluida = true;
                CarregarAulas();
            });
        }

        private static async Task SalvarEmSegundoPlanoAsync(Func<Task> operacao)
        {
            try
            {
                await operacao();
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(AulaViewModel), ex);
            }
        }
    }
}
