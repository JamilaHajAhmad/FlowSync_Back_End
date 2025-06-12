using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Models.Requests;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebApplicationFlowSync.DTOs;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using WebApplicationFlowSync.services.NotificationService;
using Microsoft.Graph.Models;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FreezeTaskRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly INotificationService notificationService;

        public FreezeTaskRequestsController(ApplicationDbContext context, UserManager<AppUser> userManager , INotificationService notificationService)
        {
            this.context = context;
            this.userManager = userManager;
            this.notificationService = notificationService;
        }

        [HttpPost("create-freeze-request")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> SubmitFreezeRequest([FromBody] FreezeTaskRequestDto dto)
        {
            try
            {

                var member = await userManager.GetUserAsync(User);
                if (member == null || !User.IsInRole("Member"))
                    return Unauthorized();

                var task = await context.Tasks.FirstOrDefaultAsync(t=> t.FRNNumber == dto.FRNNumber);
                if (task == null || task.UserID != member.Id)
                    throw new Exception("Invalid task.");


                bool hasExistingFreezeRequest = await context.PendingMemberRequests.OfType<FreezeTaskRequest>()
                 .AnyAsync(r =>
                  r.FRNNumber == dto.FRNNumber &&
                  r.MemberId == member.Id &&
                  r.RequestStatus == RequestStatus.Pending);

                if (hasExistingFreezeRequest)
                    return BadRequest("You already have a pending freeze request for this task.");


                var request = new FreezeTaskRequest
                {
                    FRNNumber = dto.FRNNumber,
                    Reason = dto.Reason,
                    MemberId = member.Id,
                    MemberName = member.FirstName + " " + member.LastName,
                    Email = member.Email,
                    RequestedAt = DateTime.Now,
                    RequestStatus = RequestStatus.Pending,
                    Type = RequestType.FreezeTask,
                    LeaderId = member.LeaderID
                };


                context.PendingMemberRequests.Add(request);
                await context.SaveChangesAsync();

                await notificationService.SendNotificationAsync(
                member.LeaderID,
                    $"Member {member.FirstName} {member.LastName} has submitted a freeze request for task #{dto.FRNNumber}.",
                   NotificationType.FreezeTaskRequest);

                return Ok("Request submitted, wait for leader approval");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString()); // هذا سيطبع الاستثناء الكامل مع InnerException
            }
        }



        [HttpPost("approve/{requestId}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> ApproveFreezeRequest(int requestId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("Only leaders can respond to requests.");

            // جلب الطلب من نوع FreezeTaskRequest
            var request = await context.PendingMemberRequests
                .OfType<FreezeTaskRequest>()
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
            if (request == null)
                return NotFound("Freeze task request not found.");

            // التأكد من أن الطلب لم تتم معالجته سابقًا
            if (request.RequestStatus != RequestStatus.Pending)
                throw new Exception("This request has already been processed.");

            // تحديث معلومات الطلب
            request.RequestStatus = RequestStatus.Approved;
            //request.LeaderId = user.Id;

            // جلب التاسك المرتبط باستخدام TaskId
            var task = await context.Tasks
                .FirstOrDefaultAsync(t => t.FRNNumber == request.FRNNumber && t.UserID == request.MemberId);

            if (task == null)
                return NotFound("Associated task not found.");

            // تغيير حالة المهمة إلى مجمدة
            task.Type = TaskStatus.Frozen;
            task.FrozenAt = DateTime.Now;
            task.Reason = request.Reason;


            //// تخزين الوقت المتبقي حتى الموعد النهائي وقت التجميد
            //var timeRemaining = task.Deadline - DateTime.Now;
            ////يمكن أن يحصل خطأ الحفظ لو أصبحت القيمة سالبة في لحظة الحفظ.
            //task.FrozenCounter = timeRemaining > TimeSpan.Zero ? timeRemaining : TimeSpan.Zero;

            try
            {
                var timeLeft = task.Deadline - DateTime.Now;
                task.FrozenCounterValue = timeLeft > TimeSpan.Zero ? timeLeft : TimeSpan.Zero;

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }

            //await context.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
                request.MemberId,
                $"Your freeze request for task #{request.FRNNumber} has been approved.",
                NotificationType.Approval
            );

            return Ok("Freeze request approved and task status updated to Frozen.");
        }

        [HttpPost("reject/{requestId}")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> RejectFreezeRequest(int requestId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("Only leaders can respond to requests.");

            // جلب الطلب من نوع FreezeTaskRequest
            var request = await context.PendingMemberRequests
                .OfType<FreezeTaskRequest>()
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
                return NotFound("Freeze task request not found.");

            // التأكد من أن الطلب لم تتم معالجته مسبقًا
            if (request.RequestStatus != RequestStatus.Pending)
                return BadRequest("This request has already been processed.");

            // تحديث حالة الطلب إلى مرفوض
            request.RequestStatus = RequestStatus.Rejected;
            request.LeaderId = user.Id;

            await context.SaveChangesAsync();

            await notificationService.SendNotificationAsync(
              request.MemberId,
              $"Your freeze request for task #{request.FRNNumber} has been rejected.",
              NotificationType.Rejection
            );

            return Ok("Freeze request has been rejected.");
        }



        [HttpGet("all-freeze-requests")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> GetAllFreezeTaskRequests()
        {
            var requests = await context.PendingMemberRequests
                .OfType<FreezeTaskRequest>()
                .Select(r => new
                {
                    r.RequestId,
                    r.MemberName,
                    r.Email,
                    r.RequestedAt,
                    r.RequestStatus,
                    r.FRNNumber,
                    r.Reason
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost("unfreeze-task")]
        [Authorize (Roles ="Member")]
        public async Task<IActionResult> UnfreezeTask([FromBody]UnfreezeTaskDto dto)
        {
            var member = await userManager.GetUserAsync(User);
            if (member == null)
                return Unauthorized();

            var task = await context.Tasks.FirstOrDefaultAsync(t => t.FRNNumber == dto.FRNNumber && t.Type == TaskStatus.Frozen);
            if (task == null)
                return NotFound("task can not be found.");

            // حساب الموعد الجديد 
            if (task.FrozenCounterValue.HasValue)
            {
                task.Deadline = DateTime.Now + task.FrozenCounterValue.Value;
            }

            // إزالة التجميد
            task.Type = TaskStatus.Opened;
            task.FrozenAt = null;
            task.FrozenCounter = null;//إعادة الترتيب لحذف FrozenCounter بعد الانتهاء من استخدامها.
            task.Reason = null;
  
            await context.SaveChangesAsync();

           return Ok("Task has been unfrozen successfully");
        }

    }
}
