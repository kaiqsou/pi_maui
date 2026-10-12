using System.Windows.Input;

namespace European.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _aba = "inicio";

        public static MainViewModel? Atual { get; set; }

        public event Action? AbaAlterada;

        public MainViewModel()
        {
            SelecionarAbaCommand = new Command<string>(SelecionarAba);
        }

        public ICommand SelecionarAbaCommand { get; }

        public string AbaAtual => _aba;

        public void SelecionarAba(string aba)
        {
            if (string.IsNullOrWhiteSpace(aba) || aba == _aba)
                return;

            _aba = aba;
            OnPropertyChanged(nameof(MostrarInicio));
            OnPropertyChanged(nameof(MostrarAulas));
            OnPropertyChanged(nameof(MostrarProvas));
            OnPropertyChanged(nameof(MostrarDesempenho));
            OnPropertyChanged(nameof(MostrarCertificados));
            AbaAlterada?.Invoke();
        }

        public bool MostrarInicio => _aba == "inicio";
        public bool MostrarAulas => _aba == "aulas";
        public bool MostrarProvas => _aba == "provas";
        public bool MostrarDesempenho => _aba == "desempenho";
        public bool MostrarCertificados => _aba == "certificados";
    }
}