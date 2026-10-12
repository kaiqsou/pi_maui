using System.Collections.ObjectModel;
using System.Windows.Input;
using European.MockData;
using European.Models;

namespace European.ViewModels
{
    public class CursosViewModel : BaseViewModel
    {
        private string _busca = string.Empty;
        private Curso? _cursoSelecionado;

        public CursosViewModel()
        {
            Title = "Catalogo de Cursos";
            Carregar();
            SelecionarCommand = new Command<Curso>(Selecionar);
        }

        public ObservableCollection<Curso> Cursos { get; } = new();

        public string Busca
        {
            get => _busca;
            set
            {
                if (SetProperty(ref _busca, value))
                    Carregar();
            }
        }

        public Curso? CursoSelecionado
        {
            get => _cursoSelecionado;
            set => SetProperty(ref _cursoSelecionado, value);
        }

        public ICommand SelecionarCommand { get; }

        public void Carregar()
        {
            var filtrados = MockDataService.Cursos()
                .Where(c => string.IsNullOrWhiteSpace(Busca) || c.Nome.Contains(Busca, StringComparison.OrdinalIgnoreCase));

            Cursos.Clear();
            foreach (var curso in filtrados)
                Cursos.Add(curso);
        }

        private void Selecionar(Curso curso)
        {
            CursoSelecionado = curso;
            Title = curso.Nome;
        }
    }
}
