using System.Globalization;
using System.Windows.Input;
using European.Enums;
using European.MockData;
using European.Models;

namespace European.ViewModels
{
    public class CursoEditViewModel : BaseViewModel
    {
        private Curso? _curso;
        private string _nome = string.Empty;
        private string _descricao = string.Empty;
        private CategoriaEnum _categoria = CategoriaEnum.Educacao;
        private string _preco = "0";
        private string _cargaHoraria = "0";
        private bool _isPublicado = true;

        public CursoEditViewModel()
        {
            SalvarCommand = new Command(async () => await SalvarAsync());
        }

        public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
        public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
        public CategoriaEnum Categoria { get => _categoria; set => SetProperty(ref _categoria, value); }
        public string Preco { get => _preco; set => SetProperty(ref _preco, value); }
        public string CargaHoraria { get => _cargaHoraria; set => SetProperty(ref _cargaHoraria, value); }
        public bool IsPublicado { get => _isPublicado; set => SetProperty(ref _isPublicado, value); }

        public List<CategoriaEnum> Categorias { get; } = Enum.GetValues<CategoriaEnum>().ToList();

        public bool IsEdicao => _curso != null;

        public ICommand SalvarCommand { get; }

        public void Carregar(Curso? curso)
        {
            _curso = curso;
            OnPropertyChanged(nameof(IsEdicao));

            if (curso == null)
            {
                Title = "Novo curso";
                Nome = string.Empty;
                Descricao = string.Empty;
                Categoria = CategoriaEnum.Educacao;
                Preco = "0";
                CargaHoraria = "0";
                IsPublicado = true;
                return;
            }

            Title = "Editar curso";
            Nome = curso.Nome;
            Descricao = curso.Descricao;
            Categoria = curso.Categoria;
            Preco = curso.Preco.ToString(CultureInfo.InvariantCulture);
            CargaHoraria = curso.CargaHorariaHoras.ToString();
            IsPublicado = curso.IsPublicado;
        }

        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Nome))
            {
                await Shell.Current.DisplayAlert("Curso", "Informe o nome do curso.", "OK");
                return;
            }

            var preco = decimal.TryParse(Preco, NumberStyles.Any, CultureInfo.InvariantCulture, out var p) ? p : 0m;
            var carga = int.TryParse(CargaHoraria, out var c) ? c : 0;

            if (_curso == null)
            {
                MockDataService.AddCurso(new Curso
                {
                    Nome = Nome,
                    Descricao = Descricao,
                    Categoria = Categoria,
                    Preco = preco,
                    CargaHorariaHoras = carga,
                    IsPublicado = IsPublicado
                });
            }
            else
            {
                _curso.Nome = Nome;
                _curso.Descricao = Descricao;
                _curso.Categoria = Categoria;
                _curso.Preco = preco;
                _curso.CargaHorariaHoras = carga;
                _curso.IsPublicado = IsPublicado;
                MockDataService.UpdateCurso(_curso);
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}
