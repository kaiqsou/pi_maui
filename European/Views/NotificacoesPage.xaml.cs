using European.ViewModels;

namespace European.Views
{
    public partial class NotificacoesPage : BasePage
    {
        public NotificacoesPage()
        {
            InitializeComponent();
            BindingContext = new NotificacoesViewModel();
        }
    }
}
