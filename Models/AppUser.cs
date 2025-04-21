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

    public UserStatus Status { get; set; } = UserStatus.OnDuty;


    public Major? Major { get; set; }

    [Required, MaxLength(20)]
    public Role Role { get; set; } // "Leader" أو "Member"

    public string? PictureURL { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }
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

    public DateTime? DateOfBirth { get; set;}

     public DateTime? JoinedAt { get; set; }


        [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

}

public enum Role
{
    Leader,
    Member
}

public enum UserStatus
{
    Temporarilyleave,
    Annuallyleave,
    OnDuty
 }

    public enum Major
    {
        MechanicalEngineering,       // هندسة ميكانيكية - لتحليل الحركات والاصطدامات
        ElectricalEngineering,       // هندسة كهربائية - لفحص الأعطال أو التوصيلات اللي قد تسبب حرائق
        CivilEngineering,            // هندسة مدنية - لفحص المباني والجسور والانهيارات
        StructuralEngineering,       // هندسة إنشائية - تحليل الهياكل بعد الانهيارات
        ChemicalEngineering,         // هندسة كيميائية - لفحص التفاعلات والانفجارات
        ComputerForensics,           // هندسة/تحليل أدلة رقمية
        AudioForensics,              // تحليل تسجيلات صوتية
        ImageVideoForensics,         // تحليل الصور والفيديو
        GeneticForensics,            // تحليل DNA
        FireForensics,               // تحقيقات في مسببات الحرائق
        AutomotiveForensics          // تحليل حوادث السيارات من منظور هندسي
    }
}
