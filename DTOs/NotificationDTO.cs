using WebApplicationFlowSync.Models;

namespace WebApplicationFlowSync.DTOs
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public NotificationType Type { get; set; }
    }
}
