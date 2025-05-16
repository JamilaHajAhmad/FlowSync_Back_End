using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.Models
{
    public class PendingMemberRequest
    {
        [Key]
        public int RequestId { get; set; }

        // العضو مقدم الطلب
        public string? MemberId { get; set; }
        public AppUser? Member { get; set; }
        // القائد المستلم للطلب
        public string? LeaderId { get; set; }
        public AppUser? Leader { get; set; }

        // نوع الطلب
        public RequestType Type { get; set; }

        // الحقول المشتركة
        public string MemberName { get; set; }
        public string Email { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public RequestStatus RequestStatus { get; set; } = RequestStatus.Pending;

    }
  
    public enum RequestType
    {
        Base,
        SignUp,
        CompleteTask,
        FreezeTask,
        DeleteAccount,
        ChangeStatus
    }

    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
