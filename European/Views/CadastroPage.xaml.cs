using European.ViewModels;

namespace European.Views
{
    public partial class CadastroPage : BasePage
    {
        public CadastroPage()
        {
            InitializeComponent();
            BindingContext = new CadastroViewModel();
        }
    }
}
