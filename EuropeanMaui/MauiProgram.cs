using Microsoft.Extensions.Logging;
using EuropeanMaui.Services;

namespace EuropeanMaui
{
    public static class MauiProgram
    {
        /// <summary>Provedor de serviços da aplicação (preenchido após Build).</summary>
        public static IServiceProvider Services { get; private set; } = null!;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansBold");
                    fonts.AddFont("Ionicons.ttf", "Ionicons");
                });

            builder.Services.AddSingleton<ThemeSession>();
            builder.Services.AddSingleton<LayoutService>();
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<AuthSession>();
            builder.Services.AddSingleton<DialogService>();
            builder.Services.AddSingleton<NavigationService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            Services = app.Services;
            AppServices.SetProvider(Services);
            return app;
        }
    }
}