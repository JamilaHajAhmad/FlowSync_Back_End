using WebApplicationFlowSync.Models;

namespace WebApplicationFlowSync.DTOs
{
    public class EditProfileDto
    {
        public string? FirstName {get; set;}
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; } = null;
        public string? Major { get; set; } = null;
        public string? Address { get; set; } = null;
        public string? PictureURL { get; set; } = null;
        public string? Phone { get; set; } = null;
        public UserStatus? Status { get; set;}
        public string? Bio { get; set; } = null;

    }
}
