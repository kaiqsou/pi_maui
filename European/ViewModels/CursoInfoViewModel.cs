using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public class ModuloResumo
    {
        public string Numero { get; init; } = string.Empty;
        public string Titulo { get; init; } = string.Empty;
        public string Detalhe { get; init; } = string.Empty;
    }

    /// <summary>Detalhes do curso para quem está se cadastrando: só o essencial para decidir a compra.</summary>
    public partial class CursoInfoViewModel : BaseViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Nome), nameof(Descricao), nameof(CategoriaTexto), nameof(PrecoTexto),
            nameof(CargaTexto), nameof(ModulosTexto), nameof(AulasTexto), nameof(Modulos), nameof(TemModulos), nameof(TemCapa))]
        public partial Curso? Curso { get; private set; }

        public string Nome => Curso?.Nome ?? string.Empty;
        public string Descricao => Curso?.Descricao ?? string.Empty;
        public bool TemCapa => !string.IsNullOrWhiteSpace(Curso?.CapaUrl);
        public string CategoriaTexto => Curso == null ? string.Empty : Formatos.Categoria(Curso.Categoria);
        public string PrecoTexto => Formatos.Preco(Curso?.Preco ?? 0);
        public string CargaTexto => Curso?.CargaHorariaHoras > 0 ? $"{Curso.CargaHorariaHoras.ToString("N0", Formatos.PtBr)}h" : "—";
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

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            if (parametros.Obter<Curso>("Curso") is { } curso)
            {
                Curso = curso;
                Title = curso.Nome;
            }
            return Task.CompletedTask;
        }


        /// <summary>Volta ao cadastro já com este curso marcado.</summary>
        [RelayCommand]
        private Task Escolher()
            => Curso == null ? Task.CompletedTask : Navegacao.VoltarAsync(("CursoEscolhidoId", Curso.Id));
    }
}
