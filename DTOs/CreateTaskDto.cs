using System.ComponentModel.DataAnnotations;
using WebApplicationFlowSync.Models;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.DTOs
{
        public class CreateTaskDto
        {

            [Required]
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

            [Required]
            public TaskPriority Priority { get; set; }

            [Required]
            public TaskStatus Type { get; set; }

            [Required]
            public string SelectedMemberId { get; set; }

    }

}
