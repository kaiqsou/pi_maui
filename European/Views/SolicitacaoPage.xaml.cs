using European.ViewModels;

namespace European.Views
{
    public partial class SolicitacaoPage : BasePage
    {
        private readonly SolicitacaoViewModel _viewModel = new();

        public SolicitacaoPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        /// <summary>Com zoom 1, a foto ocupa exatamente a área do visualizador.</summary>
        private void AoMudarAreaFoto(object? sender, EventArgs e)
        {
            if (AreaFoto.Width > 0 && AreaFoto.Height > 0)
                _viewModel.AreaVisualizador = new Size(AreaFoto.Width, AreaFoto.Height);
        }

        /// <summary>Pinça (tela de toque) amplia ou reduz a foto; Scale vem como a variação desde o último evento.</summary>
        private void AoPincar(object? sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Running)
                _viewModel.Zoom = Math.Clamp(_viewModel.Zoom * e.Scale, SolicitacaoViewModel.ZoomMinimo, SolicitacaoViewModel.ZoomMaximo);
        }
    }
}
