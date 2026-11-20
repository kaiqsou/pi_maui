using European.ViewModels;

namespace European.Views
{
    public partial class ProfessorPage : BasePage
    {
        public ProfessorPage()
        {
            InitializeComponent();
            BindingContext = new ProfessorViewModel();
        }
    }
}
