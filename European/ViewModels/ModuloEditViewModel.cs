using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using European.Data;
using European.Models;
using European.Services;

namespace European.ViewModels
{
    public partial class ModuloEditViewModel : BaseViewModel
    {
        private Curso? _curso;
        private Modulo? _modulo;

        [ObservableProperty]
        public partial string Titulo { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Descricao { get; set; } = string.Empty;

        public string CursoNome => _curso?.Nome ?? string.Empty;

        protected override Task ReceberParametrosAsync(IDictionary<string, object> parametros)
        {
            _modulo = parametros.Obter<Modulo>("Modulo");
            _curso = parametros.Obter<Curso>("Curso") ?? _modulo?.Curso;
            Title = _modulo == null ? "Novo módulo" : "Editar módulo";
            OnPropertyChanged(nameof(CursoNome));

            Titulo = _modulo?.Titulo ?? $"Módulo {(_curso?.Modulos.Count ?? 0) + 1} — ";
            Descricao = _modulo?.Descricao ?? string.Empty;
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task SalvarAsync()
        {
            if (string.IsNullOrWhiteSpace(Titulo) || _curso == null)
            {
                await AvisarAsync("Módulo", "Informe o título.");
                return;
            }

            await SalvarEVoltarAsync(() => CursoRepositorio.SalvarModuloAsync(_curso.Id, _modulo?.Id, modulo =>
            {
                modulo.Titulo = Titulo.Trim();
                modulo.Descricao = Descricao.Trim();
            }));
        }
    }
}
