using European.ViewModels;

namespace European.Views
{
    public partial class AulaEditPage : BasePage
    {
        public AulaEditPage()
        {
            InitializeComponent();
            BindingContext = new AulaEditViewModel();
        }
    }
}
