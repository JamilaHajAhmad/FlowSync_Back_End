namespace WebApplicationFlowSync.Models.Requests
{
    public class CompleteTaskRequest : PendingMemberRequest
    {
        public string FRNNumber { get; set; }
        public string Notes { get; set; }
    }
}
