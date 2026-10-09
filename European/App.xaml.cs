namespace European
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
            janela.Height = 700;
            janela.Width = 400;

            return janela;
        }
    }
}