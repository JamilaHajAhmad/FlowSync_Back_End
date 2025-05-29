using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.services.NotificationService;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CalenderController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly INotificationService notificationService;

        public CalenderController(ApplicationDbContext context, UserManager<AppUser> userManager, INotificationService notificationService)
        {
            this.context = context;
            this.userManager = userManager;
            this.notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CalendarEventDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Invalid event data.");

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var newEvent = new CalendarEvent
            {
                Title = dto.Title,
                EventDate = dto.EventDate.ToLocalTime(),
                UserID = user.Id,
                ReminderSent1Day = false,
                ReminderSent1Hour = false
            };
            context.CalendarEvents.Add(newEvent);
            await context.SaveChangesAsync();

            return Ok(new { message = "Event created successfully", newEvent.Id });

        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateCalendarEventDto dto)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            var calnderEvent = await context.CalendarEvents
                .FirstOrDefaultAsync(e => e.Id == id && e.UserID == user.Id);

            if (calnderEvent == null)
                return NotFound("Event not found.");

            calnderEvent.Title = dto.Title;
            calnderEvent.EventDate = dto.EventDate.ToLocalTime();

            await context.SaveChangesAsync();

            return Ok(new { message = "Event updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            var calnderEvent = await context.CalendarEvents
                .FirstOrDefaultAsync(e => e.Id == id && e.UserID == user.Id);

            if (calnderEvent == null)
                return NotFound("Event not found.");

            context.CalendarEvents.Remove(calnderEvent);
            await context.SaveChangesAsync();

            return Ok("Event deleted successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> GetMyEvents()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            var events = await context.CalendarEvents
                .Where(e => e.UserID == user.Id)
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.EventDate
                })
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("task-deadlines-events")]
        [Authorize(Roles ="Member")]
        public async Task<IActionResult> GetTaskDeadlinesForCalendar()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var tasks = await context.Tasks
                .Where(t => t.UserID == user.Id)
                .Select(t => new
                {
                    t.Title,
                    t.Deadline,
                    t.FRNNumber,
                    t.Type
                })
                .ToListAsync();

            var calenserEvents = tasks.Select(t => new
            {
                title = $"Deadline: {t.Title} #{t.FRNNumber}",
                start = t.Deadline,
                end = t.Deadline,
                color =t.Type == TaskStatus.Delayed ? "#dc3545" :
                        "#fd7e14"
            });

            return Ok(calenserEvents);
                
        }


        //[HttpPost("send-reminder/{eventId}")]
        //public async Task<IActionResult> SendTestReminder(int eventId)
        //{
        //    var calendarEvent = await context.CalendarEvents
        //        .Include(e => e.User)
        //        .FirstOrDefaultAsync(e => e.Id == eventId);

        //    if (calendarEvent == null || calendarEvent.User == null)
        //        return NotFound("Event or user not found.");

        //    var user = calendarEvent.User;
        //    string message = $"Test Reminder: Event '{calendarEvent.Title}' is scheduled at {calendarEvent.EventDate:HH:mm dd/MM/yyyy}.";

        //    await notificationService.SendNotificationAsync(
        //        user.Id,
        //        message,
        //        NotificationType.Reminder,
        //        user.Email
        //    );

        //    return Ok("✅ Test reminder sent successfully.");
        //}

    }
}
