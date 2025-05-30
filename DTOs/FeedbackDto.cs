namespace WebApplicationFlowSync.DTOs
{
    public class FeedbackDto
    {
        public int Rating { get; set; } // من 1 إلى 5
        public string? Message { get; set; }
        public bool CanFollowUp { get; set; }
    }
}
