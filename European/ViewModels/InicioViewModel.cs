using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    /// <summary>
    /// Tela inicial do aluno: só o que ajuda a decidir o próximo passo — continuar a aula, ver o
    /// andamento do curso, provas pendentes e, quando houver, o certificado.
    /// </summary>
    public partial class InicioViewModel : BaseViewModel
    {
        private const int MaxProvasPendentes = 3;

        public InicioViewModel()
        {
            Title = "Início";
        }

        /// <summary>Pede à tela principal para trocar de aba ("aulas", "provas"...). A MainPage liga este evento.</summary>
        public event Action<string>? AbaSolicitada;

        /// <summary>Notificações não lidas (sino no cabeçalho).</summary>
        public Sino Sino { get; } = new();

        [ObservableProperty]
        public partial string Saudacao { get; private set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemCurso), nameof(SemCurso), nameof(MostrarIniciarAulas))]
        public partial Matricula? MatriculaAtiva { get; private set; }

        public bool TemCurso => MatriculaAtiva != null;
        public bool SemCurso => MatriculaAtiva == null;

        [ObservableProperty]
        public partial string UltimaCursoNome { get; private set; } = string.Empty;

        // ---- Continuar estudando ----

        /// <summary>A aula em que o aluno parou: a primeira ainda não concluída, na ordem do curso.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemUltimaAula), nameof(MostrarIniciarAulas))]
        public partial Aula? UltimaAula { get; private set; }

        public bool TemUltimaAula => UltimaAula != null;

        /// <summary>Todas as aulas já concluídas: o card oferece rever as aulas.</summary>
        public bool MostrarIniciarAulas => UltimaAula == null && MatriculaAtiva != null;

        [ObservableProperty]
        public partial string CardTitulo { get; private set; } = string.Empty;

        [ObservableProperty]
        public partial string BotaoTexto { get; private set; } = string.Empty;

        /// <summary>"Parou em 03:25 de 07:08" quando a aula foi assistida pela metade; senão, só a duração.</summary>
        [ObservableProperty]
        public partial string UltimaDuracao { get; private set; } = string.Empty;

        /// <summary>De 0 a 100, quanto da aula atual já foi assistido.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UltimaIniciada))]
        public partial double UltimaPercentual { get; private set; }

        public bool UltimaIniciada => UltimaPercentual > 0;

        // ---- Andamento do curso ----

        [ObservableProperty]
        public partial double ProgressoCurso { get; private set; }

        [ObservableProperty]
        public partial string AulasTexto { get; private set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemModuloTexto))]
        public partial string ModuloTexto { get; private set; } = string.Empty;

        public bool TemModuloTexto => !string.IsNullOrEmpty(ModuloTexto);

        // ---- Provas pendentes ----

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemProvasPendentes))]
        public partial ObservableCollection<ProvaItem> ProvasPendentes { get; private set; } = new();

        public bool TemProvasPendentes => ProvasPendentes.Count > 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProvasPendentesTexto))]
        public partial int TotalProvasPendentes { get; private set; }

        public string ProvasPendentesTexto => TotalProvasPendentes == 1 ? "1 prova para fazer" : $"{TotalProvasPendentes} provas para fazer";

        // ---- Certificado ----

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TemCertificado))]
        public partial Certificado? Certificado { get; private set; }

        public bool TemCertificado => Certificado != null;

        [ObservableProperty]
        public partial string ValidadeTexto { get; private set; } = string.Empty;

        public override Task AoAparecerAsync() => ExecutarAsync(async () =>
        {
            var aluno = SessionService.UsuarioAtual;
            if (aluno == null)
                return;

            Saudacao = Formatos.Saudacao(aluno.Nome);
            await Sino.AtualizarAsync();

            // inclui a matrícula concluída (dentro da validade), para o aluno poder rever as aulas; ativas vêm primeiro
            var matriculas = await MatriculaRepositorio.ComConteudoDoAlunoAsync(aluno.Id);
            MatriculaAtiva = matriculas.FirstOrDefault(m => m.Curso != null);
            UltimaCursoNome = MatriculaAtiva?.Curso?.Nome ?? string.Empty;

            await CarregarContinuarEAndamentoAsync();
            await CarregarProvasPendentesAsync(aluno.Id, matriculas);

            Certificado = (await CertificadoRepositorio.DoAlunoAsync(aluno.Id)).FirstOrDefault();
            ValidadeTexto = Certificado == null
                ? string.Empty
                : $"Aulas disponíveis até {Certificado.AcessoAulasAte(MatriculaAtiva):dd/MM/yyyy}";
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
                    ? $"Parou em {Formatos.Tempo(posicao)} de {proxima.DuracaoTexto}"
                    : $"Parou em {Formatos.Tempo(posicao)}";
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

        [RelayCommand]
        private Task Continuar()
            => UltimaAula == null || MatriculaAtiva == null
                ? Task.CompletedTask
                : Navegacao.IrAsync(Navegacao.Aula, ("Aula", UltimaAula), ("Matricula", MatriculaAtiva));

        [RelayCommand]
        private void IniciarAulas() => AbaSolicitada?.Invoke("aulas");

        [RelayCommand]
        private void AbrirProvas() => AbaSolicitada?.Invoke("provas");

        [RelayCommand]
        private void AbrirCertificado() => AbaSolicitada?.Invoke("certificados");

        [RelayCommand]
        private Task AbrirPerfil() => Navegacao.IrAsync(Navegacao.Perfil);
    }
}
