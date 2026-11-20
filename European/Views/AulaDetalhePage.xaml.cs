using European.ViewModels;

namespace European.Views
{
    public partial class AulaDetalhePage : BasePage
    {
        public AulaDetalhePage()
        {
            InitializeComponent();
            BindingContext = new AulaViewModel();
        }

        /// <summary>Ao sair da tela (voltar ou abrir outra), pausa o vídeo e grava onde o aluno parou.</summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            VideoPlayer.Pausar();
        }
    }
}
