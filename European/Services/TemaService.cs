using European.Resources.Styles;

namespace European.Services
{
    /// <summary>
    /// Tema claro (padrão) ou escuro. Troca a paleta de cores do app (as telas usam DynamicResource)
    /// e guarda a escolha no aparelho.
    /// </summary>
    public static class TemaService
    {
        private const string Chave = "tema_escuro";

        public static bool Escuro { get; private set; }

        /// <summary>Avisa os botões de tema para atualizar o ícone.</summary>
        public static event Action? Alterado;

        /// <summary>Aplica o tema salvo (chamado ao abrir o app).</summary>
        public static void Iniciar() => Aplicar(Preferences.Default.Get(Chave, false));

        public static async void Alternar()
        {
            Aplicar(!Escuro);
            await RecriarTelasAsync();
        }

        private static void Aplicar(bool escuro)
        {
            var app = Application.Current;
            if (app == null)
                return;

            // as cores são lidas com DynamicResource, então basta trocar a paleta (a posição não importa)
            var dicionarios = app.Resources.MergedDictionaries;
            foreach (var paleta in dicionarios.Where(d => d is CoresClaras or CoresEscuras).ToList())
                dicionarios.Remove(paleta);
            dicionarios.Add(escuro ? new CoresEscuras() : new CoresClaras());

            // controles nativos (listas, calendário, barra de rolagem) seguem o mesmo tema
            app.UserAppTheme = escuro ? AppTheme.Dark : AppTheme.Light;

            Escuro = escuro;
            Preferences.Default.Set(Chave, escuro);
            AplicarNaJanela();
            Alterado?.Invoke();
        }

        /// <summary>
        /// No Windows, os controles nativos (switch, setinha das listas...) seguem o tema do sistema, não o do app:
        /// com o Windows no escuro e o app no claro, o contorno do switch saía branco sobre o card branco.
        /// Chamado também quando a janela abre (App.CreateWindow), pois no início do app ela ainda não existe.
        /// </summary>
        public static void AplicarNaJanela()
        {
#if WINDOWS
            if (Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView is Microsoft.UI.Xaml.Window { Content: Microsoft.UI.Xaml.FrameworkElement raiz })
                raiz.RequestedTheme = Escuro ? Microsoft.UI.Xaml.ElementTheme.Dark : Microsoft.UI.Xaml.ElementTheme.Light;
#endif
        }

        /// <summary>
        /// Monta as telas de novo, voltando para onde o usuário estava. Sem isso, cores que passaram por um
        /// Trigger/DataTrigger (filtros, chips de status...) ficam presas no tema anterior: ao desligar, o MAUI
        /// devolve a cor antiga como valor fixo, que não acompanha mais a troca de paleta.
        /// </summary>
        private static async Task RecriarTelasAsync()
        {
            var janela = Application.Current?.Windows.FirstOrDefault();
            if (janela?.Page is not Shell anterior)
                return;

            var onde = anterior.CurrentState?.Location?.OriginalString;
            janela.Page = new AppShell();

            if (string.IsNullOrEmpty(onde) || Shell.Current == null)
                return;

            try
            {
                await Shell.Current.GoToAsync(onde, false);
            }
            catch (Exception ex)
            {
                CrashLogger.Registrar(nameof(TemaService), ex);
            }
        }
    }
}
