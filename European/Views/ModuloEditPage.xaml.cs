using European.ViewModels;

namespace European.Views
{
    public partial class ModuloEditPage : BasePage
    {
        public ModuloEditPage()
        {
            InitializeComponent();
            BindingContext = new ModuloEditViewModel();
        }
    }
}
