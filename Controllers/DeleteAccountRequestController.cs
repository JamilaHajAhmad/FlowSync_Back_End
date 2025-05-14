using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Models.Requests.WebApplicationFlowSync.Models.Requests;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.services.EmailService;
using WebApplicationFlowSync.services.NotificationService;
using WebApplicationFlowSync.Data;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Models.Requests;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeleteAccountRequestController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly INotificationService notificationService;
        private readonly IEmailService emailService;

        public DeleteAccountRequestController(ApplicationDbContext context, UserManager<AppUser> userManager ,INotificationService notificationService, IEmailService emailService)
        {
            this.context = context;
            this.userManager = userManager;
            this.notificationService = notificationService;
            this.emailService = emailService;
        }

        [HttpGet("all-delete-account-requests")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> GetAllDeleteAccountRequests()
        {
            var requests = await context.PendingMemberRequests
                .OfType<DeleteAccountRequest>()
                .Select(r => new
                {
                    r.RequestId,
                    r.MemberId,
                    r.MemberName,
                    r.Email,
                    r.RequestedAt,
                    r.RequestStatus,
                    r.Reason
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost("approve-delete-member-request/{requestId}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> AprroveDeleteAccountRequest(int requestId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("Only leaders can respond to requests.");

            var request = await context.PendingMemberRequests
                .OfType<DeleteAccountRequest>()
                .Include(r => r.Member)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
                return NotFound("Delete account request not found.");

            if (request.RequestStatus != RequestStatus.Pending)
                return BadRequest("This request has already been handled.");

            request.RequestStatus = RequestStatus.Approved;

            var member = request.Member;

            member.IsRemoved = true;
            await userManager.UpdateAsync(member);
            await context.SaveChangesAsync();


            await notificationService.SendNotificationAsync(
                member.Id,
                    $@"
                    Dear {member.FirstName},

                    Your request to delete your account has been approved by your team leader.
                    As of now, your account has been deactivated and you will no longer be able to log in.

                    If you believe this was a mistake or you have further questions, please contact your team leader.

                    Best regards,  
                    FlowSync Team",
                    NotificationType.Info,
                    member.Email,
                    null,
                    null,
                    false
           );

            return Ok("The member's account has been deactivated successfully.Please reassign his tasks");

        }

        [HttpPost("reject-delete-member-request/{requestId}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> RejectDeleteAccountRequest(int requestId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("Only leaders can respond to requests.");

            var request = await context.PendingMemberRequests
                .OfType<DeleteAccountRequest>()
                .Include(r => r.Member)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
                return NotFound("Delete account request not found.");

            if (request.RequestStatus != RequestStatus.Pending)
                return BadRequest("This request has already been handled.");

            var member = request.Member;

            request.RequestStatus = RequestStatus.Rejected;
            await context.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
             request.MemberId,
             $"Your Delete aacount request has been rejected.",
             NotificationType.Rejection,
             member.Email
           );

            return Ok("Delete account request has been rejected.");

        }
    }
}
