using European.ViewModels;

namespace European.Views
{
    public partial class SalaDetalhePage : BasePage
    {
        public SalaDetalhePage()
        {
            InitializeComponent();
            BindingContext = new SalaDetalheViewModel();
        }
    }
}
