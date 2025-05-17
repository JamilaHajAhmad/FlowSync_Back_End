using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.Models.Requests.WebApplicationFlowSync.Models.Requests;
using WebApplicationFlowSync.services.EmailService;
using WebApplicationFlowSync.services.NotificationService;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Leader")]
    public class MemberManagementController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly IEmailService emailService;

        public MemberManagementController(UserManager<AppUser> userManager, ApplicationDbContext context, IEmailService emailService)
        {
            this.userManager = userManager;
            this.context = context;
            this.emailService = emailService;
        }

        [HttpGet("all-members")]
        public async Task<IActionResult> GetAllMembers()
        {
            var users = await userManager.Users
                      .Include(u => u.Tasks) // ضروري تضمين المهام
                      .ToListAsync();

            var members = new List<MemberDto>(); // بدلا من النوع  object حتى يتعرف على خصائص الميمبر عندما قمنا بالفحص في OrderBy

            foreach (var user in users)
            {
                if (user.Role != Role.Member || user.EmailConfirmed == false) continue;

                int activeTasksCount = user.Tasks?
                    .Count(t => t.Type == TaskStatus.Opened) ?? 0;

                members.Add(new MemberDto
                {
                    Id = user.Id,
                    FullName = user.FirstName + " " + user.LastName,
                    Status = user.Status,
                    Email = user.Email,
                    OngoingTasks = activeTasksCount,
                    IsRemoved = user.IsRemoved,
                    PictureURL = user.PictureURL
                    //DeleteEndpoint = $"apiMember/MemberManagementController/delete-member/{user.Id}"
                });

            }
                // ترتيب: غير المحذوفين أولاً، ثم المحذوفين
                var orderedMembers = members
                    .OrderBy( m => m.IsRemoved)
                    .ToList();

                return Ok(orderedMembers);
        }


        [HttpDelete("delete-member/{memberId}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> DeleteMember(string memberId)
        {
            var member = await userManager.Users
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Id == memberId && u.Role == Role.Member);

            if (member == null)
                return NotFound("User not found.");

            var leader = await userManager.GetUserAsync(User);

            if (leader == null || leader.Id != member.LeaderID)
                return Forbid("You are not authorized to remove this member");

            member.IsRemoved = true;
            await context.SaveChangesAsync();

            var emailDto = new EmailDto()
            {
                To = member.Email,
                Subject = "Account Deactivation Notification",
                Body = $@"
                    Dear {member.FirstName},

                    We would like to inform you that your account has been deactivated by your team leader.  
                    You have been removed from the team and you will no longer be able to access the platform.

                    If you believe this was a mistake or have any questions, please contact your team leader directly.

                    Best regards,  
                    FlowSync Team"
            };
            await emailService.sendEmailAsync(emailDto);

            return Ok("Member has been removed successfully.Please reassign his tasks");

        }

        [HttpGet("members-with-ongoing-tasks")]
        public async Task<IActionResult> GetMembersWithOngoingTasks()
        {
            var members = await userManager.Users
                .Where(u => u.Role == Role.Member && u.EmailConfirmed == true && u.Status == UserStatus.On_Duty && !u.IsRemoved)
                .Include(u => u.Tasks)
                .Select(u => new
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    PictureURL = u.PictureURL,
                    OngoingTasks = u.Tasks.Count(t => t.Type == TaskStatus.Opened)
                })
                .ToListAsync();

            return Ok(members);
        }

    }

}
