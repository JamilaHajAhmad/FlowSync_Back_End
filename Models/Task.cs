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
