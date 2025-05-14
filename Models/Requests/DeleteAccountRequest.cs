namespace WebApplicationFlowSync.Models.Requests
{
    namespace WebApplicationFlowSync.Models.Requests
    {
        public class DeleteAccountRequest : PendingMemberRequest
        {
            public string? Reason { get; set; } // اختياري: سبب الحذف
        }
    }

}
