namespace EuropeanMaui.Models
{
    public enum UserRole
    {
        Student,
        Teacher,
        Coordinator,
        Admin
    }

    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Student;
        public string? AvatarUrl { get; set; }
        public List<string> MasterProgramIds { get; set; } = new();
    }
}