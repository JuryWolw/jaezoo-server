namespace JaezooServer.Models
{
    public class User
    {
        public required string Id { get; set; }
        public required string Login { get; set; }
        public required string Password { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarPath { get; set; }
        public string? Description { get; set; }
        public List<string> Friends { get; set; } = new();
        public string Status { get; set; } = "Online";
    }
}