using European.Services;

namespace European
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // tema claro por padrão; o escuro fica salvo se o usuário escolher (botão de sol/lua)
            TemaService.Iniciar();

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                CrashLogger.Registrar("AppDomain.UnhandledException", e.ExceptionObject as Exception);

            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                CrashLogger.Registrar("TaskScheduler.UnobservedTaskException", e.Exception);
                e.SetObserved();
            };
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