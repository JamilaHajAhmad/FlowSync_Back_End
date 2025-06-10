using System.ComponentModel.DataAnnotations;
using WebApplicationFlowSync.Models;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.DTOs
{
    public class EditTaskDto
    {

        [RegularExpression(@"^\d{5}$", ErrorMessage = "New FRNNumber must be exactly 5 digits.")]
        public string? FRNNumber { get; set; }  // هذا هو الرقم الجديد المراد تعيينه

        [RegularExpression(@"^\d{12}$", ErrorMessage = "OSSNumber must be exactly 12 digits.")]
        public string? OSSNumber { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9\s.,'-]+$", ErrorMessage = "Title must contain only English letters and valid characters.")]
        public string? Title { get; set; }
        
        public CaseSource? CaseSource { get; set; }

        public string? CaseType { get; set; } = null;

        public TaskPriority? Priority { get; set; }

        public string? SelectedMemberId { get; set; }
    }
}
