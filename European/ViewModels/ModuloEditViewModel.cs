using System.Windows.Input;
using European.Data;
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
        public string CursoNome => _curso?.Nome ?? _modulo?.Curso?.Nome ?? string.Empty;

        public ICommand SalvarCommand { get; }

        public void Carregar(Curso? curso, Modulo? modulo)
        {
            _curso = curso ?? modulo?.Curso;
            _modulo = modulo;
            Title = modulo == null ? "Novo módulo" : "Editar módulo";
            OnPropertyChanged(nameof(CursoNome));

            Titulo = modulo?.Titulo ?? $"Módulo {(_curso?.Modulos.Count ?? 0) + 1} — ";
            Descricao = modulo?.Descricao ?? string.Empty;
        }

        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Titulo) || _curso == null)
            {
                await AvisarAsync("Módulo", "Informe o título.");
                return;
            }

            var salvo = await ExecutarAsync(() => CursoRepositorio.SalvarModuloAsync(_curso.Id, _modulo?.Id, modulo =>
            {
                modulo.Titulo = Titulo.Trim();
                modulo.Descricao = Descricao.Trim();
            }));

            if (salvo)
                await VoltarAsync();
        }
    }
}
