using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [Required, MaxLength(1000)]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public bool IsEdited { get; set; } = false;

        [ForeignKey("SenderId")]
        public AppUser Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public AppUser Receiver { get; set; }
    }
}
