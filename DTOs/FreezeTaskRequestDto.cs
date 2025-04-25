using System.ComponentModel.DataAnnotations;

namespace WebApplicationFlowSync.DTOs
{
    public class FreezeTaskRequestDto
    {
        [Required]
        public string FRNNumber { get; set; }
        //[Required]
        //public string FRNNumber { get; set; }

        [Required]
        public string Reason { get; set; }
    }
}
