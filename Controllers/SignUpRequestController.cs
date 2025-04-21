using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.Models.Requests;
using WebApplicationFlowSync.services.EmailService;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Leader")]
    public class SignUpRequestController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly IEmailService emailService;

        public SignUpRequestController(UserManager<AppUser> userManager, ApplicationDbContext context, IEmailService emailService)
        {
            this.userManager = userManager;
            this.context = context;
            this.emailService = emailService;
        }


        [HttpGet("all-signup-requests")]
        public async Task<IActionResult> GetAllSignUpRequests()
        {
            var requests = await context.PendingMemberRequests
                .OfType<SignUpRequest>()
                .Select(r => new
                {
                    r.Id,
                    r.MemberName,
                    r.Email,
                    r.RequestedAt,
                    r.RequestStatus,
                })
                .ToListAsync();

            return Ok(requests);
        }

        // موافقة القائد على العضو
        [HttpPost("approve-member/{requestId}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> ApproveMember(int requestId)
        {
            var pendingRequest = await context.PendingMemberRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (pendingRequest == null)
            {
                throw new Exception("Membership request not found.");
            }

            var currentLeader = await userManager.GetUserAsync(User);
            //Console.WriteLine($"Current user: {currentUser?.UserName}");  // لازم يظهر اسم
            if (currentLeader == null)
            {
                throw new Exception("User identity not verified (member information not found in the request)");
            }

            if (pendingRequest.LeaderId == null)
            {
                //return BadRequest("الطلب لا يحتوي على معرف القائد.");
                throw new Exception("The request does not contain a leader ID.");
            }

            // الموافقة على الطلب
            pendingRequest.RequestStatus = RequestStatus.Approved;
            await context.SaveChangesAsync(); // حفظ التغييرات في قاعدة البيانات

            // إرسال بريد إلكتروني للميمبر بعد الموافقة
            var member = await userManager.FindByIdAsync(pendingRequest.MemberId);

            // الربط بين القائد والعضو
            member.LeaderID = currentLeader.Id;
            context.SaveChanges();
            member.JoinedAt = DateTime.UtcNow;
            context.SaveChanges();

            if (member != null)
            {
                var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(member);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = member.Id, token = confirmationToken }, Request.Scheme);

                await emailService.SendConfirmationEmail(member.Email, "The Leader has accepted your request to join the Team. Confirm your account as a Member.", confirmationLink);
            }

            return Ok("Membership has been successfully approved, please check your email.");
        }


        [HttpPost("reject-member/{requestId}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> RejectMember(int requestId)
        {
            var pendingRequest = await context.PendingMemberRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (pendingRequest == null)
            {
                //return NotFound("طلب العضوية غير موجود.");
                throw new Exception("Membership request not found.");
            }

            // التأكد من أن القائد هو من يرفض
            var currentUser = await userManager.GetUserAsync(User);
            if (pendingRequest.LeaderId != currentUser.Id)
            {
                //return Unauthorized("أنت لست القائد المعني.");
                throw new Exception("You are not the leader required in the request.");
            }

            var member = await userManager.FindByIdAsync(pendingRequest.MemberId);
            // رفض الطلب
            pendingRequest.RequestStatus = RequestStatus.Rejected;
            context.SaveChanges();
            //حذف الطلب 
            //context.PendingMemberRequests.Remove(pendingRequest);
            await context.SaveChangesAsync(); /// نحفظ هنا قبل حذف العضو
            // حذف المستخدم من النظام
            if (member != null)
            {
                await userManager.DeleteAsync(member);
                await context.SaveChangesAsync();
            }

            return Ok("The request was rejected and the member was removed from the system.");
        }
    }
}
