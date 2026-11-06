using European.ViewModels;

namespace European.Views
{
    public partial class CursoEditPage : BasePage
    {
        public CursoEditPage()
        {
            InitializeComponent();
            BindingContext = new CursoEditViewModel();
        }
    }
}
