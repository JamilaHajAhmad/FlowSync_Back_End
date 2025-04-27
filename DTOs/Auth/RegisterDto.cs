//using System.ComponentModel.DataAnnotations;
//using System.Text.Json.Serialization;

//namespace WebApplicationFlowSync.DTOs.Auth
//{
//    public class RegisterDto
//    {
//        [Required(ErrorMessage = "first name is required..")]
//        public string FirstName { get; set; }

//        [Required(ErrorMessage = "last name is required..")]
//        public string LastName { get; set; }

//        [Required(ErrorMessage = "email is required.."), EmailAddress]
//        public string Email { get; set; }

//        [Required(ErrorMessage = "password is required..")]
//        public string Password { get; set; }

//        [Required(ErrorMessage = "confirm password is required..")]
//        [DataType(DataType.Password)]
//        [Compare(nameof(Password), ErrorMessage = "the password is confirmed incorrect..")]
//        public string ConfirmPassword { get; set; }

//        [Required]
//        [EnumDataType(typeof(Models.Role), ErrorMessage = "Role must be either 'Team Leader' or 'Team Member'.")]
//        [JsonConverter(typeof(JsonStringEnumConverter))]
//        public Models.Role Role { get; set; }
//    }
//}

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplicationFlowSync.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "first name is required..")]
        [StringLength(50, ErrorMessage = "first name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "last name is required..")]
        [StringLength(50, ErrorMessage = "last name cannot exceed 50 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "email is required..")]
        [EmailAddress(ErrorMessage = "invalid email address format.")]
        [StringLength(100, ErrorMessage = "email cannot exceed 100 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "password is required..")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "password must be at least 6 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "confirm password is required..")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "the password is confirmed incorrect..")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "role is required.")]
        [EnumDataType(typeof(Models.Role), ErrorMessage = "role must be either 'Leader' or 'Member'.")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Models.Role Role { get; set; }
    }
}

