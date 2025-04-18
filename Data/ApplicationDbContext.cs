using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Models;

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

            //علاقة التقرير مع المهام عبر جدول الربط(بدون Cascade)
            modelBuilder.Entity<TaskReport>()
                .HasKey(tr => new { tr.ReportID, tr.FRNNumber });



            modelBuilder.Entity<TaskReport>()
                        .HasOne(tr => tr.Report)
                        .WithMany(r => r.TasksReports)
                        .HasForeignKey(tr => tr.ReportID)
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TaskReport>()
                .HasOne(tr => tr.Task)
                .WithMany(t => t.TasksReports)
                .HasForeignKey(tr => tr.FRNNumber)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PendingMemberRequest>()
      .HasOne(p => p.Member)
      .WithMany(u => u.SentJoinRequests)
      .HasForeignKey(p => p.MemberId)
      .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PendingMemberRequest>()
                .HasOne(p => p.Leader)
                .WithMany(u => u.ReceivedJoinRequests)
                .HasForeignKey(p => p.LeaderId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<TaskReport> TasksReports { get; set; }
        public DbSet<PendingMemberRequest> PendingMemberRequests { get; set; }
    }
}
