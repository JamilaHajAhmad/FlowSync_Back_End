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
                task.SetDeadline();
                context.Tasks.Add(task);
                await context.SaveChangesAsync();


                await notificationService.SendNotificationAsync(
                     member.Id,
                      $"You have been assigned a new task: {task.Title} (FRN: {task.FRNNumber}).",
                      NotificationType.Info,
                      member.Email,
                      "View Task",
                      "http://localhost:3002/member-tasks"
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

            //LINQ to Entities لا يفهم كيف يترجم Counter إلى SQL
            var taskList = await query.ToListAsync(); // تم تحويلها الى list حتى نستطيع عرض ال Counter لانها NotMapped

            var tasks = taskList
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
                    Deadline = t.Deadline,
                    CompletedAt = t.CompletedAt,
                    FrozenAt = t.FrozenAt,
                    Reason = t.Reason,
                    Notes = t.Notes,
                    Counter = t.Counter.ToString(),
                    AssignedMember = new
                    {
                        Id = t.User.Id,
                        FullName = t.User.FirstName + " " + t.User.LastName
                    }
                })
                .ToList();

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

            var taskList =await query.ToListAsync();

            var tasks = taskList
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
                    Deadline = t.Deadline,
                    CompletedAt = t.CompletedAt,
                    FrozenAt = t.FrozenAt,
                    Reason = t.Reason,
                    Notes = t.Notes,
                    Counter = t.Counter.ToString()
                })
                .ToList();

            return Ok(tasks);
        }

        [HttpPost("mark-delayed-task")]
        public async Task<IActionResult> ConvertToDelayed([FromBody] DelayTaskDto dto)
        {
            var task = await context.Tasks
                .Include(t => t.User) // ضروري حتى نستطيع الوصول إلى بيانات المستخدم وقائده
                .FirstOrDefaultAsync(t => t.FRNNumber == dto.FRNNumber);

            if (task == null)
                return NotFound("task can not be found.");

            if (DateTime.Now > task.Deadline && task.Type == TaskStatus.Opened)
            {
                task.Type = TaskStatus.Delayed;
                task.IsDelayed = true;
                await context.SaveChangesAsync();

                await notificationService.SendNotificationAsync(
                    task.UserID,
                      $"You have been marked as delayed in delivering task #{task.FRNNumber}. Please follow up.",
                      NotificationType.Warning
                );

                var leaderId = task.User.LeaderID;


                await notificationService.SendNotificationAsync(
                   leaderId,
                   $"Your team member {task.User.FirstName} {task.User.LastName} has a delayed task (#{task.FRNNumber}).",
                   NotificationType.Warning
                  );

                return Ok("The task status has been changed to delayed.");

            }
            else
            {
                return BadRequest("Task is not eligible to be marked as delayed.");
            }
           
        }


        [HttpGet("get-member-tasks-to-reassign/{memberId}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> GetTasksToReassign(string memberId)
        {
            var leader = await userManager.GetUserAsync(User);
            var member = await userManager.Users
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Id == memberId && u.Role == Role.Member);

            if (member == null || leader == null || member.LeaderID != leader.Id)
                return NotFound("Member not found or you are not authorized.");

            var filteredTasks = member.Tasks?
                .Where(t => t.Type != TaskStatus.Completed)
                .ToList();

            var groupedTasks = filteredTasks
                .GroupBy(t => t.Type.ToString()) // اسم النوع كمفتاح (مثلاً: "Opened")
                .ToDictionary(
                      g => g.Key,    // تحديد key لكل عنصر
                      g => g.Select(t => new    // تحديد القيمة لكل key
                      {
                          t.FRNNumber,
                          t.OSSNumber,
                          t.Title,
                          t.Type,
                          t.Priority,
                          t.CreatedAt,
                          t.Deadline
                      }).ToList()
                );

             return Ok(groupedTasks);
        }

        [HttpPost("reassign-task")]
        [Authorize (Roles = "Leader")]
        public async Task<IActionResult> ReassignTask([FromBody] ReassignTaskDto dto)
        {
            var leader = await userManager.GetUserAsync(User);

            var task = await context.Tasks.FindAsync(dto.FRNNumber);
            if (task == null) return NotFound("Task not found.");

            var fromUser = await userManager.FindByIdAsync(task.UserID);
            if (fromUser == null || fromUser.LeaderID != leader.Id)
                return Forbid("You are not authorized to reassign this task.");

            var toUser = await userManager.Users
                .FirstOrDefaultAsync(u => u.Id == dto.NewMemberId
                    && u.Role == Role.Member
                    && u.LeaderID == leader.Id
                    && !u.IsRemoved);

            if (toUser == null) return BadRequest("Invalid member selected.");

            task.UserID = toUser.Id;
            await context.SaveChangesAsync();

            return Ok("Task reassigned successfully.");
        }

   

    }
}
