using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.services.KpiService;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KpiController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly IKpiService kpiService;

        public KpiController(UserManager<AppUser> userManager, ApplicationDbContext context, IKpiService kpiService)
        {
            this.userManager = userManager;
            this.context = context;
            this.kpiService = kpiService;
        }


        [HttpGet("member/annual-kpi")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMemberAnnualKpi()
        {
            var member = await userManager.GetUserAsync(User);
            if (member == null || member.Role != Role.Member)
                return NotFound("Member not found.");

            int year = DateTime.UtcNow.Year;
            var kpiResult = await kpiService.CalculateMemberAnnualKPIAsync(member.Id, year);

            return Ok(kpiResult);
        }

        [HttpGet("leader/annual-kpi")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> GetLeaderAnnualKpi()
        {
            var leader = await userManager.GetUserAsync(User);
            if (leader == null || leader.Role != Role.Leader)
                return NotFound("Leader not found.");

            int year = DateTime.UtcNow.Year;
            var kpiResult = await kpiService.CalculateLeaderAnnualKPIAsync(leader.Id, year);

            return Ok(kpiResult);
        }

        [HttpGet("admin/leader-annual-kpi")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLeaderAnnualKpiByAdmin()
        {
            var user = await userManager.GetUserAsync(User);
            
            if(user == null)
                return Forbid();


            var leader = await context.Users.FirstOrDefaultAsync(u => u.Role == Role.Leader && !u.IsDeactivated);
            if (leader == null)
                return NotFound("No active leader found.");

            int year = DateTime.UtcNow.Year;
            var kpiResult = await kpiService.CalculateLeaderAnnualKPIAsync(leader.Id, year);

            return Ok(kpiResult);
        }


        [HttpGet("leader/team-kpis")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> GetTeamMembersKpis([FromQuery] int? year)
        {
            var leader = await userManager.GetUserAsync(User);

            if (leader == null )
                return Unauthorized();

            int selectedYear = year ?? DateTime.Now.Year;

            var teamMembers = await context.Users
                .Where(m => m.LeaderID == leader.Id && !m.IsDeactivated)
                .ToListAsync();

            if (!teamMembers.Any())
                return NotFound("No team members found.");

            var kpis = await context.AnnualKPIs
                .Where(k => k.Year == selectedYear && teamMembers.Select(m => m.Id).Contains(k.UserId))
                .ToListAsync();

            var rankedList = teamMembers
                .Select( member =>
                {
                    var kpi = kpis.FirstOrDefault(k => k.UserId == member.Id);
                    return new
                    {
                        member.Id,
                        FullName = member.FirstName + " " + member.LastName,
                        member.PictureURL,
                        KPI = kpi?.KPI ?? 0
                    };
                })
                .OrderByDescending(m => m.KPI)
                .Select((item, index) => new
                {
                    Rank = index + 1,
                    item.Id,
                    item.FullName,
                    item.PictureURL,
                    item.KPI
                })
                .ToList();

            return Ok(rankedList);
        }

        [HttpGet("admin/team-kpis")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTeamMembersKpisForAdmin([FromQuery] int? year)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            int selectedYear = year ?? DateTime.Now.Year;

            var teamMembers = await context.Users
                .Where(m => m.Role == Role.Member && m.EmailConfirmed && !m.IsDeactivated)
                .ToListAsync();

            if (!teamMembers.Any())
                return NotFound("No team members found.");

            var kpis = await context.AnnualKPIs
                .Where(k => k.Year == selectedYear && teamMembers.Select(m => m.Id).Contains(k.UserId))
                .ToListAsync();

            var rankedList = teamMembers
                .Select(member =>
                {
                    var kpi = kpis.FirstOrDefault(k => k.UserId == member.Id);
                    return new
                    {
                        member.Id,
                        FullName = member.FirstName + " " + member.LastName,
                        member.PictureURL,
                        KPI = kpi?.KPI ?? 0
                    };
                })
                .OrderByDescending(m => m.KPI)
                .Select((item, index) => new
                {
                    Rank = index + 1,
                    item.Id,
                    item.FullName,
                    item.PictureURL,
                    item.KPI
                })
                .ToList();

            return Ok(rankedList);
        }

        [HttpGet("member/my-kpi-rank")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMyKpiWithRank([FromQuery] int? year)
        {
            var member = await userManager.GetUserAsync(User);

            if (member == null)
                return Unauthorized();

            var teamMembers = await context.Users
                .Where(m => m.LeaderID == member.LeaderID && !m.IsDeactivated)
                .ToListAsync();

            if (!teamMembers.Any())
                return NotFound("No team members found.");


            var kpis = await context.AnnualKPIs
                .Where(k => teamMembers.Select(m => m.Id).Contains(k.UserId))
                .ToListAsync();

            var rankedList = teamMembers
                .Select(member =>
                {
                    var kpi = kpis.FirstOrDefault(k => k.UserId == member.Id);
                    return new
                    {
                        member.Id,
                        FullName = member.FirstName + " " + member.LastName,
                        member.PictureURL,
                        KPI = kpi?.KPI ?? 0
                    };
                })
                .OrderByDescending(m => m.KPI)
                .ToList();


            var myRank = rankedList.FindIndex(x => x.Id == member.Id) + 1;
            var myKpi = rankedList.FirstOrDefault(x => x.Id == member.Id);

            if (myKpi == null)
                return NotFound("KPI not found for this member.");

            return Ok(new
            {
                myKpi.FullName,
                myKpi.PictureURL,
                myKpi.KPI,
                Rank = myRank
            });

        }
    }
}
