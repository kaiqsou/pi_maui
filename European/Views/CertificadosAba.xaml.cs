using European.ViewModels;

namespace European.Views
{
    /// <summary>Aba da área do aluno (MainPage). Os dados são recarregados pela MainPage via AoAparecerAsync.</summary>
    public partial class CertificadosAba : ContentView
    {
        public CertificadosAba()
        {
            InitializeComponent();
            BindingContext = new CertificadoViewModel();
        }
    }
}
