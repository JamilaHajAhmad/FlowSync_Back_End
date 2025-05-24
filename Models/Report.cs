using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.Models
{
    public class Report
    {
        [Key]
        public int ReportID { get; set; }

        public string UserID { get; set; }
        [ForeignKey("UserID")]
        public AppUser? User { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? DataJson { get; set; }           // البيانات المولدة (إحصائيات أو غيره)
        public string FiltersApplied { get; set; }      // الفلاتر المختارة

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        public byte[]? FileData { get; set; } // محتوى الملف نفسه
        public string? FileName { get; set; } // اسم الملف
        public string? FileContentType { get; set; } // نوع الملف (MIME type)

    }
}
