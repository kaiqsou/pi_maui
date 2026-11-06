using European.ViewModels;

namespace European.Views
{
    public partial class ProvaEditPage : BasePage
    {
        public ProvaEditPage()
        {
            InitializeComponent();
            BindingContext = new ProvaEditViewModel();
        }
    }
}
