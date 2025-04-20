using WebApplicationFlowSync.Models;

namespace WebApplicationFlowSync.DTOs
{
    public class EditProfileDto
    {
        public string? FirstName {get; set;}
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Major { get; set; }
        public string? Address { get; set; }
        public string? PictureURL { get; set; }
        public string? Phone { get; set; }
        public UserStatus? Status { get; set; }
        public string? Bio { get; set; }

    }
}
