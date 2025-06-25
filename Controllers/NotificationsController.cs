using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Models;

namespace WebApplicationFlowSync.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ApplicationDbContext context;

        public NotificationsController(UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyNotifications()
        {
            var user = await userManager.GetUserAsync(User);
            var notifictions = await context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDTO
                {
                    Id = n.Id,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    Type = n.Type
                })
                .ToListAsync();

            return Ok(notifictions);
        }

        [HttpPost("mark-as-read/{notificationId}")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var user = await userManager.GetUserAsync(User);
            var notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == user.Id);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;

            await context.SaveChangesAsync();

            return Ok("Notification marked as read.");
        }

        [HttpPost("mark-all-as-read")]
        [Authorize]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            var notifications = await context.Notifications
                .Where(n => n.UserId == user.Id && !n.IsRead)
                .ToListAsync();

            if (!notifications.Any())
                return Ok("No unread notifications.");

            foreach(var notification in notifications)
            {
                notification.IsRead = true;
            }

            await context.SaveChangesAsync();

            return Ok("All notifications marked as read.");
        }


    }

}
