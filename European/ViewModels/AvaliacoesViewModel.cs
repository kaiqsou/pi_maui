using System.Collections.ObjectModel;
using European.MockData;
using European.Models;

namespace European.ViewModels
{
    public class AvaliacoesViewModel : BaseViewModel
    {
        public AvaliacoesViewModel()
        {
            Title = "Avaliacoes";
        }

        public ObservableCollection<Prova> Provas { get; } = new();

        public void Carregar(Usuario aluno)
        {
            var materiasDoAluno = MockDataService.Matriculas()
                .Where(m => m.AlunoId == aluno.Id)
                .SelectMany(m => m.Curso?.Modulos.SelectMany(x => x.Materias) ?? Enumerable.Empty<Materia>())
                .Select(materia => materia.Id)
                .ToHashSet();

            Provas.Clear();
            foreach (var prova in MockDataService.Provas().Where(p => materiasDoAluno.Contains(p.MateriaId)))
                Provas.Add(prova);
        }
    }
}
