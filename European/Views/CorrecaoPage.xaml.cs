using European.ViewModels;

namespace European.Views
{
    public partial class CorrecaoPage : BasePage
    {
        public CorrecaoPage()
        {
            InitializeComponent();
            BindingContext = new CorrecaoViewModel();
        }
    }
}
