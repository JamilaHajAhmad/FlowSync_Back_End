using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Models;
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

        public MemberManagementController(UserManager<AppUser> userManager , ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        [HttpGet("all-members")]
        public async Task<IActionResult> GetAllMembers()
        {
            var users = await userManager.Users
                      .Include(u => u.Tasks) // ضروري تضمين المهام
                      .ToListAsync();

            var members = new List<object>();

            foreach (var user in users)
            {
                if (user.Role != Role.Member || user.EmailConfirmed== false) continue;

                int activeTasksCount = user.Tasks?
                    .Count(t => t.Type == TaskStatus.Opened) ?? 0;

                members.Add(new
                {
                    user.Id,
                    FullName= user.FirstName + "" + user.LastName,
                    user.Status,
                    user.Email,
                    OngoingTasks = activeTasksCount,
                    DeleteEndpoint = $"apiMember/MemberManagementController/delete-member/{user.Id}"
                });
            }

            return Ok(members);
        }

        [HttpDelete("delete-member/{memberId}")]
        public async Task<IActionResult> DeleteMember(string memberId)
        {
            var member = await userManager.FindByIdAsync(memberId);

            if (member == null)
                return NotFound("User not found.");

            if (member.Role != Role.Member)
                throw new Exception("A user who is not a member cannot be deleted.");

            // حذف العضو
            var result = await userManager.DeleteAsync(member);
            context.SaveChanges();
            if (!result.Succeeded)
                throw new Exception("An error occurred while trying to delete member.");

            return Ok("Member has been successfully deleted.");
        }

        [HttpGet("members-with-ongoing-tasks")]
        public async Task<IActionResult> GetMembersWithOngoingTasks()
        {
            var members = await userManager.Users
                .Where(u => u.Role == Role.Member && u.EmailConfirmed == true)
                .Include(u => u.Tasks)
                .Select(u => new
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    OngoingTasks = u.Tasks.Count(t => t.Type == TaskStatus.Opened)
                })
                .ToListAsync();

            return Ok(members);
        }
    }

}
