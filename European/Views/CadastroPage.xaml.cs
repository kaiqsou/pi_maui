using European.ViewModels;

namespace European.Views
{
    public partial class CadastroPage : ContentPage
    {
        public CadastroPage()
        {
            InitializeComponent();
            BindingContext = new UserViewModel();
        }
    }
}
