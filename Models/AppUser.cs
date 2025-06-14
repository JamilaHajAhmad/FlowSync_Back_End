using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApplicationFlowSync.Models
{
    public class AppUser : IdentityUser
    { 
    [Required, MaxLength(50)]
    public string FirstName { get; set; }

    [Required, MaxLength(50)]
    public string LastName { get; set; }

    public UserStatus Status { get; set; } = UserStatus.On_Duty;
    public bool IsDeactivated { get; set; } = false;
    public string? Major { get; set; } = null;

    [Required, MaxLength(20)]
    public Role Role { get; set; } // "Leader" أو "Member"

        public string? PictureURL { get; set; } = null;

        [MaxLength(255)]
        public string? Address { get; set; } = null;
    public string? LeaderID { get; set; }


    [ForeignKey("LeaderID")]
    public AppUser? Leader { get; set; }

    public ICollection<AppUser>? TeamMembers { get; set; }

    // العلاقة مع المهام
    public ICollection<Task>? Tasks { get; set; }

    // العلاقة مع التقارير
    public ICollection<Report>? Reports { get; set; }

    //الطلبات التي أرسلها هذا المستخدم كـ Member
    public ICollection<PendingMemberRequest>? SentJoinRequests { get; set; }

    // الطلبات التي استلمها هذا المستخدم كـ Leader
    public ICollection<PendingMemberRequest>? ReceivedJoinRequests { get; set; }

    public ICollection<Notification>? Notifications { get; set; }

    public ICollection<UserSession>? Sessions { get; set; }

    public ICollection<AnnualKPI>? AnnualKPIs { get; set; }
        public ICollection<CalendarEvent> CalendarEvents { get; set; }
        public ICollection<ChatMessage> SentMessages { get; set; }
    public ICollection<ChatMessage> ReceivedMessages { get; set; }

        public DateTime? DateOfBirth { get; set; } = null;

        public DateTime? JoinedAt { get; set; } = null;

        [MaxLength(20)]
        public string? Phone { get; set; } = null;

        [MaxLength(500)]
        public string? Bio { get; set; } = null;

}

public enum Role
{
    Leader,
    Member,
    Admin
    }
 public enum UserStatus
 {
     Temporarily_Leave,
     Annually_Leave,
     On_Duty
    }

}
