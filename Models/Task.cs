using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.Models
{
    public class Task
    {
        [Key]
        public string FRNNumber { get; set; }
        [Required]
        public string OSSNumber { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public CaseSource CaseSource { get; set; }

        public string? CaseType { get; set; } = null;
        public TaskStatus Type { get; set; } = TaskStatus.Opened;
        public TaskPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; } = null;
        public DateTime? FrozenAt { get; set; } = null;

        public string? Reason { get; set; } 

        public string? Notes { get; set; } 

        public string UserID { get; set; }
        [ForeignKey("UserID")]
        public AppUser? User { get; set; }


        // العلاقة مع التقارير
        public ICollection<TaskReport>? TasksReports { get; set; }
    }

    public enum TaskPriority
    {
        Urgent,//قضايا السياح 48 ساعة     0
        Regular, //1     10 ايام عمل 
        Important //2    ايام عمل
    }

    public enum TaskStatus
    {
        Opened,    // 0
        Completed, // 1
        Delayed,   // 2
        Frozen     // 3
    }

       public enum CaseSource
    {
        JebelAli,             // جبل علي
        AlRaffa,              // الرفاعة
        AlRashidiya,          // الراشدية
        AlBarsha,             // البرشاء
        BurDubai,             // بر دبي
        Lahbab,               // لهباب
        AlFuqaa,              // الفقع
        Ports,                // الموانئ
        AlQusais,             // القصيص
        AlMuraqqabat,         // المرقبات
        Naif,                 // نايف
        AlKhawanij,           // الخوانيج
        Hatta,                // حتا
        AirportSecurity,      // أمن المطارات
        PublicProsecution,    // النيابة العامة
        DubaiMunicipality,    // بلدية دبي
        DubaiCustoms,         // جمارك دبي
        RasAlKhaimah,         // رأس الخيمة
        UmmAlQuwain,          // أم القيوين
        Ajman,                // عجمان
        AbuDhabi,             // أبو ظبي
        Fujairah,             // الفجيرة
        Sharjah,              // الشارقة
        Forensics,            // الطب الشرعي
        MinistryOfDefense     // وزارة الدفاع
    }
}
