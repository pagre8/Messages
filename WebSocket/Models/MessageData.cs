namespace WebSocket_Server.Models
{
    public class MessageData
    {
        public string Id { get; set; }
        public string IdChat { get; set; }
        public string IdSender { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
