namespace WebApplicationFlowSync.DTOs
{
    public class SendMessageDto
    {
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public bool IsForwarded { get; set; } = false;
    }
}
