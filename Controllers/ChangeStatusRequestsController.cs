using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Models.Requests;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.services.NotificationService;
using Microsoft.EntityFrameworkCore;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangeStatusRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly INotificationService notificationService;

        public ChangeStatusRequestsController(ApplicationDbContext context, UserManager<AppUser> userManager, INotificationService notificationService)
        {
            this.context = context;
            this.userManager = userManager;
            this.notificationService = notificationService;
        }

        [HttpPost("approve/{requestId}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> ApproveStatusChange(int requestId)
        {
            var leader = await userManager.GetUserAsync(User);
            if (leader == null)
                return Unauthorized();

            var request = await context.PendingMemberRequests
                .OfType<ChangeStatusRequest>()
                .Include(r => r.Member)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.LeaderId == leader.Id);

            if (request == null)
                return NotFound("The request does not exist.");

            if (request.RequestStatus != RequestStatus.Pending)
                return BadRequest("This request has already been processed.");

            var member = request.Member!;
            var oldStatus = member.Status;

            member.Status = request.NewStatus;
            request.RequestStatus = RequestStatus.Approved;


            await context.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
                member.Id,
                $"Your status change has been approved to {request.NewStatus}.",
                NotificationType.Approval
            );

            return Ok("Approve the request and change the status.");
        }

        [HttpPost("reject/{requestId}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> RejectStatusChange(int requestId)
        {
            var leader = await userManager.GetUserAsync(User);
            if (leader == null)
                return Unauthorized();

            var request = await context.PendingMemberRequests
                .OfType<ChangeStatusRequest>()
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.LeaderId == leader.Id);

            if (request == null)
                return NotFound();

            if (request.RequestStatus != RequestStatus.Pending)
                return BadRequest("This request has already been processed.");

            request.RequestStatus = RequestStatus.Rejected;
            await context.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
                request.MemberId,
                "Your request to change the status has been rejected.",
                NotificationType.Rejection
            );

            return Ok("the request has been rejected.");
        }

        [HttpGet("all-change-status-requests")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> GetAllChangeStatusRequests()
        {
            var leader = await userManager.GetUserAsync(User);
            if (leader == null)
                return Unauthorized();

            var requests = await context.PendingMemberRequests
                .OfType<ChangeStatusRequest>()
                .Where(r => r.LeaderId == leader.Id)
                .Select(r => new
                {
                    r.RequestId,
                    r.MemberId,
                    r.MemberName,
                    r.Email,
                    r.RequestedAt,
                    r.PreviousStatus,
                    r.NewStatus,
                    r.RequestStatus
                })
                .ToListAsync();

            return Ok(requests);
        }
    }
}
