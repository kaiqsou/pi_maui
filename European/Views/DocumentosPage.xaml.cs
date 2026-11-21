using European.ViewModels;

namespace European.Views
{
    public partial class DocumentosPage : BasePage
    {
        public DocumentosPage()
        {
            InitializeComponent();
            BindingContext = new DocumentosViewModel();
        }
    }
}
