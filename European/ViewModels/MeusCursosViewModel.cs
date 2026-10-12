using System.Collections.ObjectModel;
using European.MockData;
using European.Models;

namespace European.ViewModels
{
    public class MeusCursosViewModel : BaseViewModel
    {
        public MeusCursosViewModel()
        {
            Title = "Meus Cursos";
        }

        public ObservableCollection<Matricula> Matriculas { get; } = new();

        public double ProgressoMedio =>
            Matriculas.Count == 0 ? 0 : Matriculas.Average(m => m.ProgressoPercentual);

        public void Carregar(Usuario aluno)
        {
            Matriculas.Clear();
            foreach (var matricula in MockDataService.Matriculas().Where(m => m.AlunoId == aluno.Id))
                Matriculas.Add(matricula);

            OnPropertyChanged(nameof(ProgressoMedio));
        }
    }
}
