namespace JaezooServer.Models
{
    public class Message
    {
        public required string FromUser { get; set; }
        public required string ToUser { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}