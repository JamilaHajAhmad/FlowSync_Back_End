using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Models;
using Task = System.Threading.Tasks.Task;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.services.KpiService
{
    public class KpiService
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;

        public KpiService(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task SaveOrUpdateAnnualKPIAsync(string userId, int year)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return;

            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31, 23, 59, 59);

            var tasks = await context.Tasks
                .Where(t => t.UserID == userId && t.CreatedAt >= start && t.CreatedAt <= end)
                .ToListAsync();

            var completed = tasks.Count(t => t.Type == TaskStatus.Completed && !t.IsDelayed);
            var total = tasks.Count;

            double kpi = total == 0 ? 0 : (double)completed / total * 100;

            var existing = await context.AnnualKPIs
                .FirstOrDefaultAsync(k => k.UserId == userId && k.Year == year);

            if (existing != null)
            {
                existing.KPI = Math.Round(kpi, 2);
                existing.CompletedTasks = completed;
                existing.TotalTasks = total;
                existing.CalculatedAt = DateTime.Now;
            }
            else
            {
                context.AnnualKPIs.Add(new AnnualKPI
                {
                    UserId = userId,
                    Year = year,
                    KPI = Math.Round(kpi, 2),
                    CompletedTasks = completed,
                    TotalTasks = total,
                    CalculatedAt = DateTime.Now
                });
            }

            await context.SaveChangesAsync();
            }
        }
}
