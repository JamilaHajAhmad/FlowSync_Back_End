using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
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

        public async Task<double> CalculateMemberAnnualKPIAsync(string memberId, int year)
        {
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31, 23, 59, 59);

            var tasks = await context.Tasks
                .Where(t => t.UserID == memberId && t.CreatedAt >= start && t.CreatedAt <= end)
                .ToListAsync();

            if (!tasks.Any())
                return 0;

            var completed = tasks.Count(t => t.Type == TaskStatus.Completed && !t.IsDelayed);
            return (double)completed / tasks.Count * 100;
        }

        public async Task<double> CalculateLeaderAnnualKPIAsync(string leaderId, int year)
        {
            var leader = await userManager.Users
                .Include(u => u.TeamMembers)
                .FirstOrDefaultAsync(u => u.Id == leaderId && u.Role == Role.Leader);

            if (leader == null || leader.TeamMembers == null)
                return 0;

            var memberIds = leader.TeamMembers
                .Where(m => !m.IsRemoved)
                .Select(m => m.Id)
                .ToList();

            if (!memberIds.Any())
                return 0;

            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31, 23, 59, 59);

            var tasks = await context.Tasks
                .Where(t => memberIds.Contains(t.UserID) && t.CreatedAt >= start && t.CreatedAt <= end)
                .ToListAsync();

            if (!tasks.Any())
                return 0;

            var completed = tasks.Count(t => t.Type == TaskStatus.Completed && !t.IsDelayed);
            return (double)completed / tasks.Count * 100;
        }
    }
}
