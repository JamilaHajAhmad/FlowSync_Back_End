using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.Models
{
    public class Report
    {
        [Key]
        public int ReportID { get; set; }

        public string FiltersApplied { get; set; }

        public string UserID { get; set; }
        [ForeignKey("UserID")]
        public AppUser? User { get; set; }

        public DateTime CreatedAt { get; set; }

        // العلاقة مع المهام
        public ICollection<TaskReport>? TasksReports { get; set; }
    }
}
