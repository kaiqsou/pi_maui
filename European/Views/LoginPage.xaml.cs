using European.ViewModels;

namespace European.Views
{
    public partial class LoginPage : BasePage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel();
        }
    }
}
