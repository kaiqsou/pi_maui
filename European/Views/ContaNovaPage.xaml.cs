using European.ViewModels;

namespace European.Views
{
    public partial class ContaNovaPage : BasePage
    {
        public ContaNovaPage()
        {
            InitializeComponent();
            BindingContext = new ContaNovaViewModel();
        }
    }
}
