using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.Models.Requests;
using WebApplicationFlowSync.Models.Requests.WebApplicationFlowSync.Models.Requests;

namespace WebApplicationFlowSync.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // علاقة المستخدم بالقائد (قائد الفريق)
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Leader)
                .WithMany(l => l.TeamMembers)
                .HasForeignKey(u => u.LeaderID)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة المستخدم بالتقارير
            modelBuilder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة المستخدم بالمهام
            modelBuilder.Entity<Models.Task>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade); // أو أي سلوك مناسب


            modelBuilder.Entity<PendingMemberRequest>()
      .HasOne(p => p.Member)
      .WithMany(u => u.SentJoinRequests)
      .HasForeignKey(p => p.MemberId)
      .OnDelete(DeleteBehavior.SetNull);// لانه العلاقة  optional

            modelBuilder.Entity<PendingMemberRequest>()
                .HasOne(p => p.Leader)
                .WithMany(u => u.ReceivedJoinRequests)
                .HasForeignKey(p => p.LeaderId)
                .OnDelete(DeleteBehavior.NoAction);

            //TPH (وراثة)
            modelBuilder.Entity<PendingMemberRequest>()
           .HasDiscriminator<RequestType>("Type")
           .HasValue<SignUpRequest>(RequestType.SignUp)
           .HasValue<CompleteTaskRequest>(RequestType.CompleteTask)
           .HasValue<FreezeTaskRequest>(RequestType.FreezeTask)
           .HasValue<DeleteAccountRequest>(RequestType.DeleteAccount)
            .HasValue <ChangeStatusRequest>(RequestType.ChangeStatus);

            //تعديل اسم عمود FRN
            modelBuilder.Entity<CompleteTaskRequest>()
           .Property(c => c.FRNNumber)
            .HasColumnName("Complete_TaskId");

            modelBuilder.Entity<FreezeTaskRequest>()
           .Property(f => f.FRNNumber)
           .HasColumnName("Freeze_FRNNumber");

            modelBuilder.Entity<Subscriber>()
            .HasIndex(s => s.Email)
            .IsUnique();

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // مهم لتجنب الحذف التبادلي

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<Report> Reports { get; set; }
        //public DbSet<TaskReport> TasksReports { get; set; }
        public DbSet<PendingMemberRequest> PendingMemberRequests { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<AnnualKPI> AnnualKPIs { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

    }
}
