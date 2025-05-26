using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.Models
{
    public class AnnualKPI
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        [Required]
        public int Year { get; set; }

        [Range(0, 100)]
        public double KPI { get; set; }

        public int CompletedTasks { get; set; }

        public int TotalTasks { get; set; }

        public DateTime CalculatedAt { get; set; } = DateTime.Now;
    }
}
