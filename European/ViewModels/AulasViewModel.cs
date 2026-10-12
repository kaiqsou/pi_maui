using System.Collections.ObjectModel;
using System.Windows.Input;
using European.MockData;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class AulaItem
    {
        public Aula Aula { get; set; } = new();
        public bool Concluida { get; set; }
        public bool Liberada { get; set; } = true;
        public bool IsSelecionada { get; set; }
        public double Percentual { get; set; }
        public string Cabecalho { get; set; } = string.Empty;
        public bool IniciaGrupo { get; set; }
        public bool MostrarCabecalho { get; set; }

        public string Duracao => TimeSpan.FromSeconds(MockDataService.DuracaoVideosDaAula(Aula)).ToString(@"mm\:ss");
        public string Descricao => Aula.Descricao;
        public bool Bloqueada => !Liberada;
        public bool PodeAssistir => Liberada && !Concluida;
        public string MateriaNome => Aula.Materia?.Nome ?? string.Empty;
    }

    public class AulasViewModel : BaseViewModel
    {
        private Matricula? _matriculaAtiva;
        private string _cursoNome = string.Empty;
        private ObservableCollection<Matricula> _matriculas = new();
        private List<Modulo> _modulos = new();
        private List<List<AulaItem>> _aulasPorModulo = new();
        private int _indicePagina;
        private IReadOnlyList<AulaItem> _aulasPagina = new List<AulaItem>();

        public AulasViewModel()
        {
            Title = "Aulas";
            AbrirAulaCommand = new Command<AulaItem>(async item => await AbrirAulaAsync(item));
            PaginaAnteriorCommand = new Command(() => MudarPagina(-1));
            PaginaProximaCommand = new Command(() => MudarPagina(1));
        }

        public ObservableCollection<Matricula> Matriculas
        {
            get => _matriculas;
            private set => SetProperty(ref _matriculas, value);
        }

        public string CursoNome
        {
            get => _cursoNome;
            set => SetProperty(ref _cursoNome, value);
        }

        public Matricula? MatriculaAtiva
        {
            get => _matriculaAtiva;
            set => SetProperty(ref _matriculaAtiva, value);
        }

        public IReadOnlyList<AulaItem> AulasPagina
        {
            get => _aulasPagina;
            private set => SetProperty(ref _aulasPagina, value);
        }

        public int TotalPaginas => _modulos.Count == 0 ? 1 : _modulos.Count;
        public int PaginaAtual => _indicePagina + 1;
        public bool TemPaginacao => TotalPaginas > 1;
        public bool PodePaginaAnterior => _indicePagina > 0;
        public bool PodePaginaProxima => _indicePagina < TotalPaginas - 1;
        public string IndicadorPagina => $"{PaginaAtual} / {TotalPaginas}";
        public string ModuloAtualTitulo => _indicePagina >= 0 && _indicePagina < _modulos.Count
            ? _modulos[_indicePagina].Titulo
            : string.Empty;

        public ICommand AbrirAulaCommand { get; }
        public ICommand PaginaAnteriorCommand { get; }
        public ICommand PaginaProximaCommand { get; }

        public void Carregar()
        {
            var aluno = SessionService.UsuarioAtual;

            var matriculas = new List<Matricula>();
            var modulos = new List<Modulo>();
            var aulasPorModulo = new List<List<AulaItem>>();

            if (aluno != null)
            {
                foreach (var matricula in MockDataService.Matriculas().Where(m => m.AlunoId == aluno.Id))
                    matriculas.Add(matricula);

                var matriculaAtiva = matriculas.FirstOrDefault();
                MatriculaAtiva = matriculaAtiva;

                if (matriculaAtiva?.Curso != null)
                {
                    CursoNome = matriculaAtiva.Curso.Nome;

                    foreach (var modulo in matriculaAtiva.Curso.Modulos.OrderBy(m => m.Ordem))
                    {
                        var aulasModulo = new List<AulaItem>();

                        foreach (var materia in modulo.Materias.OrderBy(m => m.Ordem))
                        {
                            var primeiroDaMateria = true;

                            foreach (var aula in materia.Aulas.OrderBy(a => a.Ordem))
                            {
                                var progresso = MockDataService.ProgressoDaAula(matriculaAtiva.Id, aula.Id);
                                aulasModulo.Add(new AulaItem
                                {
                                    Aula = aula,
                                    Concluida = progresso?.Concluida ?? false,
                                    Liberada = MockDataService.AulaEstaLiberada(matriculaAtiva.Id, aula),
                                    Percentual = progresso?.PercentualAssistido ?? 0,
                                    Cabecalho = materia.Nome,
                                    IniciaGrupo = primeiroDaMateria
                                });

                                primeiroDaMateria = false;
                            }
                        }

                        modulos.Add(modulo);
                        aulasPorModulo.Add(aulasModulo);
                    }
                }
            }
            else
            {
                MatriculaAtiva = null;
                CursoNome = string.Empty;
            }

            Matriculas = new ObservableCollection<Matricula>(matriculas);
            _modulos = modulos;
            _aulasPorModulo = aulasPorModulo;
            _indicePagina = 0;
            AplicarPagina();
        }

        private void MudarPagina(int delta)
        {
            var novo = _indicePagina + delta;
            if (novo < 0 || novo >= TotalPaginas)
                return;

            _indicePagina = novo;
            AplicarPagina();
        }

        private void AplicarPagina()
        {
            var pagina = _indicePagina >= 0 && _indicePagina < _aulasPorModulo.Count
                ? _aulasPorModulo[_indicePagina]
                : new List<AulaItem>();

            for (var i = 0; i < pagina.Count; i++)
                pagina[i].MostrarCabecalho = pagina[i].IniciaGrupo;

            AulasPagina = pagina;

            OnPropertyChanged(nameof(PaginaAtual));
            OnPropertyChanged(nameof(TotalPaginas));
            OnPropertyChanged(nameof(TemPaginacao));
            OnPropertyChanged(nameof(PodePaginaAnterior));
            OnPropertyChanged(nameof(PodePaginaProxima));
            OnPropertyChanged(nameof(IndicadorPagina));
            OnPropertyChanged(nameof(ModuloAtualTitulo));
        }

        private async Task AbrirAulaAsync(AulaItem? item)
        {
            if (item == null || MatriculaAtiva == null)
                return;

            if (!item.Liberada)
            {
                await Shell.Current.DisplayAlert(
                    "Aula bloqueada",
                    "Conclua a aula anterior para liberar esta aula.",
                    "OK");
                return;
            }

            var parametros = new Dictionary<string, object>
            {
                ["Aula"] = item.Aula,
                ["Matricula"] = MatriculaAtiva
            };
            await Shell.Current.GoToAsync("aula", parametros);
        }
    }
}
