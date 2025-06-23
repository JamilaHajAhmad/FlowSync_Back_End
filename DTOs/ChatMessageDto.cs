namespace WebApplicationFlowSync.DTOs
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEdited { get; set; }
        public bool IsForwarded { get; set; }
        public bool IsMine { get; set; }
    }
}
