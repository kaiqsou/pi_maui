using European.ViewModels;

namespace European.Views
{
    /// <summary>Aba da área do aluno (MainPage). Os dados são recarregados pela MainPage via AoAparecerAsync.</summary>
    public partial class ProvasAba : ContentView
    {
        public ProvasAba()
        {
            InitializeComponent();
            BindingContext = new ProvasViewModel();
        }
    }
}
