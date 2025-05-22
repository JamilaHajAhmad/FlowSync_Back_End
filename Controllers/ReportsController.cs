using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ReportsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        //Bar Chart - Task Distribution by Team Member
        [HttpGet("tak-distribution-by-member")]
        public async Task<IActionResult> GetTaskDistributionByMember()
        {
            var data = await context.Tasks
                .GroupBy(t => new { t.UserID, t.User.FirstName, t.User.LastName, t.Type })
                .Select(g => new
                {
                    Member = g.Key.FirstName + " " + g.Key.LastName,
                    Status = g.Key.Type.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("tasks-over-months")]
        public async Task<IActionResult> GetTasksOverMonths()
        {
            var data = await context.Tasks
                .GroupBy(t => new { t.CreatedAt.Year , t.CreatedAt.Month})
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Created = g.Count(),
                    Completed = g.Count(t => t.Type == TaskStatus.Completed)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("task-status-summary")]
        public async Task<IActionResult> GetTaskStatusSummary()
        {
            var data = await context.Tasks
                .GroupBy(t => new { t.Type })
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("calendar-activity")]
        public async Task<IActionResult> GetCalendarActivity()
        {
            var data = await context.Tasks
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("tasks-by-case-source")]
        public async Task<IActionResult> GetTasksByDepartmentAndStatus()
        {
            var data = await context.Tasks
                .GroupBy(t => new { t.CaseSource, t.Type })
                .Select(g => new
                {
                    Department = g.Key.CaseSource.ToString(),
                    Status = g.Key.Type,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }

        //[HttpGet("team-progress-kpi")]
        //public async Task<IActionResult> GetTeamProgressVsKPI()
        //{
        //    var currentYear = DateTime.UtcNow.Year;

        //    // فرضًا: عندنا جدول Members فيه KPI سنوي لكل عضو
        //    var data = await context.Users
        //        .Select(member => new
        //        {
        //            Member = member.FirstName+" "+ member.LastName,
        //            KPI = member.AnnualKPI, // مثلاً 1000 مهمة في السنة
        //            MonthlyData = context.Tasks
        //                .Where(t => t.UserID == member.Id
        //                            && t.Type == TaskStatus.Completed
        //                            && t.CompletedAt.HasValue
        //                            && t.CompletedAt.Value.Year == currentYear)
        //                .GroupBy(t => t.CompletedAt.Value.Month)
        //                .Select(g => new
        //                {
        //                    Month = g.Key,
        //                    CompletedTasks = g.Count()
        //                }).ToList()
        //        }).ToListAsync();

        //    return Ok(data);
        //}




    }
}
