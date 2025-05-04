using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Models.Requests;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.Data;
using Microsoft.AspNetCore.Authorization;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;
using WebApplicationFlowSync.DTOs;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompleteTaskRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;

        public CompleteTaskRequestsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }


        [HttpPost("create-complete-request")]
        [Authorize(Roles = "Member")] // تأكد من أن الصلاحيات مناسبة حسب دور المستخدم
        public async Task<IActionResult> CreateRequest([FromBody] CompleteTaskRequestDto dto)
        {
            var member = await userManager.GetUserAsync(User);
            if (member == null || !User.IsInRole("Member"))
                return Unauthorized();

            var task = await context.Tasks.FindAsync(dto.FRNNumber);
            if (task == null)
                return NotFound("Task not found.");

            

            //var leader = await context.Users.FindAsync(member.LeaderID);
            //if (leader == null)
            //    return BadRequest("The leader cannot be found.");

            var request = new CompleteTaskRequest
            {
                FRNNumber = task.FRNNumber,
                Notes = dto.Notes,
                MemberName = member.FirstName + " " + member.LastName,
                MemberId = member.Id,
                Email = member.Email,
                RequestedAt = DateTime.UtcNow,
                RequestStatus = RequestStatus.Pending,
                Type = RequestType.CompleteTask
            };

            context.PendingMemberRequests.Add(request);
            await context.SaveChangesAsync();

            return Ok("The task completion request has been sent successfully.");
        }

        [HttpGet("all-complet-requests")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> GetAllCompleteTaskRequests()
        {
            var requests = await context.PendingMemberRequests
                .OfType<CompleteTaskRequest>()
                .Select(r => new
                {
                    r.RequestId,
                    r.MemberName,
                    r.Email,
                    r.RequestedAt,
                    r.RequestStatus,
                    r.Notes,
                    r.FRNNumber
                }).ToListAsync();

            return Ok(requests);
        }

        [HttpPost("approve-complete-task/{requestId}")]
        [Authorize(Roles = "Leader")] // حسب الدور المناسب
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("Only leaders can respond to requests.");
            var request = await context.PendingMemberRequests
                .OfType<CompleteTaskRequest>()
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
                return NotFound("Complete task request not found.");

            var task = await context.Tasks.FirstOrDefaultAsync(t => t.FRNNumber == request.FRNNumber && t.UserID == request.MemberId);

            if (task == null)
                return NotFound("Associated task not found.");


            request.RequestStatus = RequestStatus.Approved;
            task.Type = TaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.Notes = request.Notes;

            await context.SaveChangesAsync();

            return Ok("Complete task request approved and task status updated to Completed.");

        }

    }
}
