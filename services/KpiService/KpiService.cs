using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Models;
using Task = System.Threading.Tasks.Task;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.services.KpiService
{
    public class KpiService : IKpiService
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;

        public KpiService(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<KpiResultDto> CalculateMemberAnnualKPIAsync(string memberId, int year)
        {
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31, 23, 59, 59);

            var tasks = await context.Tasks
                .Where(t => t.UserID == memberId && t.CreatedAt >= start && t.CreatedAt <= end)
                .ToListAsync();

            int total = tasks.Count();
            if (total == 0)
                return new KpiResultDto();

            int completed = tasks.Count(t => t.Type == TaskStatus.Completed && !t.IsDelayed);
            int delayed = tasks.Count(t => t.Type == TaskStatus.Delayed || (t.Type == TaskStatus.Completed && t.IsDelayed));
            int frozen = tasks.Count(t => t.Type == TaskStatus.Frozen);
            int ongoing = tasks.Count(t => t.Type == TaskStatus.Opened);

            double kpi = (double)completed / tasks.Count * 100;

            //Store or Update KPI in DB
            var existing = await context.AnnualKPIs
                .FirstOrDefaultAsync(k => k.UserId == memberId && k.Year == year);

            if (existing != null)
            {
                existing.KPI = kpi;
                existing.CompletedTasks = completed;
                existing.TotalTasks = total;
                existing.CalculatedAt = DateTime.Now;
            }
            else
            {
                context.AnnualKPIs.Add(new AnnualKPI
                {
                    UserId = memberId,
                    Year = year,
                    KPI = kpi,
                    CompletedTasks = completed,
                    TotalTasks = total,
                    CalculatedAt = DateTime.Now
                });
            }

            await context.SaveChangesAsync();

            return new KpiResultDto
            {
                KPI = kpi,
                TotalTasks = total,
                CompletedTasks = completed,
                DelayedTasks = delayed,
                FrozenTasks = frozen,
                OngoingTasks = ongoing
            };
        }

        public async Task<KpiResultDto> CalculateLeaderAnnualKPIAsync(string leaderId, int year)
        {
            var leader = await userManager.Users
                .Include(u => u.TeamMembers)
                .FirstOrDefaultAsync(u => u.Id == leaderId && u.Role == Role.Leader);

            if (leader == null || leader.TeamMembers == null)
                return new KpiResultDto();

            var memberIds = leader.TeamMembers
                .Where(m => !m.IsRemoved)
                .Select(m => m.Id)
                .ToList();

            if (!memberIds.Any())
                return new KpiResultDto();

            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31, 23, 59, 59);

            var tasks = await context.Tasks
                .Where(t => memberIds.Contains(t.UserID) && t.CreatedAt >= start && t.CreatedAt <= end)
                .ToListAsync();

            int total = tasks.Count;
            if (total == 0)
                return new KpiResultDto();

            int completed = tasks.Count(t => t.Type == TaskStatus.Completed && !t.IsDelayed);
            int delayed = tasks.Count(t => t.Type == TaskStatus.Delayed || (t.Type == TaskStatus.Completed && t.IsDelayed));
            int frozen = tasks.Count(t => t.Type == TaskStatus.Frozen);
            int ongoing = tasks.Count(t => t.Type == TaskStatus.Opened);

            
            double kpi = (double)completed / tasks.Count * 100;

            //Store or Update KPI in DB
            var existing = await context.AnnualKPIs
                .FirstOrDefaultAsync(k => k.UserId == leaderId && k.Year == year);

            if (existing != null)
            {
                existing.KPI = kpi;
                existing.CompletedTasks = completed;
                existing.TotalTasks = total;
                existing.CalculatedAt = DateTime.Now;
            }
            else
            {
                context.AnnualKPIs.Add(new AnnualKPI
                {
                    UserId = leaderId,
                    Year = year,
                    KPI = kpi,
                    CompletedTasks = completed,
                    TotalTasks = total,
                    CalculatedAt = DateTime.Now
                });
            }

            await context.SaveChangesAsync();


            return new KpiResultDto
            {
                KPI = kpi,
                TotalTasks = total,
                CompletedTasks = completed,
                DelayedTasks = delayed,
                FrozenTasks = frozen,
                OngoingTasks = ongoing
            };
        }
    }
}
