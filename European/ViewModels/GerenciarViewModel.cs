using System.Collections.ObjectModel;
using System.Windows.Input;
using European.MockData;
using European.Models;

namespace European.ViewModels
{
    public class GerenciarViewModel : BaseViewModel
    {
        public GerenciarViewModel()
        {
            Title = "Gerenciar";
            VoltarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            NovoCommand = new Command(async () => await Shell.Current.GoToAsync("cursoedit"));
            EditarCommand = new Command<Curso>(async c => await Shell.Current.GoToAsync("cursoedit", new Dictionary<string, object> { ["Curso"] = c }));
            AbrirCommand = new Command<Curso>(async c => await Shell.Current.GoToAsync("curso", new Dictionary<string, object> { ["Curso"] = c }));
            ExcluirCommand = new Command<Curso>(async c => await ExcluirAsync(c));
        }

        public ObservableCollection<Curso> Cursos { get; } = new();

        public ICommand VoltarCommand { get; }
        public ICommand NovoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand AbrirCommand { get; }
        public ICommand ExcluirCommand { get; }

        public void Carregar()
        {
            Cursos.Clear();
            foreach (var curso in MockDataService.Cursos())
                Cursos.Add(curso);
        }

        private async Task ExcluirAsync(Curso? curso)
        {
            if (curso == null)
                return;

            var confirmar = await Shell.Current.DisplayAlert("Confirmar", $"Excluir o curso \"{curso.Nome}\"?", "Excluir", "Cancelar");
            if (!confirmar)
                return;

            MockDataService.RemoveCurso(curso);
            Carregar();
        }
    }
}
