using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
