using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
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
                      $"You have been assigned a new task: {task.Title} (FRN: {task.FRNNumber}) by your leader. Please check your task list.",

                      NotificationType.Info,
                      member.Email,
                      "View Task",
                      "http://localhost:3001/member-tasks"
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


        [HttpPatch("edit-task/{taskId}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> EditTask(int taskId ,EditTaskDto dto)
        {
            // الحصول على المستخدم الحالي
            var leader = await userManager.GetUserAsync(User);

            // التأكد من أن المستخدم هو قائد
            if (leader == null || leader.Role != Role.Leader)
            {
                return Forbid("Only leaders can edit tasks.");
            }

            var task = await context.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return NotFound("Task not found.");
            }

            if (task.User?.LeaderID != leader.Id)
                return Forbid("You are not authorized to edit this task.");

            if (task.Type == TaskStatus.Completed)
            {
                return BadRequest("Cannot edit a completed task.");
            }

            // تحديث الحقول إذا تم إرسالها ما عدا اذا كانت فارغ 
            if (!string.IsNullOrWhiteSpace(dto.FRNNumber))
            {
                // التحقق من عدم وجود مهمة بنفس الرقم الجديد
                bool frnExists = await context.Tasks.AnyAsync(t => t.FRNNumber == dto.FRNNumber && t.Id != taskId);
                if (frnExists)
                    return BadRequest("Another task with the same FRNNumber already exists.");

                task.FRNNumber = dto.FRNNumber;
            }

            if (!string.IsNullOrWhiteSpace(dto.OSSNumber))
                task.OSSNumber = dto.OSSNumber;

            if (!string.IsNullOrWhiteSpace(dto.Title))
                task.Title = dto.Title;

            if (dto.CaseSource != null)
                task.CaseSource = dto.CaseSource.Value;

            if (!string.IsNullOrWhiteSpace(dto.CaseType))
                task.CaseType = dto.CaseType;

            if (dto.Priority != null)
                task.Priority = (TaskPriority)dto.Priority;
            

            if (!string.IsNullOrWhiteSpace(dto.SelectedMemberId))
            {
                var member = await context.Users.FirstOrDefaultAsync(u => u.Id == dto.SelectedMemberId && u.LeaderID == leader.Id);
                if (member == null)
                    return BadRequest("Selected member not found or not under your team.");

                task.UserID = dto.SelectedMemberId;
                await notificationService.SendNotificationAsync(
                    member.Id,
                     $"You have been assigned a new task: {task.Title} (FRN: {task.FRNNumber}) by your leader. Please check your task list.",

                     NotificationType.Info,
                     member.Email,
                     "View Task",
                     "http://localhost:3001/member-tasks"
               );

            }


            try
            {
                await context.SaveChangesAsync();
                //return Ok("Task updated successfully.");
                return Ok(new
                {
                    task.Id,
                    task.FRNNumber,
                    task.OSSNumber,
                    task.Title,
                    task.CaseSource,
                    task.CaseType,
                    task.Priority,
                    task.Type,
                    task.IsDelayed,
                    task.CreatedAt,
                    task.Deadline,
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
                    TaskId = t.Id,
                    TaskTitle = t.Title,
                    FRNNumber = t.FRNNumber,
                    OSSNumber = t.OSSNumber,
                    CaseSource = t.CaseSource,
                    CaseType = t.CaseType,
                    Priority = t.Priority,
                    Status = t.Type,
                    IsDelayed = t.IsDelayed,
                    OpenDate = t.CreatedAt,
                    Deadline = t.Deadline,
                    CompletedAt = t.CompletedAt,
                    FrozenAt = t.FrozenAt,
                    Reason = t.Reason,
                    Notes = t.Notes,
                    Counter = (t.Counter < TimeSpan.Zero ? "-" : "") + t.Counter.Duration().ToString(@"d\.hh\:mm\:ss")
,
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
                    TaskId = t.Id,
                    TaskTitle = t.Title,
                    FRNNumber = t.FRNNumber,
                    OSSNumber = t.OSSNumber,
                    CaseSource = t.CaseSource,
                    CaseType = t.CaseType,
                    Priority = t.Priority,
                    Type = t.Type,
                    IsDelayed = t.IsDelayed,
                    CreatedAt = t.CreatedAt,
                    Deadline = t.Deadline,
                    CompletedAt = t.CompletedAt,
                    FrozenAt = t.FrozenAt,
                    Reason = t.Reason,
                    Notes = t.Notes,
                    Counter = (t.Counter < TimeSpan.Zero ? "-" : "") + t.Counter.Duration().ToString(@"d\.hh\:mm\:ss")
                })
                .ToList();

            return Ok(tasks);
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

            var task = await context.Tasks.FirstOrDefaultAsync(t => t.FRNNumber == dto.FRNNumber);
            if (task == null) return NotFound("Task not found.");

            var fromUser = await userManager.FindByIdAsync(task.UserID);
            if (fromUser == null || fromUser.LeaderID != leader.Id)
                return Forbid("You are not authorized to reassign this task.");

            var toUser = await userManager.Users
                .FirstOrDefaultAsync(u => u.Id == dto.NewMemberId
                    && u.Role == Role.Member
                    && u.LeaderID == leader.Id
                    && !u.IsDeactivated);

            if (toUser == null) return BadRequest("Invalid member selected.");


            task.UserID = toUser.Id;
            await context.SaveChangesAsync();

            await notificationService.SendNotificationAsync(toUser.Id,
                $"You have been assigned a new task {task.Title} (FRN: {task.FRNNumber}) by your leader. Please check your task list.",
                NotificationType.Info,
                toUser.Email,
                "View Task",
                "http://localhost:3001/member-tasks"
                );

            return Ok("Task reassigned successfully.");
        }

   

    }
}
