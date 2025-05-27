namespace WebApplicationFlowSync.DTOs
{
    public class MonthlyTaskStatusCountDto
    {
        public string Month { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; } = new();
    }
}
