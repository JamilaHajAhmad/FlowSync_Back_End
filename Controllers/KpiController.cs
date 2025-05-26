using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            var kpi = await kpiService.CalculateMemberAnnualKPIAsync(member.Id, year);

            return Ok(new
            {
                KPI = Math.Round(kpi, 2)
            });
        }

        [HttpGet("leader/annual-kpi")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> GetLeaderAnnualKpi()
        {
            var leader = await userManager.GetUserAsync(User);
            if (leader == null || leader.Role != Role.Leader)
                return NotFound("Leader not found.");

            int year = DateTime.UtcNow.Year;
            var kpi = await kpiService.CalculateLeaderAnnualKPIAsync(leader.Id, year);

            return Ok(new
            {
                KPI = Math.Round(kpi, 2)
            });
        }
    }
}
