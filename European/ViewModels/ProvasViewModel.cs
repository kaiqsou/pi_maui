using System.Collections.ObjectModel;
using System.Windows.Input;
using European.MockData;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class ProvaItem
    {
        public string Titulo { get; set; } = string.Empty;
        public string MateriaNome { get; set; } = string.Empty;
        public string Prazo { get; set; } = string.Empty;
        public string Duracao { get; set; } = string.Empty;
        public double NotaCorte { get; set; }
    }

    public class ProvasViewModel : BaseViewModel
    {
        private string _abaSelecionada = "disponiveis";
        private ObservableCollection<ProvaItem> _disponiveis = new();
        private ObservableCollection<ResultadoItem> _realizadas = new();

        public ProvasViewModel()
        {
            Title = "Provas";
            SelecionarAbaCommand = new Command<string>(aba => AbaSelecionada = aba);
        }

        public ObservableCollection<ProvaItem> Disponiveis
        {
            get => _disponiveis;
            private set => SetProperty(ref _disponiveis, value);
        }

        public ObservableCollection<ResultadoItem> Realizadas
        {
            get => _realizadas;
            private set => SetProperty(ref _realizadas, value);
        }

        public string AbaSelecionada
        {
            get => _abaSelecionada;
            set
            {
                if (SetProperty(ref _abaSelecionada, value))
                {
                    OnPropertyChanged(nameof(MostrarDisponiveis));
                    OnPropertyChanged(nameof(MostrarRealizadas));
                    OnPropertyChanged(nameof(MostrarVazioDisponiveis));
                    OnPropertyChanged(nameof(MostrarVazioRealizadas));
                }
            }
        }

        public bool MostrarDisponiveis => AbaSelecionada == "disponiveis";
        public bool MostrarRealizadas => AbaSelecionada == "realizadas";

        public bool PossuiDisponiveis => Disponiveis.Count > 0;
        public bool PossuiRealizadas => Realizadas.Count > 0;
        public bool MostrarVazioDisponiveis => MostrarDisponiveis && !PossuiDisponiveis;
        public bool MostrarVazioRealizadas => MostrarRealizadas && !PossuiRealizadas;

        public ICommand SelecionarAbaCommand { get; }

        public void Carregar()
        {
            var aluno = SessionService.UsuarioAtual;
            var disponiveis = new ObservableCollection<ProvaItem>();
            var realizadas = new ObservableCollection<ResultadoItem>();

            if (aluno != null)
            {
                var materiasDoAluno = MockDataService.Matriculas()
                    .Where(m => m.AlunoId == aluno.Id)
                    .SelectMany(m => m.Curso?.Modulos.SelectMany(x => x.Materias) ?? Enumerable.Empty<Materia>())
                    .Select(materia => materia.Id)
                    .ToHashSet();

                var respondidas = MockDataService.Resultados()
                    .Where(r => r.AlunoId == aluno.Id)
                    .Select(r => r.ProvaId)
                    .ToHashSet();

                foreach (var prova in MockDataService.Provas().Where(p => materiasDoAluno.Contains(p.MateriaId)))
                {
                    if (respondidas.Contains(prova.Id))
                        continue;

                    disponiveis.Add(new ProvaItem
                    {
                        Titulo = prova.Titulo,
                        MateriaNome = prova.Materia?.Nome ?? string.Empty,
                        Prazo = prova.Prazo.ToString("dd/MM/yyyy"),
                        Duracao = $"{prova.DuracaoMinutos} min",
                        NotaCorte = prova.NotaCorte
                    });
                }

                foreach (var resultado in MockDataService.Resultados().Where(r => r.AlunoId == aluno.Id))
                {
                    realizadas.Add(new ResultadoItem
                    {
                        ProvaTitulo = resultado.Prova?.Titulo ?? "Avaliação",
                        MateriaNome = resultado.Prova?.Materia?.Nome ?? string.Empty,
                        Nota = resultado.NotaObtida,
                        Aprovado = resultado.NotaObtida >= (resultado.Prova?.NotaCorte ?? 6),
                        DataRealizacao = resultado.DataRealizacao
                    });
                }
            }

            Disponiveis = disponiveis;
            Realizadas = realizadas;

            OnPropertyChanged(nameof(PossuiDisponiveis));
            OnPropertyChanged(nameof(PossuiRealizadas));
            OnPropertyChanged(nameof(MostrarVazioDisponiveis));
            OnPropertyChanged(nameof(MostrarVazioRealizadas));
        }
    }
}
