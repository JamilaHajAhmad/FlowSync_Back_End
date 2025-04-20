using System.ComponentModel.DataAnnotations;
using WebApplicationFlowSync.Models;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.DTOs
{
        public class CreateTaskDto
        {

            [Required]
             public int FRNNumber { get; set; }
            [Required]
            public string OSSNumber { get; set; }
            [Required]
            public string Title { get; set; }
        [Required]
            public CaseSource CaseSource { get; set; }

            public CaseType? CaseType { get; set; }

            [Required]
            public TaskPriority Priority { get; set; }
            [Required]
            public TaskStatus Type { get; set; }

            [Required]
            public string SelectedMemberId { get; set; }

    }

}
