using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.Models
{
    public class Task
    {
        [Key]
        public int FRNNumber { get; set; }
        [Required]
        public string OSSNumber { get; set; }
        [Required]
        public string CaseSource { get; set; }
        public TaskStatus Type { get; set; } = TaskStatus.Opened;
        public TaskPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserID { get; set; }
        [ForeignKey("UserID")]
        public AppUser? User { get; set; }


        // العلاقة مع التقارير
        public ICollection<TaskReport>? TasksReports { get; set; }
    }

    public enum TaskPriority
    {
        Urgant,//قضايا السياح 48 ساعة 
        Regular, //10 ايام عمل
        Important //10 ايام عمل
    }

    public enum TaskStatus
    {
        Opened,    // مفتوحة
        Completed, // مكتملة
        Delayed,   // متأخرة
        Frozen     // مجمدة بعد طلب من المستخدم
    }
}
