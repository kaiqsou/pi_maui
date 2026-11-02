using System.Collections.ObjectModel;
using System.Windows.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>
    /// Tela inicial do aluno: só o que ajuda a decidir o próximo passo — continuar a aula, ver o
    /// andamento do curso, provas pendentes e, quando houver, o certificado.
    /// </summary>
    public class InicioViewModel : BaseViewModel
    {
        private const int MaxProvasPendentes = 3;

        private string _saudacao = string.Empty;
        private Aula? _ultimaAula;
        private string _ultimaCursoNome = string.Empty;
        private Matricula? _matriculaAtiva;
        private string _cardTitulo = string.Empty;
        private string _botaoTexto = string.Empty;
        private string _ultimaDuracao = string.Empty;
        private double _ultimaPercentual;
        private double _progressoCurso;
        private string _aulasTexto = string.Empty;
        private string _moduloTexto = string.Empty;
        private ObservableCollection<ProvaItem> _provasPendentes = new();
        private int _totalProvasPendentes;
        private Certificado? _certificado;
        private string _validadeTexto = string.Empty;

        public InicioViewModel()
        {
            Title = "Início";
            ContinuarCommand = new Command(async () => await ContinuarAsync());
            IniciarAulasCommand = new Command(() => MainViewModel.Atual?.SelecionarAba("aulas"));
            AbrirProvasCommand = new Command(() => MainViewModel.Atual?.SelecionarAba("provas"));
            AbrirCertificadoCommand = new Command(() => MainViewModel.Atual?.SelecionarAba("certificados"));
            AbrirPerfilCommand = new Command(async () => await Shell.Current.GoToAsync("perfil"));
        }

        public string Saudacao
        {
            get => _saudacao;
            set => SetProperty(ref _saudacao, value);
        }

        public Matricula? MatriculaAtiva
        {
            get => _matriculaAtiva;
            set
            {
                if (SetProperty(ref _matriculaAtiva, value))
                {
                    OnPropertyChanged(nameof(TemCurso));
                    OnPropertyChanged(nameof(SemCurso));
                    OnPropertyChanged(nameof(MostrarIniciarAulas));
                }
            }
        }

        public bool TemCurso => MatriculaAtiva != null;
        public bool SemCurso => MatriculaAtiva == null;

        public string UltimaCursoNome
        {
            get => _ultimaCursoNome;
            set => SetProperty(ref _ultimaCursoNome, value);
        }

        // ---- Continuar estudando ----

        /// <summary>A aula em que o aluno parou: a primeira ainda não concluída, na ordem do curso.</summary>
        public Aula? UltimaAula
        {
            get => _ultimaAula;
            set
            {
                if (SetProperty(ref _ultimaAula, value))
                {
                    OnPropertyChanged(nameof(TemUltimaAula));
                    OnPropertyChanged(nameof(MostrarIniciarAulas));
                }
            }
        }

        public bool TemUltimaAula => UltimaAula != null;

        /// <summary>Todas as aulas já concluídas: o card oferece rever as aulas.</summary>
        public bool MostrarIniciarAulas => UltimaAula == null && MatriculaAtiva != null;

        public string CardTitulo
        {
            get => _cardTitulo;
            private set => SetProperty(ref _cardTitulo, value);
        }

        public string BotaoTexto
        {
            get => _botaoTexto;
            private set => SetProperty(ref _botaoTexto, value);
        }

        /// <summary>"Parou em 03:25 de 07:08" quando a aula foi assistida pela metade; senão, só a duração.</summary>
        public string UltimaDuracao
        {
            get => _ultimaDuracao;
            private set => SetProperty(ref _ultimaDuracao, value);
        }

        /// <summary>De 0 a 100, quanto da aula atual já foi assistido.</summary>
        public double UltimaPercentual
        {
            get => _ultimaPercentual;
            private set
            {
                if (SetProperty(ref _ultimaPercentual, value))
                    OnPropertyChanged(nameof(UltimaIniciada));
            }
        }

        public bool UltimaIniciada => UltimaPercentual > 0;

        // ---- Andamento do curso ----

        public double ProgressoCurso
        {
            get => _progressoCurso;
            private set => SetProperty(ref _progressoCurso, value);
        }

        public string AulasTexto
        {
            get => _aulasTexto;
            private set => SetProperty(ref _aulasTexto, value);
        }

        public string ModuloTexto
        {
            get => _moduloTexto;
            private set
            {
                if (SetProperty(ref _moduloTexto, value))
                    OnPropertyChanged(nameof(TemModuloTexto));
            }
        }

        public bool TemModuloTexto => !string.IsNullOrEmpty(ModuloTexto);

        // ---- Provas pendentes ----

        public ObservableCollection<ProvaItem> ProvasPendentes
        {
            get => _provasPendentes;
            private set
            {
                if (SetProperty(ref _provasPendentes, value))
                    OnPropertyChanged(nameof(TemProvasPendentes));
            }
        }

        public bool TemProvasPendentes => ProvasPendentes.Count > 0;

        public int TotalProvasPendentes
        {
            get => _totalProvasPendentes;
            private set
            {
                if (SetProperty(ref _totalProvasPendentes, value))
                    OnPropertyChanged(nameof(ProvasPendentesTexto));
            }
        }

        public string ProvasPendentesTexto => TotalProvasPendentes == 1 ? "1 prova para fazer" : $"{TotalProvasPendentes} provas para fazer";

        // ---- Certificado ----

        public Certificado? Certificado
        {
            get => _certificado;
            private set
            {
                if (SetProperty(ref _certificado, value))
                    OnPropertyChanged(nameof(TemCertificado));
            }
        }

        public bool TemCertificado => Certificado != null;

        public string ValidadeTexto
        {
            get => _validadeTexto;
            private set => SetProperty(ref _validadeTexto, value);
        }

        public ICommand ContinuarCommand { get; }
        public ICommand IniciarAulasCommand { get; }
        public ICommand AbrirProvasCommand { get; }
        public ICommand AbrirCertificadoCommand { get; }
        public ICommand AbrirPerfilCommand { get; }

        public Task CarregarAsync() => ExecutarAsync(async () =>
        {
            var aluno = SessionService.UsuarioAtual;
            if (aluno == null)
                return;

            Saudacao = $"Olá, {PrimeiroNome(aluno.Nome)}";

            // inclui a matrícula concluída (dentro da validade), para o aluno poder rever as aulas; ativas vêm primeiro
            var matriculas = await MatriculaRepositorio.ComConteudoDoAlunoAsync(aluno.Id);
            MatriculaAtiva = matriculas.FirstOrDefault(m => m.Curso != null);
            UltimaCursoNome = MatriculaAtiva?.Curso?.Nome ?? string.Empty;

            await CarregarContinuarEAndamentoAsync();
            await CarregarProvasPendentesAsync(aluno.Id, matriculas);

            Certificado = (await CertificadoRepositorio.DoAlunoAsync(aluno.Id)).FirstOrDefault();
            if (Certificado != null)
            {
                var acessoAte = MatriculaAtiva?.AcessoAte ?? Certificado.EmitidoEm.AddMonths(Matricula.MesesAcessoAposConclusao);
                ValidadeTexto = $"Aulas disponíveis até {acessoAte:dd/MM/yyyy}";
            }
            else
            {
                ValidadeTexto = string.Empty;
            }
        });

        private async Task CarregarContinuarEAndamentoAsync()
        {
            var curso = MatriculaAtiva?.Curso;
            Aula? proxima = null;
            ProgressoAula? progresso = null;
            var algumProgresso = false;
            var concluidas = new HashSet<string>();

            if (MatriculaAtiva != null && curso != null)
            {
                var progressos = await ProgressoRepositorio.DaMatriculaAsync(MatriculaAtiva.Id);
                concluidas = progressos.Where(p => p.Concluida).Select(p => p.AulaId).ToHashSet();
                algumProgresso = progressos.Count > 0;

                // as aulas são liberadas em sequência, então a primeira não concluída é sempre onde o aluno parou
                proxima = curso.TodasAulas.FirstOrDefault(a => !concluidas.Contains(a.Id));
                progresso = proxima == null ? null : progressos.FirstOrDefault(p => p.AulaId == proxima.Id);
            }

            UltimaAula = proxima;
            OnPropertyChanged(nameof(MostrarIniciarAulas));

            var posicao = progresso?.UltimaPosicaoSegundos ?? 0;
            UltimaPercentual = progresso?.PercentualAssistido ?? 0;

            if (proxima == null)
            {
                CardTitulo = "Todas as aulas concluídas";
                BotaoTexto = "Rever aulas";
                UltimaDuracao = string.Empty;
            }
            else if (posicao >= 1)
            {
                CardTitulo = "Continuar de onde parou";
                BotaoTexto = "Continuar aula";
                UltimaDuracao = proxima.DuracaoSegundos > 0
                    ? $"Parou em {Tempo(posicao)} de {proxima.DuracaoTexto}"
                    : $"Parou em {Tempo(posicao)}";
            }
            else
            {
                CardTitulo = algumProgresso ? "Próxima aula" : "Comece seus estudos";
                BotaoTexto = algumProgresso ? "Assistir próxima aula" : "Assistir primeira aula";
                UltimaDuracao = proxima.DuracaoSegundos > 0 ? proxima.DuracaoTexto : string.Empty;
            }

            // andamento: aulas concluídas e em qual módulo o aluno está
            var totalAulas = curso?.TotalAulas ?? 0;
            var feitas = curso?.TodasAulas.Count(a => concluidas.Contains(a.Id)) ?? 0;
            ProgressoCurso = totalAulas == 0 ? 0 : Math.Round(feitas * 100.0 / totalAulas);
            AulasTexto = totalAulas == 0 ? "Nenhuma aula publicada ainda" : $"{feitas} de {totalAulas} aulas concluídas";

            var modulos = curso?.Modulos.OrderBy(m => m.Ordem).ToList() ?? new List<Modulo>();
            var moduloAtual = proxima?.Materia?.Modulo;
            ModuloTexto = moduloAtual == null || modulos.Count == 0
                ? string.Empty
                : $"Módulo {modulos.IndexOf(moduloAtual) + 1} de {modulos.Count} · {moduloAtual.Titulo}";
        }

        private async Task CarregarProvasPendentesAsync(string alunoId, List<Matricula> matriculas)
        {
            var provas = await ProvaService.VisiveisParaAlunoAsync(alunoId, matriculas);
            var respondidas = (await ResultadoRepositorio.DoAlunoAsync(alunoId)).Select(r => r.ProvaId).ToHashSet();

            // VisiveisParaAlunoAsync já ordena pelo prazo mais próximo
            var pendentes = provas.Where(p => !respondidas.Contains(p.Id) && !p.Expirada).ToList();
            TotalProvasPendentes = pendentes.Count;
            ProvasPendentes = new ObservableCollection<ProvaItem>(
                pendentes.Take(MaxProvasPendentes).Select(p => new ProvaItem { Prova = p }));
        }

        private async Task ContinuarAsync()
        {
            if (UltimaAula == null || MatriculaAtiva == null)
                return;

            var parametros = new Dictionary<string, object>
            {
                ["Aula"] = UltimaAula,
                ["Matricula"] = MatriculaAtiva
            };
            await Shell.Current.GoToAsync("aula", parametros);
        }

        private static string Tempo(double segundos)
            => TimeSpan.FromSeconds(segundos).ToString(segundos >= 3600 ? @"h\:mm\:ss" : @"mm\:ss");

        private static string PrimeiroNome(string nome)
            => string.IsNullOrWhiteSpace(nome) ? string.Empty : nome.Split(' ')[0];
    }
}
