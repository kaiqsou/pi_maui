using European.ViewModels;

namespace European.Views
{
    public partial class SalaEditPage : BasePage
    {
        public SalaEditPage()
        {
            InitializeComponent();
            BindingContext = new SalaEditViewModel();
        }
    }
}
