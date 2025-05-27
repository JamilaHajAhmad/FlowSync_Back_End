using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.Models
{
    public class Subscriber
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@dubaipolice\.gov\.ae$",
            ErrorMessage = "Only emails from @dubaipolice.gov.ae domain are allowed.")]
        public string Email { get; set; }
        public DateTime SubscribedAt { get; set; } = DateTime.Now;
    }
}
