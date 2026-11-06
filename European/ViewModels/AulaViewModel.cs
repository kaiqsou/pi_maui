using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>Tela da aula: vídeo, progresso, outras aulas do módulo e materiais complementares.</summary>
    public partial class AulaViewModel : BaseViewModel
    {
        private List<ProgressoAula> _progressos = new();
        private double _duracaoSegundos;
        private double _segundosParaRetomar;
        private bool _salvandoPosicao;

        // VideoSource é notificado em AtualizarProgresso, depois de saber o minuto em que o aluno parou
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DuracaoFormatada), nameof(MateriaNome))]
        public partial Aula? Aula { get; private set; }

        [ObservableProperty]
        public partial Matricula? Matricula { get; private set; }

        public ObservableCollection<MaterialComplementar> Materiais { get; } = new();
        public ObservableCollection<AulaItem> AulasDoModulo { get; } = new();

        public string MateriaNome => Aula?.Materia?.Nome ?? string.Empty;
        public string DuracaoFormatada => Formatos.Tempo(_duracaoSegundos > 0 ? _duracaoSegundos : Aula?.DuracaoSegundos ?? 0);

        /// <summary>Página do player, já posicionada onde o aluno parou.</summary>
        public WebViewSource VideoSource => Aula == null
            ? new UrlWebViewSource { Url = "about:blank" }
            : YouTubePlayer.Fonte(Aula.VideoId, _segundosParaRetomar);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProgressoTexto))]
        public partial double PercentualAssistido { get; private set; }

        public string ProgressoTexto => $"{PercentualAssistido:0}% assistido";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ConcluirTexto), nameof(PodeConcluir))]
        public partial bool Concluida { get; private set; }

        public bool PodeConcluir => !Concluida;
        public string ConcluirTexto => Concluida ? "Aula concluída" : "Marcar como concluída";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MostrarAulas), nameof(MostrarMateriais))]
        public partial string AbaSelecionada { get; set; } = "aulas";

        public bool MostrarAulas => AbaSelecionada == "aulas";
        public bool MostrarMateriais => AbaSelecionada == "materiais";

        protected override async Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            if (parametros.Obter<Aula>("Aula") is not { } aula)
                return;

            var matricula = parametros.Obter<Matricula>("Matricula");
            Matricula = matricula;
            await ExecutarAsync(async () =>
            {
                _progressos = matricula == null ? new() : await ProgressoRepositorio.DaMatriculaAsync(matricula.Id);
                await SelecionarAulaAsync(aula);
            });
        }

        /// <summary>Ao voltar para a tela, recarrega o player a partir da última posição salva.</summary>
        public override Task AoAparecerAsync()
        {
            if (Aula != null)
            {
                _segundosParaRetomar = ProgressoLocal(Aula.Id).UltimaPosicaoSegundos;
                OnPropertyChanged(nameof(VideoSource));
            }
            return Task.CompletedTask;
        }

        private async Task SelecionarAulaAsync(Aula aula)
        {
            Aula = aula;
            Title = aula.Titulo;
            AbaSelecionada = "aulas";

            if (Matricula != null)
                await ProgressoRepositorio.SalvarAsync(ProgressoLocal(aula.Id));

            AtualizarProgresso();
            CarregarAulas();
            await CarregarMateriaisAsync();
        }

        /// <summary>Progresso da aula já carregado (ou um novo, criado agora e mantido na lista).</summary>
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
        }

        private void CarregarAulas()
        {
            AulasDoModulo.Clear();
            var modulo = Aula?.Materia?.Modulo;
            var curso = modulo?.Curso;
            if (curso == null || modulo == null)
                return;

            foreach (var aula in modulo.Materias.OrderBy(m => m.Ordem).SelectMany(m => m.Aulas.OrderBy(a => a.Ordem)))
            {
                var progresso = _progressos.FirstOrDefault(p => p.AulaId == aula.Id);
                AulasDoModulo.Add(new AulaItem
                {
                    Aula = aula,
                    Concluida = progresso?.Concluida ?? false,
                    Liberada = Matricula == null || ProgressoService.AulaEstaLiberada(curso, _progressos, aula),
                    IsSelecionada = aula.Id == Aula?.Id,
                    Percentual = progresso?.PercentualAssistido ?? 0
                });
            }
        }

        // ---- Informações vindas do player (controle PlayerYouTube) ----

        /// <summary>Duração real do vídeo; corrige a cadastrada na aula se for diferente.</summary>
        [RelayCommand]
        private void RegistrarDuracao(double segundos)
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

        /// <summary>Posição atual do vídeo: guarda onde o aluno parou e quanto já assistiu.</summary>
        [RelayCommand]
        private void RegistrarPosicao(double segundos)
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

        // ---- Comandos da tela ----


        [RelayCommand]
        private void SelecionarAba(string aba) => AbaSelecionada = aba;

        [RelayCommand]
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

        [RelayCommand]
        private Task AbrirMaterialAsync(MaterialComplementar? material)
        {
            if (material == null)
                return Task.CompletedTask;

            return ExecutarAsync(async () =>
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

        [RelayCommand]
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
    }
}
