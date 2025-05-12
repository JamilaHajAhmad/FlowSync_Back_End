using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
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
                    FullName= user.FirstName + " " + user.LastName,
                    user.Status,
                    user.Email,
                    OngoingTasks = activeTasksCount,
                    user.PictureURL
                    //DeleteEndpoint = $"apiMember/MemberManagementController/delete-member/{user.Id}"
                });
            }

            return Ok(members);
        }

        [HttpDelete("delete-member/{memberId}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> DeleteMember(string memberId)
        {
            var member = await userManager.Users
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Id == memberId &&  u.Role == Role.Member);

                if (member == null)
                return NotFound("User not found.");

            var leader = await userManager.GetUserAsync(User);

            if (leader == null || leader.Id != member.LeaderID)
                return Forbid("You are not authorized to remove this member");

            member.IsRemoved = true;
            await context.SaveChangesAsync();

            return Ok("Member has been removed successfully.Please reassign his tasks");

        }

        [HttpGet("members-with-ongoing-tasks")]
        public async Task<IActionResult> GetMembersWithOngoingTasks()
        {
            var members = await userManager.Users
                .Where(u => u.Role == Role.Member && u.EmailConfirmed == true && !u.IsRemoved)
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
