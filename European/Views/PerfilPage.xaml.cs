using European.ViewModels;

namespace European.Views
{
    public partial class PerfilPage : BasePage
    {
        public PerfilPage()
        {
            InitializeComponent();
            BindingContext = new PerfilViewModel();
        }
    }
}
