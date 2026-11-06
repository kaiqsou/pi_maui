using European.ViewModels;

namespace European.Views
{
    public partial class CertificadoDetalhePage : BasePage
    {
        public CertificadoDetalhePage()
        {
            InitializeComponent();
            BindingContext = new CertificadoDetalheViewModel();
        }
    }
}
