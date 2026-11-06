using European.ViewModels;

namespace European.Views
{
    public partial class MateriaEditPage : BasePage
    {
        public MateriaEditPage()
        {
            InitializeComponent();
            BindingContext = new MateriaEditViewModel();
        }
    }
}
