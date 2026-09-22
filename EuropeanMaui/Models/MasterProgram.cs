namespace EuropeanMaui.Models
{
    public class MasterProgram
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public List<Module> Modules { get; set; } = new();

        public double AverageProgress
        {
            get
            {
                if (Modules.Count == 0) return 0;
                double sum = 0;
                foreach (var m in Modules) sum += m.Progress;
                return Math.Round(sum / Modules.Count);
            }
        }

        public int TotalHours
        {
            get
            {
                int sum = 0;
                foreach (var m in Modules) sum += m.DurationHours;
                return sum;
            }
        }
    }
}