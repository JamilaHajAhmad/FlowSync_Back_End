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
    public class CompleteTaskRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public CompleteTaskRequestsController(ApplicationDbContext context)
        {
            this.context = context;
        }
        [HttpGet("all-complete-requests")]
        public async Task<IActionResult> GetAllCompleteTaskRequests()
        {
            var requests = await context.PendingMemberRequests
                .OfType<CompleteTaskRequest>()
                .Select(r => new
                {
                    r.MemberName,
                    r.Email,
                    r.RequestedAt,
                    r.RequestStatus,
                    r.FRNNumber,
                    r.Notes
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost("approve/{requestId}")]
        public async Task<IActionResult> ApproveCompleteTaskRequest(int requestId)
        {
            var request = await context.PendingMemberRequests.OfType<CompleteTaskRequest>()
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
            if (request == null) return NotFound("The request does not exist.");

            request.RequestStatus = RequestStatus.Approved;
            await context.SaveChangesAsync();

            return Ok("The task completion request has been approved.");
        }

        [HttpPost("reject/{requestId}")]
        public async Task<IActionResult> RejectCompleteTaskRequest(int requestId)
        {
            var request = await context.PendingMemberRequests.OfType<CompleteTaskRequest>()
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
            if (request == null) return NotFound("The request does not exist.");

            request.RequestStatus = RequestStatus.Rejected;
            await context.SaveChangesAsync();

            return Ok("The task completion request was rejected.");
        }
    }
}
