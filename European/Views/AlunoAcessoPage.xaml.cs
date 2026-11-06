using European.ViewModels;

namespace European.Views
{
    public partial class AlunoAcessoPage : BasePage
    {
        public AlunoAcessoPage()
        {
            InitializeComponent();
            BindingContext = new AlunoAcessoViewModel();
        }
    }
}
