namespace WebApplicationFlowSync.Models.Requests
{
    public class ChangeStatusRequest : PendingMemberRequest
    {
        public UserStatus PreviousStatus { get; set; }
        public UserStatus NewStatus { get; set; }
    }

}
