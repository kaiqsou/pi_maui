using European.Resources.Styles;

namespace European.Services
{
    /// <summary>
    /// Tema claro (padrão) ou escuro. Troca a paleta de cores do app em tempo real (as telas usam DynamicResource)
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

        public static void Alternar() => Aplicar(!Escuro);

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
            Alterado?.Invoke();
        }
    }
}
