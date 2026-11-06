using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace European.ViewModels
{
    /// <summary>Área do aluno: guarda qual aba está aberta ("inicio", "aulas", "provas", "desempenho" ou "certificados").</summary>
    public partial class MainViewModel : BaseViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MostrarInicio), nameof(MostrarAulas), nameof(MostrarProvas),
            nameof(MostrarDesempenho), nameof(MostrarCertificados))]
        public partial string AbaAtual { get; private set; } = "inicio";

        public bool MostrarInicio => AbaAtual == "inicio";
        public bool MostrarAulas => AbaAtual == "aulas";
        public bool MostrarProvas => AbaAtual == "provas";
        public bool MostrarDesempenho => AbaAtual == "desempenho";
        public bool MostrarCertificados => AbaAtual == "certificados";

        /// <summary>A MainPage troca o conteúdo exibido quando a aba muda.</summary>
        public event Action? AbaAlterada;

        partial void OnAbaAtualChanged(string value) => AbaAlterada?.Invoke();

        [RelayCommand]
        public void SelecionarAba(string aba)
        {
            if (!string.IsNullOrWhiteSpace(aba))
                AbaAtual = aba;
        }
    }
}
