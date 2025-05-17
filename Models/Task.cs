using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WebApplicationFlowSync.Models
{
    public class Task
    {
        [Key]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "FRNNumber must be exactly 5 digits.")]
        public string FRNNumber { get; set; }

        [Required]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "OSSNumber must be exactly 12 digits.")]
        public string OSSNumber { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\s.,'-]+$", ErrorMessage = "Title must contain only English letters and valid characters.")]
        public string Title { get; set; }
        [Required]
        public CaseSource CaseSource { get; set; }

        public string? CaseType { get; set; } = null;
        public TaskStatus Type { get; set; } = TaskStatus.Opened;
        public bool IsDelayed { get; set; } = false;
        public TaskPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime? CompletedAt { get; set; } = null;
        public DateTime? FrozenAt { get; set; } = null;

        public string? Reason { get; set; }

        public string? Notes { get; set; }

        public string UserID { get; set; }
        [ForeignKey("UserID")]
        public AppUser? User { get; set; }


        // العلاقة مع التقارير
        public ICollection<TaskReport>? TasksReports { get; set; }


        public void SetDeadline()
        {
            int allowedWorkingDays = Priority switch
            {
                TaskPriority.Urgent => 2,
                TaskPriority.Regular => 10,
                TaskPriority.Important => 10,
                _ => 0
            };

            Deadline = AddWorkingDays(CreatedAt, allowedWorkingDays);
        }

        [NotMapped]
        public TimeSpan Counter
        {
            get
            {
                int allowedWorkingDays = Priority switch
                {
                    TaskPriority.Urgent => 2,
                    TaskPriority.Regular => 10,
                    TaskPriority.Important => 10,
                    _ => 0
                };

                // نحسب التاريخ النهائي حسب أيام العمل فقط
                DateTime deadline = AddWorkingDays(CreatedAt, allowedWorkingDays);

                // الفارق بين الآن والموعد النهائي
                return deadline - DateTime.Now;
            }
        }

        private DateTime AddWorkingDays(DateTime startDate, int workingDays)
        {
            var current = startDate;
            while (workingDays > 0)
            {
                current = current.AddDays(1);
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays--;
                }
            }
            return current;
        }

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
