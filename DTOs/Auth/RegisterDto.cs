using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplicationFlowSync.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "first name is required..")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "last name is required..")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "email is required.."), EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "password is required..")]
        public string Password { get; set; }

        [Required(ErrorMessage = "confirm password is required..")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "the password is confirmed incorrect..")]
        public string ConfirmPassword { get; set; }

        [Required]
        [EnumDataType(typeof(Models.Role), ErrorMessage = "Role must be either 'Team Leader' or 'Team Member'.")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Models.Role Role { get; set; }
    }
}
