using EuropeanMaui.Models;

namespace EuropeanMaui.Services
{
    /// <summary>
    /// Textos de UI (pt-BR). Fonte única dos textos visíveis na aplicação,
    /// equivalente aos arquivos JSON em src/content/pt-BR do front-end original.
    /// </summary>
    public static class AppText
    {
        public static class Common
        {
            public const string AppName = "AVA";
            public const string InstitutionName = "European & Icon Institute";
            public const string Tagline = "Ambiente Virtual de Aprendizagem";
            public const string Yes = "Sim";
            public const string No = "Não";
            public const string Cancel = "Cancelar";
            public const string Save = "Salvar";
            public const string Close = "Fechar";
            public const string Confirm = "Confirmar";
            public const string Back = "Voltar";
            public const string Next = "Avançar";
            public const string Loading = "Carregando...";
            public const string Retry = "Tentar novamente";
            public const string ErrorOccurred = "Algo deu errado.";
            public const string Empty = "Nenhum item disponível.";
            public const string Search = "Buscar";
            public const string SeeAll = "Ver todos";
            public const string Optional = "Opcional";
        }

        public static class Login
        {
            public const string Title = "Entrar";
            public const string Subtitle = "Acesse sua conta para continuar";
            public const string EmailLabel = "E-mail";
            public const string EmailPlaceholder = "voce@exemplo.com";
            public const string PasswordLabel = "Senha";
            public const string PasswordPlaceholder = "Digite sua senha";
            public const string ShowPassword = "Mostrar";
            public const string HidePassword = "Ocultar";
            public const string RememberMe = "Lembrar de mim";
            public const string ForgotPassword = "Esqueci minha senha";
            public const string LoginButton = "Entrar";
            public const string LoginLoading = "Entrando...";
            public const string NoAccount = "Não tem conta?";
            public const string SignupLink = "Criar conta";
            public const string ErrorEmailRequired = "Informe seu e-mail.";
            public const string ErrorEmailInvalid = "Digite um e-mail válido.";
            public const string ErrorPasswordRequired = "Informe sua senha.";
            public const string ErrorPasswordMin = "A senha deve ter pelo menos 6 caracteres.";
            public const string ErrorInvalidCredentials = "E-mail ou senha inválidos.";
            public const string ServerError = "Não foi possível conectar. Tente novamente.";
            public const string DemoHint = "Use qualquer e-mail e senha com 6+ caracteres para entrar (demonstração).";
            public const string SelectProgram = "Selecione seu mestrado";
            public const string SelectProgramHint = "Escolha o mestrado em que você está matriculado";
            public const string EnterProgram = "Entrar no mestrado";
        }

        public static class Dashboard
        {
            public const string Greeting = "Olá";
            public const string WelcomeBack = "Bem-vindo de volta";
            public const string CurrentProgram = "Mestrado";
            public const string DueSoon = "Prazos próximos";
            public const string NoDueItems = "Nenhum prazo por agora.";
            public const string ProgressLabel = "Progresso";
            public const string ContinueLabel = "Continuar";
            public const string ContinueWhere = "Continue de onde parou";
            public const string CompletedLabel = "concluído";
            public const string ModuleCompleted = "Concluído";
            public const string ModuleInProgress = "Em andamento";
            public const string ModuleNotStarted = "Não iniciado";
            public const string ActivityPending = "Atividade para entregar";
            public const string ExamScheduled = "Prova marcada";
            public const string DueInOneDay = "Vence amanhã";
            public const string DueInDaysTemplate = "Vence em {days} dias";
            public const string CalendarTitle = "Calendário";
            public const string EventsTitle = "Eventos & presenciais";
            public const string NoEvents = "Nenhum evento programado.";
            public const string ViewFullCalendar = "Ver calendário completo";
            public const string ViewMore = "Ver mais";
            public const string StatsCompleted = "Módulos concluídos";
            public const string StatsProgress = "Progresso geral";
            public const string StatsDue = "Prazos pendentes";
            public const string StudyTitle = "Atividade de estudo";
            public const string StudySummaryTemplate = "Logou {active} de {total} dias";
            public const string StudyStreakTemplate = "Estudando há {days} dias seguidos";
            public const string StudyNoStreak = "Não deixe de estudar!";

            public static string DueInDays(int days)
                => DueInDaysTemplate.Replace("{days}", days.ToString());

            public static string StudySummary(int active, int total)
                => StudySummaryTemplate.Replace("{active}", active.ToString()).Replace("{total}", total.ToString());

            public static string StudyStreak(int days)
                => StudyStreakTemplate.Replace("{days}", days.ToString());
        }

        public static class Navigation
        {
            public const string AppName = "AVA";
            public const string Home = "Início";
            public const string SearchPlaceholder = "Buscar módulos, materiais...";
            public const string NotificationsTitle = "Notificações";
            public const string MenuEditProfile = "Editar perfil";
            public const string MenuSettings = "Configurações";
            public const string MenuHelp = "Ajuda";
            public const string MenuLogout = "Sair";
            public const string SidebarDashboard = "Dashboard";
            public const string SidebarModules = "Módulos";
            public const string SidebarExams = "Provas";
            public const string SidebarForums = "Fóruns";
            public const string SidebarChat = "Chat";
            public const string SidebarCertificates = "Certificados";
            public const string SidebarLibrary = "Biblioteca";
            public const string NavDashboard = "Início";
            public const string NavModules = "Módulos";
            public const string NavExams = "Provas";
            public const string NavProfile = "Perfil";
            public const string NavMore = "Mais";
        }

        public static class Perfil
        {
            public const string Title = "Perfil";
            public const string Subtitle = "Suas informações e preferências";
            public const string Account = "Conta";
            public const string NameLabel = "Nome";
            public const string EmailLabel = "E-mail";
            public const string RoleLabel = "Perfil";
            public const string ProgramLabel = "Mestrado";
            public const string RoleStudent = "Aluno";
            public const string RoleTeacher = "Professor";
            public const string RoleCoordinator = "Coordenador";
            public const string RoleAdmin = "Administrador";
            public const string Preferences = "Preferências";
            public const string ThemeLabel = "Tema";
            public const string ThemeLight = "Claro";
            public const string ThemeDark = "Escuro";
            public const string ThemeSystem = "Sistema";
            public const string NotificationsLabel = "Notificações";
            public const string AboutLabel = "Sobre o app";
            public const string Version = "Versão";
            public const string Logout = "Sair";
            public const string LogoutConfirmTitle = "Sair da conta";
            public const string LogoutConfirmMessage = "Deseja realmente sair?";
            public const string LogoutLoading = "Saindo...";
        }

        public static class Configuracoes
        {
            public const string Title = "Configurações";
            public const string Appearance = "Aparência";
            public const string ThemeLabel = "Tema";
            public const string NotificationsLabel = "Notificações";
            public const string NotificationsDisabled = "Em breve";
            public const string AboutLabel = "Sobre o app";
            public const string Version = "Versão";
            public const string VersionValue = "1.0.0";
            public const string AppName = "AVA European & Icon Institute";
        }

        public static class Placeholders
        {
            public const string ExamsTitle = "Provas";
            public const string ExamsMessage = "Suas provas agendadas aparecerão aqui assim que o professor publicá-las.";
            public const string ForumsTitle = "Fóruns";
            public const string ForumsMessage = "Os fóruns de discussão dos seus módulos aparecerão aqui assim que forem abertos.";
            public const string ChatTitle = "Chat";
            public const string ChatMessage = "Suas conversas com professores e colegas aparecerão aqui.";
            public const string CertificatesTitle = "Certificados";
            public const string CertificatesMessage = "Seus certificados serão emitidos aqui automaticamente ao concluir os requisitos.";
            public const string LibraryTitle = "Biblioteca";
            public const string LibraryMessage = "Os materiais de apoio dos módulos aparecerão aqui.";
        }

        public static class Modulos
        {
            public const string Title = "Módulos";
            public const string Subtitle = "Acompanhe seu progresso nos módulos";
            public const string SearchPlaceholder = "Buscar módulos...";
            public const string EmptyResult = "Nenhum módulo encontrado.";
            public const string Hours = "h";
            public const string Instructor = "Professor";
            public const string ProgressLabel = "Progresso";
            public const string CompletedLabel = "concluído";
            public const string ModuleCompleted = "Concluído";
            public const string ModuleInProgress = "Em andamento";
            public const string ModuleNotStarted = "Não iniciado";

            public static string ProgressFormat(double progress)
                => $"{ProgressLabel} {progress:0}% • {CompletedLabel}";
        }

        /// <summary>Catálogo de mestrados e módulos (fonte da verdade).</summary>
        public static List<MasterProgram> Catalog { get; } = BuildCatalog();

        private const string Lorem =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

        private static Module M(string id, string title, double progress) =>
            new Module { Id = id, Title = title, Description = Lorem, DurationHours = 8, Progress = progress };

        private static List<MasterProgram> BuildCatalog() => new()
        {
            new MasterProgram
            {
                Id = "hof",
                Code = "HOF",
                FullName = "Harmonização Orofacial",
                Subtitle = Lorem,
                Modules = new()
                {
                    M("hof-m1", "Módulo 1", 100),
                    M("hof-m2", "Módulo 2", 70),
                    M("hof-m3", "Módulo 3", 45),
                    M("hof-m4", "Módulo 4", 10),
                    M("hof-m5", "Módulo 5", 0),
                    M("hof-m6", "Módulo 6", 0),
                }
            },
            new MasterProgram
            {
                Id = "dpe",
                Code = "DPE",
                FullName = "Direito Penal Econômico",
                Subtitle = Lorem,
                Modules = new()
                {
                    M("dpe-m1", "Módulo 1", 100),
                    M("dpe-m2", "Módulo 2", 55),
                    M("dpe-m3", "Módulo 3", 20),
                    M("dpe-m4", "Módulo 4", 0),
                    M("dpe-m5", "Módulo 5", 0),
                }
            }
        };
    }
}