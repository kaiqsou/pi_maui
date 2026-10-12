using System.Collections.ObjectModel;
using European.MockData;
using European.Models;

namespace European.ViewModels
{
    public class NotasViewModel : BaseViewModel
    {
        private const double NotaCorte = 6;

        public NotasViewModel()
        {
            Title = "Notas";
        }

        public ObservableCollection<ResultadoProva> Resultados { get; } = new();

        public double Media =>
            Resultados.Count == 0 ? 0 : Resultados.Average(r => r.NotaObtida);

        public bool Aprovado => Media >= NotaCorte;

        public void Carregar(Usuario aluno)
        {
            Resultados.Clear();
            foreach (var resultado in MockDataService.Resultados().Where(r => r.AlunoId == aluno.Id))
                Resultados.Add(resultado);

            OnPropertyChanged(nameof(Media));
            OnPropertyChanged(nameof(Aprovado));
        }
    }
}
