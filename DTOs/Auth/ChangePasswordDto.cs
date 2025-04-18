using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.DTOs.Auth
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "confirm password is required..")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
