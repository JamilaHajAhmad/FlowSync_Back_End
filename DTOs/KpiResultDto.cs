namespace WebApplicationFlowSync.DTOs
{
    public class KpiResultDto
    {
        public double KPI { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int DelayedTasks { get; set; }
        public int FrozenTasks { get; set; }
        public int OngoingTasks { get; set; }
    }
}
