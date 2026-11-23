using European.ViewModels;

namespace European.Views
{
    public partial class ProvaRevisaoPage : BasePage
    {
        public ProvaRevisaoPage()
        {
            InitializeComponent();
            BindingContext = new ProvaRevisaoViewModel();
        }
    }
}
