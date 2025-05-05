using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public NotificationType Type { get; set; }
    }

    public enum NotificationType
    {
        SignUpRequest,
        Approval,
        Rejection,
        CompleteTaskRequest,
        FreezeTaskRequest,
        Info,
        Warning,
        Error
    }
}
