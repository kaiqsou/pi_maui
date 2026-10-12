using System.Collections.ObjectModel;
using System.Windows.Input;
using European.MockData;
using European.Models;

namespace European.ViewModels
{
    public class AulaViewModel : BaseViewModel
    {
        private const string VideoNome = "AppHotel.mp4";

        private Aula? _aula;
        private Matricula? _matricula;
        private double _percentualAssistido;
        private bool _concluida;
        private string _abaSelecionada = "aulas";
        private double _duracaoSegundos;
        private double _segundosParaRetomar;

        public AulaViewModel()
        {
            ConcluirCommand = new Command(Concluir);
            SelecionarAbaCommand = new Command<string>(SelecionarAba);
            AbrirAulaCommand = new Command<AulaItem>(async item => await AbrirAulaAsync(item));
            AbrirMaterialCommand = new Command<MaterialComplementar>(async m => await AbrirMaterialAsync(m));
            VoltarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
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
                    OnPropertyChanged(nameof(VideoSource));
                    CarregarMateriais();
                    CarregarAulas();
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

                var total = _duracaoSegundos > 0 ? _duracaoSegundos : MockDataService.DuracaoVideosDaAula(Aula);
                var horas = (int)(total / 3600);
                return horas > 0
                    ? TimeSpan.FromSeconds(total).ToString(@"h\:mm\:ss")
                    : TimeSpan.FromSeconds(total).ToString(@"mm\:ss");
            }
        }

        public string MateriaNome => Aula?.Materia?.Nome ?? string.Empty;

        public WebViewSource VideoSource
        {
            get
            {
                if (Aula == null)
                    return new UrlWebViewSource { Url = "about:blank" };

                return new UrlWebViewSource { Url = CaminhoDoVideo() };
            }
        }

        public double SegundosParaRetomar => _segundosParaRetomar;

        public void RegistrarDuracao(double segundos)
        {
            if (segundos <= 0 || Math.Abs(segundos - _duracaoSegundos) < 1)
                return;

            _duracaoSegundos = segundos;

            if (Aula != null)
                MockDataService.SalvarDuracaoRealAula(Aula.Id, segundos);

            OnPropertyChanged(nameof(DuracaoFormatada));
        }

        public void RegistrarPosicao(double segundos)
        {
            if (Aula == null || Matricula == null || segundos < 0)
                return;

            var total = _duracaoSegundos > 0 ? _duracaoSegundos : MockDataService.DuracaoVideosDaAula(Aula);
            MockDataService.SalvarPosicaoAula(Matricula.Id, Aula.Id, segundos, total);

            if (total > 0)
                PercentualAssistido = Math.Min(100, segundos / total * 100);
        }

        public void RecarregarVideo()
        {
            _segundosParaRetomar = Aula == null || Matricula == null
                ? 0
                : MockDataService.ProgressoDaAula(Matricula.Id, Aula.Id)?.UltimaPosicaoSegundos ?? 0;

            OnPropertyChanged(nameof(VideoSource));
        }

        private static string CaminhoDoVideo()
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                    return $"file:///android_asset/Videos/{VideoNome}";

                if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.MacCatalyst)
                    return $"file://{Path.Combine(FileSystem.Current.AppDataDirectory, VideoNome).Replace('\\', '/')}";

                var candidatos = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "Videos", VideoNome),
                    Path.Combine(AppContext.BaseDirectory, VideoNome)
                };

                var caminho = candidatos.FirstOrDefault(File.Exists) ?? candidatos[0];
                return $"file:///{caminho.Replace('\\', '/')}";
            }
            catch
            {
                return "about:blank";
            }
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

        public void Carregar(Aula aula, Matricula? matricula)
        {
            Matricula = matricula;
            Aula = aula;
            Title = aula.Titulo;

            if (matricula != null)
                MockDataService.RegistrarAcessoAula(matricula.Id, aula.Id);

            AtualizarProgresso();
        }

        private async Task AbrirAulaAsync(AulaItem? item)
        {
            if (item == null)
                return;

            if (!item.Liberada)
            {
                await Shell.Current.DisplayAlert(
                    "Aula bloqueada",
                    "Conclua a aula anterior para liberar esta aula.",
                    "OK");
                return;
            }

            Aula = item.Aula;
            Title = item.Aula.Titulo;
            AbaSelecionada = "aulas";

            if (Matricula != null)
                MockDataService.RegistrarAcessoAula(Matricula.Id, item.Aula.Id);

            AtualizarProgresso();
        }

        private void AtualizarProgresso()
        {
            var progresso = Aula == null || Matricula == null
                ? null
                : MockDataService.ProgressoDaAula(Matricula.Id, Aula.Id);

            PercentualAssistido = progresso?.PercentualAssistido ?? 0;
            Concluida = progresso?.Concluida ?? false;
            _segundosParaRetomar = progresso?.UltimaPosicaoSegundos ?? 0;

            _duracaoSegundos = 0;
            OnPropertyChanged(nameof(DuracaoFormatada));
        }

        private void CarregarMateriais()
        {
            Materiais.Clear();
            if (Aula == null)
                return;

            foreach (var material in MockDataService.Materiais().Where(m => m.AulaId == Aula.Id))
                Materiais.Add(material);
        }

        private void CarregarAulas()
        {
            AulasDoModulo.Clear();
            if (Aula?.Materia?.Modulo == null)
                return;

            var matriculaId = Matricula?.Id;

            foreach (var materia in Aula.Materia.Modulo.Materias.OrderBy(m => m.Ordem))
            {
                foreach (var aula in materia.Aulas.OrderBy(a => a.Ordem))
                {
                    var progresso = Matricula == null ? null : MockDataService.ProgressoDaAula(Matricula.Id, aula.Id);
                    AulasDoModulo.Add(new AulaItem
                    {
                        Aula = aula,
                        Concluida = progresso?.Concluida ?? false,
                        Liberada = matriculaId == null || MockDataService.AulaEstaLiberada(matriculaId, aula),
                        IsSelecionada = Aula != null && aula.Id == Aula.Id,
                        Percentual = progresso?.PercentualAssistido ?? 0
                    });
                }
            }
        }

        private void SelecionarAba(string aba) => AbaSelecionada = aba;

        private static async Task AbrirMaterialAsync(MaterialComplementar? material)
        {
            if (material == null || string.IsNullOrWhiteSpace(material.Url))
                return;

            try
            {
                await Launcher.Default.OpenAsync(material.Url);
            }
            catch
            {
                // link externo indisponível no ambiente de teste
            }
        }

        private void Concluir()
        {
            if (Aula == null)
                return;

            if (Matricula != null)
                MockDataService.ConcluirAula(Matricula, Aula);

            PercentualAssistido = 100;
            Concluida = true;
            CarregarAulas();
        }
    }
}
