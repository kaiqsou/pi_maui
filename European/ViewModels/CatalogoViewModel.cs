using System.Collections.ObjectModel;
using System.Windows.Input;
using European.MockData;
using European.Models;

namespace European.ViewModels
{
    public class CatalogoViewModel : BaseViewModel
    {
        public CatalogoViewModel()
        {
            Title = "Catálogo de Cursos";
            AbrirCommand = new Command<Curso>(async c => await AbrirAsync(c));
            VoltarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public ObservableCollection<Curso> Cursos { get; } = new();

        public ICommand AbrirCommand { get; }
        public ICommand VoltarCommand { get; }

        public void Carregar()
        {
            Cursos.Clear();
            foreach (var curso in MockDataService.Cursos().Where(c => c.IsPublicado))
                Cursos.Add(curso);
        }

        private static async Task AbrirAsync(Curso curso)
        {
            var parametros = new Dictionary<string, object>
            {
                ["Curso"] = curso
            };
            await Shell.Current.GoToAsync("curso", parametros);
        }
    }
}
