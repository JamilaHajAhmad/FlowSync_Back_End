using Microsoft.Graph.Models;
using WebApplicationFlowSync.Models;

namespace WebApplicationFlowSync.DTOs
{
    public class MemberDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public UserStatus Status { get; set; }
        public string Email { get; set; }
        public int OngoingTasks { get; set; }
        public bool IsDeactivated { get; set; }
        public string PictureURL { get; set; }
    }
}
