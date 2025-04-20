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
        public string Tiltle { get; set; }
        [Required]
        public CaseSource CaseSource { get; set; }

        public CaseType? CaseType { get; set; }
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
        // Traditional Police Stations
        NaifPoliceStation,
        AlMuraqqabatPoliceStation,
        AlQusaisPoliceStation,
        AlRashidiyaPoliceStation,
        JebelAliPoliceStation,
        AlBarshaPoliceStation,
        PortsPoliceStation,
        AlFuqaaPoliceStation,
        LahbabPoliceStation,
        AlRaffaPoliceStation,

        // Smart Police Stations (SPS)
        SmartPoliceStation_LaMer,
        SmartPoliceStation_CityWalk,
        SmartPoliceStation_PalmJumeirah,
        SmartPoliceStation_DubaiDesignDistrict,
        SmartPoliceStation_Hatta
    }

    public enum CaseType
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
