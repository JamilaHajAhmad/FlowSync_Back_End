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
    public class TaskManagementController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly INotificationService notificationService;

        public TaskManagementController(UserManager<AppUser> userManager, ApplicationDbContext context, INotificationService notificationService)
        {
            this.userManager = userManager;
            this.context = context;
            this.notificationService = notificationService;
        }

        // إنشاء تاسك جديد
        [HttpPost("create-new-task")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto model)
        {
            // الحصول على المستخدم الحالي
            var currentUser = await userManager.GetUserAsync(User);

            // التأكد من أن المستخدم هو قائد
            if (currentUser == null || currentUser.Role != Role.Leader)
            {
                return Forbid("Only leaders can create tasks.");
            }

            // التأكد من أن الـ DTO المرسل صحيح
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // idالبحث عن العضو بناءً على ال

            var member = await context.Users
             .FirstOrDefaultAsync(u => u.Id == model.SelectedMemberId && u.LeaderID == currentUser.Id && u.Role == Role.Member);


            if (member == null)
            {
                return NotFound("Member not found or not assigned to your team.");
            }

            // التحقق من تأكيد البريد الإلكتروني للعضو
            if (!member.EmailConfirmed)
            {
                throw new Exception("The selected member has not confirmed their email address and cannot be assigned a task.");
            }

            // إنشاء التاسك
            var task = new Models.Task
            {
                FRNNumber = model.FRNNumber,
                OSSNumber = model.OSSNumber,
                Title = model.Title,
                CaseSource = model.CaseSource, // تحويل enum إلى string
                CaseType = model.CaseType,
                Priority = model.Priority,
                Type = TaskStatus.Opened, // تحويل النوع بناءً على الـ DTO
                CreatedAt = DateTime.Now,
                UserID = member.Id
            };

            // إضافة التاسك إلى قاعدة البيانات
            try
            {
                context.Tasks.Add(task);
                await context.SaveChangesAsync();


                await notificationService.SendNotificationAsync(
                     member.Id,
                      $"You have been assigned a new task: {task.Title} (FRN: {task.FRNNumber}).",
                      NotificationType.Info,
                      member.Email
                );

                return Ok(new
                {
                    Message = "Task created successfully.",
                    TaskId = task.FRNNumber,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }
        [HttpGet("all-tasks")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> GetAllTasks([FromQuery] TaskStatus? type)
        {
            var query = context.Tasks
                .Include(t => t.User)
                .AsQueryable();

            // فلترة حسب نوع المهمة إذا تم تمرير قيمة
            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type.Value);
            }

            var tasks = await query
                .Select(t => new
                {
                    TaskTitle = t.Title,
                    FRNNumber = t.FRNNumber,
                    OSSNumber = t.OSSNumber,
                    CaseSource = t.CaseSource,
                    CaseType = t.CaseType,
                    Priority = t.Priority,
                    Status = t.Type,
                    OpenDate = t.CreatedAt,
                    CompletedAt = t.CompletedAt,
                    FrozenAt = t.FrozenAt,
                    Reason = t.Reason,
                    Notes = t.Notes,
                    AssignedMember = new
                    {
                        Id = t.User.Id,
                        FullName = t.User.FirstName + " " + t.User.LastName
                    }
                })
                .ToListAsync();

            return Ok(tasks);
        }


        [HttpGet("member-tasks")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMemberTasks([FromQuery] TaskStatus? type)
        {
            var currentUser = await userManager.GetUserAsync(User);

            if (currentUser == null || currentUser.Role != Role.Member)
            {
                return Forbid("Only members can access their tasks.");
            }

            var query = context.Tasks
                .Where(t => t.UserID == currentUser.Id)
                .AsQueryable();

            // إذا تم تمرير النوع، فلتر عليه
            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type.Value);
            }

            var tasks = await query
                .Select(t => new
                {
                    TaskTitle = t.Title,
                    FRNNumber = t.FRNNumber,
                    OSSNumber = t.OSSNumber,
                    CaseSource = t.CaseSource,
                    CaseType = t.CaseType,
                    Priority = t.Priority,
                    Type = t.Type,
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt,
                    FrozenAt = t.FrozenAt,
                    Reason = t.Reason,
                    Notes = t.Notes
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpPost("mark-delayed-task")]
        public async Task<IActionResult> ConvertToDelayed([FromBody] DelayTaskDto dto)
        {
            var task = await context.Tasks.FirstOrDefaultAsync(t => t.FRNNumber == dto.FRNNumber);

            if (task == null)
                return NotFound("task can not be found.");

            task.Type = TaskStatus.Delayed;
            await context.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
                task.UserID,
                  $"You have been marked as delayed in delivering task #{task.FRNNumber}. Please follow up.",
                  NotificationType.Warning
            );

            return Ok("The task status has been changed to delayed.");
        }

    }
}
