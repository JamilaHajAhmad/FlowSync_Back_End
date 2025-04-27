using System.ComponentModel.DataAnnotations;
using WebApplicationFlowSync.Models;

namespace WebApplicationFlowSync.DTOs
{
    public class EditProfileDto
    {
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(100, ErrorMessage = "Major cannot exceed 100 characters.")]
        public string? Major { get; set; }

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
        public string? Address { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [RegularExpression(@"^(\+?[0-9]{7,20})?$", ErrorMessage = "Invalid phone number format.")]
        public string? Phone { get; set; }

        [StringLength(255, ErrorMessage = "Picture URL cannot exceed 255 characters.")]
        [RegularExpression(@"^(http(s)?://.*)?$", ErrorMessage = "Invalid picture URL format.")]
        public string? PictureURL { get; set; }

        public UserStatus? Status { get; set; } = UserStatus.On_Duty;

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string? Bio { get; set; }
    }
}
