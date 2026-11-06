using European.ViewModels;

namespace European.Views
{
    public partial class PagamentoPage : BasePage
    {
        public PagamentoPage()
        {
            InitializeComponent();
            BindingContext = new PagamentoViewModel();
        }
    }
}
