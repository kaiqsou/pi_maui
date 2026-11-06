using European.ViewModels;

namespace European.Views
{
    public partial class CursoInfoPage : BasePage
    {
        public CursoInfoPage()
        {
            InitializeComponent();
            BindingContext = new CursoInfoViewModel();
        }
    }
}
