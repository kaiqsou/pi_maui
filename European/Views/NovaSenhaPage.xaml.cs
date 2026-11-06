using European.ViewModels;

namespace European.Views
{
    public partial class NovaSenhaPage : BasePage
    {
        public NovaSenhaPage()
        {
            InitializeComponent();
            BindingContext = new NovaSenhaViewModel();
        }
    }
}
