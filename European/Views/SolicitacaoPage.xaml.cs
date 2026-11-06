using European.ViewModels;

namespace European.Views
{
    public partial class SolicitacaoPage : BasePage
    {
        public SolicitacaoPage()
        {
            InitializeComponent();
            BindingContext = new SolicitacaoViewModel();
        }
    }
}
