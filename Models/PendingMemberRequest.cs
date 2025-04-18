namespace WebApplicationFlowSync.Models
{
    public class PendingMemberRequest
    {
        public int Id { get; set; }

        //علاقة مع العضو(Member)
        public string? MemberId { get; set; }
        public AppUser? Member { get; set; }

        //علاقة مع القائد(Leader)
        public string LeaderId { get; set; }
        public AppUser Leader { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; } = false;
    }
}
