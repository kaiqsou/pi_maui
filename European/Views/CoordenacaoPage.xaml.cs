using European.ViewModels;

namespace European.Views
{
    public partial class CoordenacaoPage : BasePage
    {
        public CoordenacaoPage()
        {
            InitializeComponent();
            BindingContext = new CoordenacaoViewModel();
        }
    }
}
