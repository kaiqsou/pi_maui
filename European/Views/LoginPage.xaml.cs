using European.ViewModels;

namespace European.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new UserViewModel();
        }
    }
}
