namespace WebApplicationFlowSync.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string UserID { get; set; }
        public AppUser User { get; set; }
        public DateTime EventDate { get; set; } = DateTime.Now;
        public bool ReminderSent1Day { get; set; }
        public bool ReminderSent1Hour { get; set; }
    }
}
