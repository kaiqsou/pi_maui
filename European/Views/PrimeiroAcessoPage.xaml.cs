using European.ViewModels;

namespace European.Views
{
    public partial class PrimeiroAcessoPage : BasePage
    {
        public PrimeiroAcessoPage()
        {
            InitializeComponent();
            BindingContext = new PrimeiroAcessoViewModel();
        }
    }
}
