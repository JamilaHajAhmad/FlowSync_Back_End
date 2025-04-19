using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Models.Requests;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.Data;
using Microsoft.AspNetCore.Authorization;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Leader")]
    public class FreezeTaskRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public FreezeTaskRequestsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllFreezeTaskRequests()
        {
            var requests = await context.PendingMemberRequests
                .OfType<FreezeTaskRequest>()
                .Select(r => new
                {
                    r.MemberName,
                    r.Email,
                    r.RequestedAt,
                    r.RequestStatus,
                    r.FRNNumber,
                    r.Reason
                })
                .ToListAsync();

            return Ok(requests);
        }


        [HttpPost("approve/{requestId}")]
        public async Task<IActionResult> ApproveFreezeTaskRequest(int requestId)
        {
            var request = await context.PendingMemberRequests.OfType<FreezeTaskRequest>()
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null) return NotFound("The request does not exist.");

            request.RequestStatus = RequestStatus.Approved;
            await context.SaveChangesAsync();

            return Ok("The freeze request has been approved.");
        }

        [HttpPost("reject/{requestId}")]
        public async Task<IActionResult> RejectFreezeTaskRequest(int requestId)
        {
            var request = await context.PendingMemberRequests.OfType<FreezeTaskRequest>()
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null) return NotFound("The request does not exist.");

            request.RequestStatus = RequestStatus.Rejected;
            await context.SaveChangesAsync();

            return Ok("The freeze request was rejected.");
        }
    }
}
