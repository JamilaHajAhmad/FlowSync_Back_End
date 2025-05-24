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

        public string? ChartFilePath { get; set; }
        public string? ChartFileContentType { get; set; }

    }
}
