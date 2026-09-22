using EuropeanMaui.Services;

namespace EuropeanMaui.Controls
{
    public record NavRoute(NavigationKey Key, string Icon, string ActiveIcon);

    /// <summary>Rotas de navegação (equivalentes a SIDEBAR_ROUTES e BOTTOM_NAV_ROUTES do front-end).</summary>
    public static class NavRoutes
    {
        public static readonly IReadOnlyList<NavRoute> SidebarMain = new[]
        {
            new NavRoute(NavigationKey.Dashboard, Icons.HomeOutline, Icons.Home),
            new NavRoute(NavigationKey.Modulos, Icons.BookOutline, Icons.Book),
            new NavRoute(NavigationKey.Provas, Icons.DocumentTextOutline, Icons.DocumentText),
        };

        public static readonly IReadOnlyList<NavRoute> SidebarSecondary = new[]
        {
            new NavRoute(NavigationKey.Foruns, Icons.ChatbubblesOutline, Icons.Chatbubbles),
            new NavRoute(NavigationKey.Chat, Icons.ChatboxEllipsesOutline, Icons.ChatboxEllipses),
            new NavRoute(NavigationKey.Certificados, Icons.RibbonOutline, Icons.Ribbon),
            new NavRoute(NavigationKey.Biblioteca, Icons.LibraryOutline, Icons.Library),
        };

        public static readonly IReadOnlyList<NavRoute> BottomNav = new[]
        {
            new NavRoute(NavigationKey.Dashboard, Icons.HomeOutline, Icons.Home),
            new NavRoute(NavigationKey.Modulos, Icons.BookOutline, Icons.Book),
            new NavRoute(NavigationKey.Provas, Icons.DocumentTextOutline, Icons.DocumentText),
            new NavRoute(NavigationKey.Perfil, Icons.PersonOutline, Icons.Person),
            new NavRoute(NavigationKey.Mais, Icons.GridOutline, Icons.Grid),
        };

        public static string LabelOf(NavRoute route) => route.Key switch
        {
            NavigationKey.Dashboard => AppText.Navigation.SidebarDashboard,
            NavigationKey.Modulos => AppText.Navigation.SidebarModules,
            NavigationKey.Provas => AppText.Navigation.SidebarExams,
            NavigationKey.Foruns => AppText.Navigation.SidebarForums,
            NavigationKey.Chat => AppText.Navigation.SidebarChat,
            NavigationKey.Certificados => AppText.Navigation.SidebarCertificates,
            NavigationKey.Biblioteca => AppText.Navigation.SidebarLibrary,
            NavigationKey.Perfil => AppText.Navigation.NavProfile,
            NavigationKey.Mais => AppText.Navigation.NavMore,
            _ => route.Key.ToString()
        };
    }

    /// <summary>Tint para navegação ativa/inativa (primaryDark/textMuted).</summary>
    public static class NavTint
    {
        public static (Color active, Color idle) Resolve()
            => (ThemeColors.Get("PrimaryDark"), ThemeColors.Get("TextMuted"));
    }
}