namespace WebApplicationFlowSync.DTOs
{
    public class SaveReportRequestDto
    {
        public string? Description { get; set; }
        public string? FileFormat { get; set; } // "pdf" أو "excel"
    }
}
