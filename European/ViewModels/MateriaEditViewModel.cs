using System.Windows.Input;
using European.MockData;
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
        public bool IsEdicao => _materia != null;

        public ICommand SalvarCommand { get; }

        public void Carregar(Modulo? modulo, Materia? materia)
        {
            _modulo = modulo;
            _materia = materia;
            Title = materia == null ? "Nova matéria" : "Editar matéria";
            OnPropertyChanged(nameof(IsEdicao));

            Nome = materia?.Nome ?? string.Empty;
            Descricao = materia?.Descricao ?? string.Empty;
        }

        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Nome))
            {
                await Shell.Current.DisplayAlert("Matéria", "Informe o nome.", "OK");
                return;
            }

            if (_materia == null && _modulo != null)
            {
                MockDataService.AddMateria(_modulo, new Materia { Nome = Nome, Descricao = Descricao });
            }
            else if (_materia != null)
            {
                _materia.Nome = Nome;
                _materia.Descricao = Descricao;
                MockDataService.UpdateMateria(_materia);
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}
