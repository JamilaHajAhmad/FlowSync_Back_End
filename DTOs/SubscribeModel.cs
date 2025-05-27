using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.DTOs
{
    public class SubscribeModel
    {
        [Required]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@dubaipolice\.gov\.ae$",
            ErrorMessage = "Only emails from @dubaipolice.gov.ae domain are allowed.")]
        public string Email { get; set; } = string.Empty;
    }
}
