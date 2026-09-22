namespace EuropeanMaui.Models
{
    public class Module
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public double Progress { get; set; }

        public bool IsCompleted => Progress >= 100;
        public bool IsInProgress => Progress > 0 && Progress < 100;
        public bool IsNotStarted => Progress <= 0;
    }
}