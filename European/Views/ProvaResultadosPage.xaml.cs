using European.ViewModels;

namespace European.Views
{
    public partial class ProvaResultadosPage : BasePage
    {
        public ProvaResultadosPage()
        {
            InitializeComponent();
            BindingContext = new ProvaResultadosViewModel();
        }
    }
}
