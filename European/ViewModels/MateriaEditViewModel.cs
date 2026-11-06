using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public partial class MateriaEditViewModel : BaseViewModel
    {
        private Modulo? _modulo;
        private Materia? _materia;

        [ObservableProperty]
        public partial string Nome { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Descricao { get; set; } = string.Empty;

        public string ModuloTitulo => _modulo?.Titulo ?? string.Empty;

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            _materia = parametros.Obter<Materia>("Materia");
            _modulo = parametros.Obter<Modulo>("Modulo") ?? _materia?.Modulo;
            Title = _materia == null ? "Nova matéria" : "Editar matéria";
            OnPropertyChanged(nameof(ModuloTitulo));

            Nome = _materia?.Nome ?? string.Empty;
            Descricao = _materia?.Descricao ?? string.Empty;
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task SalvarAsync()
        {
            var cursoId = _modulo?.Curso?.Id;
            if (string.IsNullOrWhiteSpace(Nome) || _modulo == null || cursoId == null)
            {
                await AvisarAsync("Matéria", "Informe o nome.");
                return;
            }

            await SalvarEVoltarAsync(() => CursoRepositorio.SalvarMateriaAsync(cursoId, _modulo.Id, _materia?.Id, materia =>
            {
                materia.Nome = Nome.Trim();
                materia.Descricao = Descricao.Trim();
            }));
        }
    }
}
