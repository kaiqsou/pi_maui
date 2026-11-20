using European.ViewModels;

namespace European.Views
{
    public partial class AguardandoAcessoPage : BasePage
    {
        public AguardandoAcessoPage()
        {
            InitializeComponent();
            BindingContext = new AcessoRestritoViewModel();
        }
    }
}
