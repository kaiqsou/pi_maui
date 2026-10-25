using System.Windows.Input;
using European.Data;
using European.Enums;
using European.Models;

namespace European.ViewModels
{
    public class CursoEditViewModel : BaseViewModel
    {
        private Curso? _curso;
        private string _nome = string.Empty;
        private string _descricao = string.Empty;
        private CategoriaEnum _categoria = CategoriaEnum.Educacao;
        private string _cargaHoraria = "0";
        private bool _isPublicado = true;

        public CursoEditViewModel()
        {
            SalvarCommand = new Command(async () => await SalvarAsync());
        }

        public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
        public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
        public CategoriaEnum Categoria { get => _categoria; set => SetProperty(ref _categoria, value); }
        public string CargaHoraria { get => _cargaHoraria; set => SetProperty(ref _cargaHoraria, value); }
        public bool IsPublicado { get => _isPublicado; set => SetProperty(ref _isPublicado, value); }

        public List<CategoriaEnum> Categorias { get; } = Enum.GetValues<CategoriaEnum>().ToList();

        public ICommand SalvarCommand { get; }

        public void Carregar(Curso? curso)
        {
            _curso = curso;
            Title = curso == null ? "Novo curso" : "Editar curso";

            Nome = curso?.Nome ?? string.Empty;
            Descricao = curso?.Descricao ?? string.Empty;
            Categoria = curso?.Categoria ?? CategoriaEnum.Educacao;
            CargaHoraria = (curso?.CargaHorariaHoras ?? 0).ToString();
            IsPublicado = curso?.IsPublicado ?? true;
        }

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

            var salvo = await ExecutarAsync(async () =>
            {
                if (_curso == null)
                {
                    var novo = new Curso();
                    Aplicar(novo);
                    await CursoRepositorio.SalvarAsync(novo);
                }
                else
                {
                    await CursoRepositorio.AlterarAsync(_curso.Id, Aplicar);
                }
            });

            if (salvo)
                await VoltarAsync();
        }
    }
}
