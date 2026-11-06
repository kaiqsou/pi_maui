using System.Windows.Input;
using European.Data;
using European.Models;

namespace European.ViewModels
{
    public class MateriaEditViewModel : BaseViewModel
    {
        private Modulo? _modulo;
        private Materia? _materia;
        private string _nome = string.Empty;
        private string _descricao = string.Empty;

        public MateriaEditViewModel()
        {
            SalvarCommand = new Command(async () => await SalvarAsync());
        }

        public string Nome { get => _nome; set => SetProperty(ref _nome, value); }
        public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
        public string ModuloTitulo => _modulo?.Titulo ?? string.Empty;

        public ICommand SalvarCommand { get; }

        public void Carregar(Modulo? modulo, Materia? materia)
        {
            _modulo = modulo ?? materia?.Modulo;
            _materia = materia;
            Title = materia == null ? "Nova matéria" : "Editar matéria";
            OnPropertyChanged(nameof(ModuloTitulo));

            Nome = materia?.Nome ?? string.Empty;
            Descricao = materia?.Descricao ?? string.Empty;
        }

        private async Task SalvarAsync()
        {
            var cursoId = _modulo?.Curso?.Id;
            if (string.IsNullOrWhiteSpace(Nome) || _modulo == null || cursoId == null)
            {
                await AvisarAsync("Matéria", "Informe o nome.");
                return;
            }

            var salvo = await ExecutarAsync(() => CursoRepositorio.SalvarMateriaAsync(cursoId, _modulo.Id, _materia?.Id, materia =>
            {
                materia.Nome = Nome.Trim();
                materia.Descricao = Descricao.Trim();
            }));

            if (salvo)
                await VoltarAsync();
        }
    }
}
