using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.Models
{
    public class TaskReport
    {
        [Key, Column(Order = 0)]
        public int ReportID { get; set; }

        [ForeignKey("ReportID")]
        public Report? Report { get; set; }

        [Key, Column(Order = 1)]
        public int FRNNumber { get; set; }

        [ForeignKey("FRNNumber")]
        public Task? Task { get; set; }
    }
}
