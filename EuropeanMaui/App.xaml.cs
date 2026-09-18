namespace EuropeanMaui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var janela = new Window(new AppShell());
            janela.Width = 400;
            janela.Height = 750;

            return janela;
        }
    }
}