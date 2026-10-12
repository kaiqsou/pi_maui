using System.Collections.ObjectModel;
using European.MockData;
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
        public bool Aprovado { get; set; }
        public string Data => DataRealizacao.ToString("dd/MM/yyyy");
        public DateTime DataRealizacao { get; set; }
    }

    public class DesempenhoViewModel : BaseViewModel
    {
        private const double NotaCorte = 6;
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

        public void Carregar()
        {
            var aluno = SessionService.UsuarioAtual;
            var materias = new ObservableCollection<MateriaDesempenho>();
            var resultados = new ObservableCollection<ResultadoItem>();

            if (aluno != null)
            {
                var matriculas = MockDataService.Matriculas().Where(m => m.AlunoId == aluno.Id).ToList();

                foreach (var matricula in matriculas)
                {
                    if (matricula.Curso == null)
                        continue;

                    foreach (var modulo in matricula.Curso.Modulos.OrderBy(m => m.Ordem))
                    {
                        foreach (var materia in modulo.Materias.OrderBy(m => m.Ordem))
                        {
                            var notas = MockDataService.Resultados()
                                .Where(r => r.AlunoId == aluno.Id && (r.Prova?.MateriaId) == materia.Id)
                                .Select(r => r.NotaObtida)
                                .ToList();

                            var media = notas.Count == 0 ? 0 : Math.Round(notas.Average(), 1);

                            var total = materia.Aulas.Count;
                            var concluidas = materia.Aulas.Count(a => MockDataService.ProgressoDaAula(matricula.Id, a.Id)?.Concluida ?? false);

                            materias.Add(new MateriaDesempenho
                            {
                                Nome = materia.Nome,
                                ModuloTitulo = modulo.Titulo,
                                Media = media,
                                Progresso = total == 0 ? 0 : Math.Round(concluidas * 100.0 / total),
                                Aprovado = notas.Count > 0 && media >= NotaCorte
                            });
                        }
                    }
                }

                foreach (var resultado in MockDataService.Resultados().Where(r => r.AlunoId == aluno.Id))
                {
                    resultados.Add(new ResultadoItem
                    {
                        ProvaTitulo = resultado.Prova?.Titulo ?? "Avaliação",
                        MateriaNome = resultado.Prova?.Materia?.Nome ?? string.Empty,
                        Nota = resultado.NotaObtida,
                        Aprovado = resultado.NotaObtida >= (resultado.Prova?.NotaCorte ?? NotaCorte),
                        DataRealizacao = resultado.DataRealizacao
                    });
                }
            }

            Materias = materias;
            Resultados = resultados;

            MediaGeral = Resultados.Count == 0 ? 0 : Math.Round(Resultados.Average(r => r.Nota), 1);
            Aprovado = Resultados.Count > 0 && MediaGeral >= NotaCorte;
        }
    }
}
