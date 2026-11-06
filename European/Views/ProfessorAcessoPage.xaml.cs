using European.ViewModels;

namespace European.Views
{
    public partial class ProfessorAcessoPage : BasePage
    {
        public ProfessorAcessoPage()
        {
            InitializeComponent();
            BindingContext = new ProfessorAcessoViewModel();
        }
    }
}
