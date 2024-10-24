

namespace WebSocket_Server.Models
{
    public class MessageData
    {
        public Guid Id { get; set; }
        public Guid IdChat { get; set; }
        public Guid IdSender { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
