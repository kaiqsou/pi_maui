using System.Globalization;
using System.Windows.Input;
using European.MockData;
using European.Models;

namespace European.ViewModels
{
    public class ProvaEditViewModel : BaseViewModel
    {
        private Materia? _materia;
        private Prova? _prova;
        private string _titulo = string.Empty;
        private string _notaMaxima = "10";
        private string _peso = "1";
        private string _notaCorte = "6";
        private string _duracaoMinutos = "60";
        private DateTime _prazo = DateTime.Now.AddDays(7);

        public ProvaEditViewModel()
        {
            SalvarCommand = new Command(async () => await SalvarAsync());
        }

        public string Titulo { get => _titulo; set => SetProperty(ref _titulo, value); }
        public string NotaMaxima { get => _notaMaxima; set => SetProperty(ref _notaMaxima, value); }
        public string Peso { get => _peso; set => SetProperty(ref _peso, value); }
        public string NotaCorte { get => _notaCorte; set => SetProperty(ref _notaCorte, value); }
        public string DuracaoMinutos { get => _duracaoMinutos; set => SetProperty(ref _duracaoMinutos, value); }
        public DateTime Prazo { get => _prazo; set => SetProperty(ref _prazo, value); }
        public bool IsEdicao => _prova != null;

        public ICommand SalvarCommand { get; }

        public void Carregar(Materia? materia, Prova? prova)
        {
            _materia = materia;
            _prova = prova;
            Title = prova == null ? "Nova prova" : "Editar prova";
            OnPropertyChanged(nameof(IsEdicao));

            Titulo = prova?.Titulo ?? string.Empty;
            NotaMaxima = (prova?.NotaMaxima ?? 10).ToString(CultureInfo.InvariantCulture);
            Peso = (prova?.Peso ?? 1).ToString(CultureInfo.InvariantCulture);
            NotaCorte = (prova?.NotaCorte ?? 6).ToString(CultureInfo.InvariantCulture);
            DuracaoMinutos = (prova?.DuracaoMinutos ?? 60).ToString();
            Prazo = prova?.Prazo ?? DateTime.Now.AddDays(7);
        }

        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Titulo))
            {
                await Shell.Current.DisplayAlert("Prova", "Informe o título.", "OK");
                return;
            }

            var notaMaxima = double.TryParse(NotaMaxima, NumberStyles.Any, CultureInfo.InvariantCulture, out var nm) ? nm : 10;
            var peso = double.TryParse(Peso, NumberStyles.Any, CultureInfo.InvariantCulture, out var pe) ? pe : 1;
            var corte = double.TryParse(NotaCorte, NumberStyles.Any, CultureInfo.InvariantCulture, out var nc) ? nc : 6;
            var duracao = int.TryParse(DuracaoMinutos, out var dm) ? dm : 0;

            if (_prova == null && _materia != null)
            {
                MockDataService.AddProva(new Prova
                {
                    MateriaId = _materia.Id,
                    Materia = _materia,
                    Titulo = Titulo,
                    NotaMaxima = notaMaxima,
                    Peso = peso,
                    NotaCorte = corte,
                    DuracaoMinutos = duracao,
                    Prazo = Prazo
                });
            }
            else if (_prova != null)
            {
                _prova.Titulo = Titulo;
                _prova.NotaMaxima = notaMaxima;
                _prova.Peso = peso;
                _prova.NotaCorte = corte;
                _prova.DuracaoMinutos = duracao;
                _prova.Prazo = Prazo;
                MockDataService.UpdateProva(_prova);
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}
