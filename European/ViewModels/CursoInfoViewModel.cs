using System.Windows.Input;
using European.Enums;
using European.Models;

namespace European.ViewModels
{
    public class ModuloResumo
    {
        public string Numero { get; init; } = string.Empty;
        public string Titulo { get; init; } = string.Empty;
        public string Detalhe { get; init; } = string.Empty;
    }

    /// <summary>Detalhes do curso para quem está se cadastrando: só o essencial para decidir a compra.</summary>
    public class CursoInfoViewModel : BaseViewModel
    {
        private Curso? _curso;

        public CursoInfoViewModel()
        {
            VoltarCommand = new Command(async () => await VoltarAsync());
            EscolherCommand = new Command(async () => await EscolherAsync());
        }

        public Curso? Curso
        {
            get => _curso;
            private set
            {
                if (SetProperty(ref _curso, value))
                {
                    OnPropertyChanged(nameof(Nome));
                    OnPropertyChanged(nameof(Descricao));
                    OnPropertyChanged(nameof(CategoriaTexto));
                    OnPropertyChanged(nameof(PrecoTexto));
                    OnPropertyChanged(nameof(CargaTexto));
                    OnPropertyChanged(nameof(ModulosTexto));
                    OnPropertyChanged(nameof(AulasTexto));
                    OnPropertyChanged(nameof(Modulos));
                    OnPropertyChanged(nameof(TemModulos));
                    OnPropertyChanged(nameof(TemCapa));
                }
            }
        }

        public string Nome => Curso?.Nome ?? string.Empty;
        public string Descricao => Curso?.Descricao ?? string.Empty;
        public bool TemCapa => !string.IsNullOrWhiteSpace(Curso?.CapaUrl);

        public string CategoriaTexto => Curso == null ? string.Empty : NomeCategoria(Curso.Categoria);

        public static string NomeCategoria(CategoriaEnum categoria) => categoria switch
        {
            CategoriaEnum.Educacao => "Educação",
            CategoriaEnum.Engenharia => "Engenharia",
            CategoriaEnum.Gestao => "Gestão",
            CategoriaEnum.Direito => "Direito",
            CategoriaEnum.Saude => "Saúde",
            _ => categoria.ToString()
        };

        public string PrecoTexto => Curso == null || Curso.Preco <= 0 ? "Gratuito" : Curso.Preco.ToString("C", PagamentoViewModel.PtBr);
        public string CargaTexto => Curso?.CargaHorariaHoras > 0 ? $"{Curso.CargaHorariaHoras}h" : "—";
        public string ModulosTexto => (Curso?.Modulos.Count ?? 0).ToString();
        public string AulasTexto => (Curso?.TotalAulas ?? 0).ToString();

        public List<ModuloResumo> Modulos => Curso?.Modulos
            .OrderBy(m => m.Ordem)
            .Select((m, i) => new ModuloResumo
            {
                Numero = (i + 1).ToString(),
                Titulo = m.Titulo,
                Detalhe = m.Materias.Count == 1 ? "1 matéria" : $"{m.Materias.Count} matérias"
            })
            .ToList() ?? new List<ModuloResumo>();

        public bool TemModulos => Modulos.Count > 0;

        public ICommand VoltarCommand { get; }
        public ICommand EscolherCommand { get; }

        public void Carregar(Curso curso)
        {
            Curso = curso;
            Title = curso.Nome;
        }

        /// <summary>Volta ao cadastro já com este curso marcado.</summary>
        private async Task EscolherAsync()
        {
            if (Curso == null)
                return;

            await Shell.Current.GoToAsync("..", new Dictionary<string, object> { ["CursoEscolhidoId"] = Curso.Id });
        }
    }
}
