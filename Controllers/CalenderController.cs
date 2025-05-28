using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Authorize]
    public class CalenderController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;

        public CalenderController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
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
                EventDate = dto.EventDate,
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
                color = t.Type == TaskStatus.Completed ? "#28a745" :
                        t.Type == TaskStatus.Delayed ? "#dc3545" :
                        "#007bff"
            });

            return Ok(calenserEvents);
                
        }

    }
}
