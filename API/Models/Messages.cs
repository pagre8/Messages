using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Messages
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public virtual UserData From { get; set; } 
        public virtual UserData To { get; set; }
    }
}
