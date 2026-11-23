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
#if WINDOWS
            _semTransicao = true;
#endif
            try
            {
                await MontarShellAsync(janela, onde);
            }
            finally
            {
#if WINDOWS
                // espera o WinUI terminar de mostrar as telas novas antes de devolver as transições
                await Task.Delay(500);
                _semTransicao = false;
                foreach (var (frame, transicoes) in _framesSemTransicao)
                    frame.ContentTransitions = transicoes;
                _framesSemTransicao.Clear();
#endif
            }
        }

#if WINDOWS
        private static bool _semTransicao;
        private static readonly List<(Microsoft.UI.Xaml.Controls.Frame Frame, Microsoft.UI.Xaml.Media.Animation.TransitionCollection? Transicoes)> _framesSemTransicao = new();

        /// <summary>
        /// Chamado quando o Windows cria o Frame de uma seção do Shell (MauiProgram). Durante a troca de tema o
        /// Frame fica sem transição: senão o WinUI anima a entrada das telas novas (o conteúdo "sobe" de baixo).
        /// </summary>
        public static void PrepararFrame(Microsoft.UI.Xaml.Controls.Frame frame)
        {
            if (!_semTransicao || _framesSemTransicao.Any(f => f.Frame == frame))
                return;

            _framesSemTransicao.Add((frame, frame.ContentTransitions));
            frame.ContentTransitions = null;
        }
#endif

        private static async Task MontarShellAsync(Window janela, string? onde)
        {
            var novo = new AppShell();

            // Já abre na área certa (login, main, professor...): sem isso o Shell novo mostrava o login e
            // depois navegava até a tela, o que aparecia como uma animação estranha na troca de tema.
            var partes = (onde ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries);
            var area = partes.Length > 0 ? novo.Items.FirstOrDefault(i => i.Route == partes[0]) : null;
            if (area != null)
                novo.CurrentItem = area;

            janela.Page = novo;

            // telas empilhadas por cima da área (ex.: perfil) voltam sem animação
            if (partes.Length == 0 || (area != null && partes.Length < 2) || Shell.Current == null)
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
