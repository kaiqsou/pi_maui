using System.Windows.Input;
using European.MockData;
using European.Models;

namespace European.ViewModels
{
    public class ModuloEditViewModel : BaseViewModel
    {
        private Curso? _curso;
        private Modulo? _modulo;
        private string _titulo = string.Empty;
        private string _descricao = string.Empty;

        public ModuloEditViewModel()
        {
            SalvarCommand = new Command(async () => await SalvarAsync());
        }

        public string Titulo { get => _titulo; set => SetProperty(ref _titulo, value); }
        public string Descricao { get => _descricao; set => SetProperty(ref _descricao, value); }
        public bool IsEdicao => _modulo != null;

        public ICommand SalvarCommand { get; }

        public void Carregar(Curso? curso, Modulo? modulo)
        {
            _curso = curso;
            _modulo = modulo;
            Title = modulo == null ? "Novo módulo" : "Editar módulo";
            OnPropertyChanged(nameof(IsEdicao));

            Titulo = modulo?.Titulo ?? string.Empty;
            Descricao = modulo?.Descricao ?? string.Empty;
        }

        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Titulo))
            {
                await Shell.Current.DisplayAlert("Módulo", "Informe o título.", "OK");
                return;
            }

            if (_modulo == null && _curso != null)
            {
                MockDataService.AddModulo(_curso, new Modulo { Titulo = Titulo, Descricao = Descricao });
            }
            else if (_modulo != null)
            {
                _modulo.Titulo = Titulo;
                _modulo.Descricao = Descricao;
                MockDataService.UpdateModulo(_modulo);
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}
