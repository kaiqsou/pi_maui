namespace EuropeanMaui.Models
{
    public enum DueItemType
    {
        Activity,
        Exam
    }

    public class DueItem
    {
        public string Id { get; set; } = string.Empty;
        public string ModuleId { get; set; } = string.Empty;
        public string ModuleTitle { get; set; } = string.Empty;
        public DueItemType Type { get; set; } = DueItemType.Activity;
        public int DueInDays { get; set; }
    }
}