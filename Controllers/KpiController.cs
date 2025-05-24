using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Models;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KpiController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ApplicationDbContext context;

        public KpiController(UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }


        [HttpGet("member/{memberId}/annual-kpi")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMemberAnnualKpi(string memberId)
        {
            var user = await userManager.FindByIdAsync(memberId);
            if (user == null || user.Role != Role.Member)
                return NotFound("Member not found.");

            var startOfYear = new DateTime(DateTime.UtcNow.Year, 1, 1);
            var endOfYear = new DateTime(DateTime.UtcNow.Year, 12, 31);

            var tasks = await context.Tasks
                .Where(t => t.UserID == memberId &&
                            t.CreatedAt >= startOfYear &&
                            t.CreatedAt <= endOfYear)
                .ToListAsync();

            if (!tasks.Any())
                return Ok(new { KPI = 0, Message = "No tasks found for this member." });

            var completedNotDelayed = tasks
                .Count(t => t.Type == TaskStatus.Completed && !t.IsDelayed);

            double kpi = (double)completedNotDelayed / tasks.Count * 100;

            return Ok(new
            {
                KPI = Math.Round(kpi, 2)
            });
        }



        [HttpGet("leader/{leaderId}/annual-kpi")]
        [Authorize(Roles = "leader")]
        public async Task<IActionResult> GetLeaderAnnualKpi(string leaderId)
        {
            var leader = await userManager.Users
                .Include(l => l.TeamMembers)
                .FirstOrDefaultAsync(l => l.Id == leaderId && l.Role == Role.Leader);

            if (leader == null)
                return NotFound("Leader not found.");

            var memberIds = leader.TeamMembers?
                .Where(m => !m.IsRemoved)
                .Select(m => m.Id)
                .ToList();

            if (memberIds == null || !memberIds.Any())
                return Ok(new { KPI = 0, Message = "No active team members found." });

            var startOfYear = new DateTime(DateTime.UtcNow.Year, 1, 1);
            var endOfYear = new DateTime(DateTime.UtcNow.Year, 12, 31);

            var memberTasks = await context.Tasks
                .Where(t => memberIds.Contains(t.UserID) &&
                            t.CreatedAt >= startOfYear &&
                            t.CreatedAt <= endOfYear)
                .ToListAsync();

            if (!memberTasks.Any())
                return Ok(new { KPI = 0, Message = "No tasks assigned to team members this year." });

            var completedNotDelayed = memberTasks
                .Count(t => t.Type == TaskStatus.Completed && !t.IsDelayed);

            double kpi = (double)completedNotDelayed / memberTasks.Count * 100;

            return Ok(new
            {
                KPI = Math.Round(kpi, 2)
            });
        }

    }
}
