using System.Collections.ObjectModel;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class MateriaDesempenho
    {
        public string Nome { get; set; } = string.Empty;
        public string ModuloTitulo { get; set; } = string.Empty;
        public double Media { get; set; }
        public double Progresso { get; set; }
        public bool Aprovado { get; set; }
    }

    public class ResultadoItem
    {
        public string ProvaTitulo { get; set; } = string.Empty;
        public string MateriaNome { get; set; } = string.Empty;
        public double Nota { get; set; }
        public string AcertosTexto { get; set; } = string.Empty;
        public bool Aprovado { get; set; }
        public string Data => DataRealizacao.ToString("dd/MM/yyyy");
        public DateTime DataRealizacao { get; set; }

        public static ResultadoItem De(ResultadoProva resultado, Prova? prova) => new()
        {
            ProvaTitulo = prova?.Titulo ?? "Avaliação",
            MateriaNome = prova?.Materia?.Nome ?? string.Empty,
            Nota = resultado.NotaObtida,
            AcertosTexto = $"{resultado.Acertos}/{resultado.TotalQuestoes} acertos",
            Aprovado = resultado.NotaObtida >= (prova?.NotaCorte ?? ProgressoService.NotaCortePadrao),
            DataRealizacao = resultado.DataRealizacao
        };
    }

    public class DesempenhoViewModel : BaseViewModel
    {
        private double _mediaGeral;
        private bool _aprovado;
        private ObservableCollection<MateriaDesempenho> _materias = new();
        private ObservableCollection<ResultadoItem> _resultados = new();

        public DesempenhoViewModel()
        {
            Title = "Desempenho";
        }

        public ObservableCollection<MateriaDesempenho> Materias
        {
            get => _materias;
            private set => SetProperty(ref _materias, value);
        }

        public ObservableCollection<ResultadoItem> Resultados
        {
            get => _resultados;
            private set => SetProperty(ref _resultados, value);
        }

        public double MediaGeral
        {
            get => _mediaGeral;
            set
            {
                if (SetProperty(ref _mediaGeral, value))
                    OnPropertyChanged(nameof(MediaTexto));
            }
        }

        public string MediaTexto => MediaGeral.ToString("0.0");

        public bool Aprovado
        {
            get => _aprovado;
            set => SetProperty(ref _aprovado, value);
        }

        public Task CarregarAsync() => ExecutarAsync(async () =>
        {
            var aluno = SessionService.UsuarioAtual;
            var materias = new ObservableCollection<MateriaDesempenho>();
            var resultados = new ObservableCollection<ResultadoItem>();
            var notasGerais = new List<(ResultadoProva, Prova)>();

            if (aluno != null)
            {
                var matriculas = await MatriculaRepositorio.AtivasDoAlunoAsync(aluno.Id);
                var provas = await ProvaRepositorio.DosCursosAsync(matriculas.Select(m => m.CursoId));
                var doAluno = await ResultadoRepositorio.DoAlunoAsync(aluno.Id);

                foreach (var matricula in matriculas)
                {
                    var progressos = await ProgressoRepositorio.DaMatriculaAsync(matricula.Id);

                    foreach (var modulo in matricula.Curso!.Modulos.OrderBy(m => m.Ordem))
                    {
                        foreach (var materia in modulo.Materias.OrderBy(m => m.Ordem))
                        {
                            foreach (var prova in provas.Where(p => p.MateriaId == materia.Id))
                                prova.Materia = materia;

                            var notas = doAluno
                                .Where(r => r.MateriaId == materia.Id)
                                .Select(r => (Resultado: r, Prova: provas.FirstOrDefault(p => p.Id == r.ProvaId)))
                                .Where(n => n.Prova != null)
                                .Select(n => (n.Resultado, n.Prova!))
                                .ToList();
                            notasGerais.AddRange(notas);

                            var total = materia.Aulas.Count;
                            var concluidas = materia.Aulas.Count(a => progressos.Any(p => p.AulaId == a.Id && p.Concluida));
                            var media = Math.Round(ProgressoService.Media(notas), 1);

                            materias.Add(new MateriaDesempenho
                            {
                                Nome = materia.Nome,
                                ModuloTitulo = modulo.Titulo,
                                Media = media,
                                Progresso = total == 0 ? 0 : Math.Round(concluidas * 100.0 / total),
                                Aprovado = notas.Count > 0 && media >= ProgressoService.NotaCortePadrao
                            });
                        }
                    }
                }

                foreach (var resultado in doAluno)
                    resultados.Add(ResultadoItem.De(resultado, provas.FirstOrDefault(p => p.Id == resultado.ProvaId)));
            }

            Materias = materias;
            Resultados = resultados;

            MediaGeral = Math.Round(ProgressoService.Media(notasGerais), 1);
            Aprovado = notasGerais.Count > 0 && MediaGeral >= ProgressoService.NotaCortePadrao;
        });
    }
}
