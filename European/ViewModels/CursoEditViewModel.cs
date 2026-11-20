using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Enums;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public partial class CursoEditViewModel : BaseViewModel
    {
        private Curso? _curso;

        [ObservableProperty]
        public partial string Nome { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Descricao { get; set; } = string.Empty;

        [ObservableProperty]
        public partial CategoriaEnum Categoria { get; set; } = CategoriaEnum.Educacao;

        [ObservableProperty]
        public partial string CargaHoraria { get; set; } = "0";

        [ObservableProperty]
        public partial bool IsPublicado { get; set; } = true;

        public List<CategoriaEnum> Categorias { get; } = Enum.GetValues<CategoriaEnum>().ToList();

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            var curso = parametros.Obter<Curso>("Curso");
            _curso = curso;
            Title = curso == null ? "Novo curso" : "Editar curso";

            Nome = curso?.Nome ?? string.Empty;
            Descricao = curso?.Descricao ?? string.Empty;
            Categoria = curso?.Categoria ?? CategoriaEnum.Educacao;
            CargaHoraria = (curso?.CargaHorariaHoras ?? 0).ToString();
            IsPublicado = curso?.IsPublicado ?? true;
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Nome))
            {
                await AvisarAsync("Curso", "Informe o nome do curso.");
                return;
            }

            var carga = int.TryParse(CargaHoraria, out var c) ? Math.Max(0, c) : 0;

            void Aplicar(Curso curso)
            {
                curso.Nome = Nome.Trim();
                curso.Descricao = Descricao.Trim();
                curso.Categoria = Categoria;
                curso.CargaHorariaHoras = carga;
                curso.IsPublicado = IsPublicado;
            }

            await SalvarEVoltarAsync(async () =>
            {
                if (_curso != null)
                {
                    await CursoRepositorio.AlterarAsync(_curso.Id, Aplicar);
                    return;
                }

                var novo = new Curso();
                Aplicar(novo);
                await CursoRepositorio.SalvarAsync(novo);
            });
        }
    }
}
