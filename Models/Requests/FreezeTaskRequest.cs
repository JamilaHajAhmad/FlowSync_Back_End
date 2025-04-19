namespace WebApplicationFlowSync.Models.Requests
{
    public class FreezeTaskRequest : PendingMemberRequest
    {
        public string FRNNumber { get; set; }
        public string Reason { get; set; }
    }
}
