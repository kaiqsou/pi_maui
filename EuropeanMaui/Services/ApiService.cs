using EuropeanMaui.Models;

namespace EuropeanMaui.Services
{
    /// <summary>
    /// Camada Model — simula requisições à API (como src/api no front-end original).
    /// Quando o back-end estiver pronto, trocar estes mocks por chamadas reais.
    /// </summary>
    public class ApiService
    {
        public bool FailLogin { get; set; }
        public bool FailLists { get; set; }

        private static Task Delay(int ms) => Task.Delay(ms);

        private async Task<T> MockFetch<T>(T data, int ms, bool shouldFail)
        {
            await Delay(ms);
            if (shouldFail) throw new Exception("Network request failed");
            return data;
        }

        /// <summary>Simula o login. Aceita qualquer e-mail válido com senha >= 6 caracteres.</summary>
        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var result = await MockFetch(
                new LoginResponse
                {
                    Token = "mock-token-123",
                    User = new User
                    {
                        Id = "u-1",
                        Name = string.IsNullOrEmpty(email) ? "Estudante" : email.Split('@')[0],
                        Email = email,
                        Role = UserRole.Student,
                        MasterProgramIds = new() { "hof", "dpe" }
                    }
                },
                900,
                FailLogin);

            return result;
        }

        public async Task<List<MasterProgram>> FetchMasterProgramsAsync()
        {
            await Delay(700);
            if (FailLists) throw new Exception("Network request failed");
            return AppText.Catalog;
        }

        public async Task<List<MasterProgram>> FetchMyMasterProgramsAsync(List<string> ids)
        {
            await Delay(600);
            if (FailLists) throw new Exception("Network request failed");
            return AppText.Catalog.FindAll(p => ids.Contains(p.Id));
        }

        public async Task<List<DueItem>> FetchDueItemsAsync()
        {
            await Delay(600);
            if (FailLists) throw new Exception("Network request failed");
            return new()
            {
                new DueItem { Id = "d1", ModuleId = "hof-m2", ModuleTitle = "Módulo 2", Type = DueItemType.Activity, DueInDays = 2 },
                new DueItem { Id = "d2", ModuleId = "hof-m3", ModuleTitle = "Módulo 3", Type = DueItemType.Exam, DueInDays = 5 }
            };
        }

        public async Task<List<StudyDay>> FetchStudyStatsAsync()
        {
            await Delay(300);
            return GenerateMockStats();
        }

        private static List<StudyDay> GenerateMockStats()
        {
            var days = new List<StudyDay>();
            var now = DateTime.Today;
            int dayOfWeek = (int)now.DayOfWeek;
            int mondayOffset = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
            int[] logPattern = { 1, 1, 0, 1, 0, 1, 1 };

            for (int i = 0; i < 7; i++)
            {
                var d = now.AddDays(-mondayOffset + i);
                days.Add(new StudyDay { Date = d.ToString("yyyy-MM-dd"), Logged = logPattern[i] == 1 });
            }

            return days;
        }
    }
}