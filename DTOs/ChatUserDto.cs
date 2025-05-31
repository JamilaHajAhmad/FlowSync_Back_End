namespace WebApplicationFlowSync.DTOs
{
    public class ChatUserDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PictureURL { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastMessageSentAt { get; set; } // لأجل الترتيب
        public object LastMessage { get; set; }
    }
}
