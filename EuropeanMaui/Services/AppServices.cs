namespace EuropeanMaui.Services
{
    /// <summary>
    /// Acesso estático ao provedor de serviços para controles/views que não
    /// passam por injeção de construtor (ex.: controles criados via XAML).
    /// </summary>
    public static class AppServices
    {
        public static IServiceProvider? Provider { get; private set; }

        public static event Action? ServicesReady;

        public static void SetProvider(IServiceProvider provider)
        {
            Provider = provider;
            ServicesReady?.Invoke();
        }

        public static T? Get<T>() where T : class
            => Provider?.GetService(typeof(T)) as T;
    }
}