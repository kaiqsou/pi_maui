using European.ViewModels;

namespace European.Views
{
    public partial class PrimeiroAcessoPage : ContentPage
    {
        public PrimeiroAcessoPage()
        {
            InitializeComponent();
            BindingContext = new UserViewModel();
        }
    }
}
