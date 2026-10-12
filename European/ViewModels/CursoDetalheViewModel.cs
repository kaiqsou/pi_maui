using System.Windows.Input;
using European.Enums;
using European.MockData;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class MateriaNode
    {
        public Materia Materia { get; set; } = new();
        public List<Aula> Aulas { get; set; } = new();
        public List<Prova> Provas { get; set; } = new();
    }

    public class ModuloNode
    {
        public Modulo Modulo { get; set; } = new();
        public List<MateriaNode> Materias { get; set; } = new();
    }

    public class CursoDetalheViewModel : BaseViewModel
    {
        private const int ModulosPorPagina = 2;

        private Curso? _curso;
        private Usuario? _aluno;
        private bool _isMatriculado;
        private List<ModuloNode> _todosModulos = new();
        private int _indicePagina;
        private IReadOnlyList<ModuloNode> _modulosPagina = new List<ModuloNode>();

        public CursoDetalheViewModel()
        {
            MatricularCommand = new Command(async () => await MatricularAsync());
            VoltarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            NovoModuloCommand = new Command(async () => await IrAsync("moduloedit", new() { ["Curso"] = Curso! }));
            EditarModuloCommand = new Command<Modulo>(async m => await IrAsync("moduloedit", new() { ["Curso"] = Curso!, ["Modulo"] = m }));
            ExcluirModuloCommand = new Command<Modulo>(async m => await ExcluirModuloAsync(m));
            NovaMateriaCommand = new Command<Modulo>(async m => await IrAsync("materiaedit", new() { ["Modulo"] = m }));
            EditarMateriaCommand = new Command<Materia>(async m => await IrAsync("materiaedit", new() { ["Modulo"] = m.Modulo!, ["Materia"] = m }));
            ExcluirMateriaCommand = new Command<Materia>(async m => await ExcluirMateriaAsync(m));
            NovaAulaCommand = new Command<Materia>(async m => await IrAsync("aulaedit", new() { ["Materia"] = m }));
            EditarAulaCommand = new Command<Aula>(async a => await IrAsync("aulaedit", new() { ["Materia"] = a.Materia!, ["Aula"] = a }));
            ExcluirAulaCommand = new Command<Aula>(async a => await ExcluirAulaAsync(a));
            NovaProvaCommand = new Command<Materia>(async m => await IrAsync("provaedit", new() { ["Materia"] = m }));
            EditarProvaCommand = new Command<Prova>(async p => await IrAsync("provaedit", new() { ["Materia"] = p.Materia!, ["Prova"] = p }));
            ExcluirProvaCommand = new Command<Prova>(async p => await ExcluirProvaAsync(p));
            PaginaAnteriorCommand = new Command(() => MudarPagina(-1));
            PaginaProximaCommand = new Command(() => MudarPagina(1));
        }

        public Curso? Curso
        {
            get => _curso;
            set
            {
                if (SetProperty(ref _curso, value))
                {
                    OnPropertyChanged(nameof(CargaHorariaFormatada));
                    OnPropertyChanged(nameof(TotalAulas));
                    OnPropertyChanged(nameof(PrecoFormatado));
                }
            }
        }

        public IReadOnlyList<ModuloNode> ModulosPagina
        {
            get => _modulosPagina;
            private set => SetProperty(ref _modulosPagina, value);
        }

        public int TotalPaginas => _todosModulos.Count == 0 ? 1 : (_todosModulos.Count + ModulosPorPagina - 1) / ModulosPorPagina;
        public int PaginaAtual => _indicePagina + 1;
        public bool TemPaginacao => TotalPaginas > 1;
        public bool PodePaginaAnterior => _indicePagina > 0;
        public bool PodePaginaProxima => _indicePagina < TotalPaginas - 1;
        public string IndicadorPagina => $"Página {PaginaAtual} de {TotalPaginas}";

        public int TotalAulas => Curso?.Modulos.Sum(m => m.Materias.Sum(x => x.Aulas.Count)) ?? 0;
        public string CargaHorariaFormatada => Curso == null ? string.Empty : $"{Curso.CargaHorariaHoras} horas";
        public string PrecoFormatado => Curso == null ? string.Empty : Curso.Preco.ToString("C");

        public bool IsMatriculado
        {
            get => _isMatriculado;
            set
            {
                if (SetProperty(ref _isMatriculado, value))
                    OnPropertyChanged(nameof(PodeMatricular));
            }
        }

        public bool PodeMatricular => !IsMatriculado;

        public bool PodeGerenciar => SessionService.UsuarioAtual is { Roles: Roles.Administrador or Roles.Professor or Roles.Coordenador };

        public ICommand MatricularCommand { get; }
        public ICommand VoltarCommand { get; }
        public ICommand NovoModuloCommand { get; }
        public ICommand EditarModuloCommand { get; }
        public ICommand ExcluirModuloCommand { get; }
        public ICommand NovaMateriaCommand { get; }
        public ICommand EditarMateriaCommand { get; }
        public ICommand ExcluirMateriaCommand { get; }
        public ICommand NovaAulaCommand { get; }
        public ICommand EditarAulaCommand { get; }
        public ICommand ExcluirAulaCommand { get; }
        public ICommand NovaProvaCommand { get; }
        public ICommand EditarProvaCommand { get; }
        public ICommand ExcluirProvaCommand { get; }
        public ICommand PaginaAnteriorCommand { get; }
        public ICommand PaginaProximaCommand { get; }

        public void Carregar(Curso curso)
        {
            _aluno = SessionService.UsuarioAtual;
            Curso = curso;
            Title = curso.Nome;

            RecarregarModulos();

            IsMatriculado = _aluno != null &&
                MockDataService.Matriculas().Any(m => m.CursoId == curso.Id && m.AlunoId == _aluno.Id);
        }

        private void RecarregarModulos()
        {
            _todosModulos = new List<ModuloNode>();

            if (Curso != null)
            {
                foreach (var modulo in Curso.Modulos.OrderBy(m => m.Ordem))
                {
                    var node = new ModuloNode { Modulo = modulo };

                    foreach (var materia in modulo.Materias.OrderBy(m => m.Ordem))
                    {
                        node.Materias.Add(new MateriaNode
                        {
                            Materia = materia,
                            Aulas = materia.Aulas.OrderBy(a => a.Ordem).ToList(),
                            Provas = MockDataService.Provas().Where(p => p.MateriaId == materia.Id).ToList()
                        });
                    }

                    _todosModulos.Add(node);
                }
            }

            _indicePagina = 0;
            AplicarPagina();
            OnPropertyChanged(nameof(TotalAulas));
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
            ModulosPagina = _todosModulos
                .Skip(_indicePagina * ModulosPorPagina)
                .Take(ModulosPorPagina)
                .ToList();

            OnPropertyChanged(nameof(PaginaAtual));
            OnPropertyChanged(nameof(TotalPaginas));
            OnPropertyChanged(nameof(TemPaginacao));
            OnPropertyChanged(nameof(PodePaginaAnterior));
            OnPropertyChanged(nameof(PodePaginaProxima));
            OnPropertyChanged(nameof(IndicadorPagina));
        }

        private async Task MatricularAsync()
        {
            if (Curso == null || IsMatriculado)
                return;

            var parametros = new Dictionary<string, object>
            {
                ["Curso"] = Curso
            };
            await Shell.Current.GoToAsync("pagamento", parametros);
        }

        private static Task IrAsync(string rota, Dictionary<string, object> parametros)
            => Shell.Current.GoToAsync(rota, parametros);

        private async Task ExcluirModuloAsync(Modulo? modulo)
        {
            if (modulo == null || Curso == null)
                return;

            if (!await ConfirmarAsync($"Excluir o módulo \"{modulo.Titulo}\"?"))
                return;

            MockDataService.RemoveModulo(modulo);
            RecarregarModulos();
        }

        private async Task ExcluirMateriaAsync(Materia? materia)
        {
            if (materia == null)
                return;

            if (!await ConfirmarAsync($"Excluir a matéria \"{materia.Nome}\"?"))
                return;

            MockDataService.RemoveMateria(materia);
            RecarregarModulos();
        }

        private async Task ExcluirAulaAsync(Aula? aula)
        {
            if (aula == null)
                return;

            if (!await ConfirmarAsync($"Excluir a aula \"{aula.Titulo}\"?"))
                return;

            MockDataService.RemoveAula(aula);
            RecarregarModulos();
        }

        private async Task ExcluirProvaAsync(Prova? prova)
        {
            if (prova == null)
                return;

            if (!await ConfirmarAsync($"Excluir a prova \"{prova.Titulo}\"?"))
                return;

            MockDataService.RemoveProva(prova);
            RecarregarModulos();
        }

        private static Task<bool> ConfirmarAsync(string mensagem)
            => Shell.Current.DisplayAlert("Confirmar", mensagem, "Excluir", "Cancelar");
    }
}
