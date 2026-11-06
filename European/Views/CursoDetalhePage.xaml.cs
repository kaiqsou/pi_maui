using European.ViewModels;

namespace European.Views
{
    public partial class CursoDetalhePage : BasePage
    {
        public CursoDetalhePage()
        {
            InitializeComponent();
            BindingContext = new CursoDetalheViewModel();
        }
    }
}
