using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.Models.Requests;
using WebApplicationFlowSync.services.NotificationService;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly INotificationService notificationService;

        public ProfileController(UserManager<AppUser> userManager, ApplicationDbContext context, INotificationService notificationService)
        {
            this.userManager = userManager;
            this.context = context;
            this.notificationService = notificationService;
        }

        [HttpGet("get-profile")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var profile = new
            {
                user.Role,
                user.FirstName,
                user.LastName,
                user.Email,
                user.DateOfBirth,
                user.Address,
                user.Phone,
                user.Major,
                user.Status,
                user.JoinedAt,
                user.Bio,
                user.PictureURL
            };

            return Ok(profile);
        }

        [HttpPatch("edit-profile")]
        [Authorize]
        public async Task<IActionResult> EditProfile([FromBody] EditProfileDto dto)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized("User not found.");

            // تحديث الحقول إذا تم إرسالها حتى لو كانت فارغة
            if (dto.FirstName != null) user.FirstName = dto.FirstName;
            if (dto.LastName != null) user.LastName = dto.LastName;
            if (dto.Email != null) user.Email = dto.Email;
            if (dto.DateOfBirth.HasValue) user.DateOfBirth = dto.DateOfBirth.Value;
            if (dto.Major != null) user.Major = dto.Major;
            if (dto.Address != null) user.Address = dto.Address;
            if (dto.PictureURL != null) user.PictureURL = dto.PictureURL;
            if (dto.Phone != null) user.Phone = dto.Phone;
            if (dto.Bio != null) user.Bio = dto.Bio;

            // محاولة إنشاء طلب تعديل الحالة إن وُجد تعديل في Status
            string? statusMessage = null;

            if (dto.Status != null && dto.Status != user.Status)
            {
                if(user.Role == Role.Member) { 
                var hasUrgentTasks = await context.Tasks
                    .AnyAsync(t => t.UserID == user.Id && t.Priority == TaskPriority.Urgent && t.Type == TaskStatus.Opened);

                    if (hasUrgentTasks)
                    {
                        statusMessage = "Status change request was not submitted because you have urgent tasks.";
                    }
                    else
                    {
                        bool hasExistingRequest = await context.PendingMemberRequests
                            .OfType<ChangeStatusRequest>()
                            .AnyAsync(r => r.NewStatus == dto.Status && r.MemberId == user.Id && r.RequestStatus == RequestStatus.Pending);

                        if (hasExistingRequest)
                        {
                            statusMessage = "You already have a pending request for this status.";
                        }
                        else
                        {
                            var request = new ChangeStatusRequest
                            {
                                MemberId = user.Id,
                                LeaderId = user.LeaderID,
                                Email = user.Email,
                                MemberName = $"{user.FirstName} {user.LastName}",
                                PreviousStatus = user.Status,
                                NewStatus = dto.Status.Value,
                                Type = RequestType.ChangeStatus,
                                RequestedAt = DateTime.Now
                            };

                            context.PendingMemberRequests.Add(request);
                            await context.SaveChangesAsync();

                            await notificationService.SendNotificationAsync(
                                user.LeaderID,
                                $"New request to change status from {user.FirstName} {user.LastName}.",
                                NotificationType.ChangeStatusRequest
                            );

                            statusMessage = "Status change request submitted successfully.";
                        }
                    }
                }
                else if (user.Role == Role.Leader || user.Role == Role.Admin)
                {
                    user.Status = dto.Status.Value;
                }
            }

            bool onlyStatusChanged =
                    dto.FirstName == null &&
                    dto.LastName == null &&
                    dto.Email == null &&
                    !dto.DateOfBirth.HasValue &&
                    dto.Major == null &&
                    dto.Address == null &&
                    dto.PictureURL == null &&
                    dto.Phone == null &&
                    dto.Bio == null &&
                    dto.Status != null;

            if (onlyStatusChanged)
            {
                // تأكد من حفظ التعديل في حالة تغيير الحالة فقط
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return BadRequest("Failed to update profile.");

                return Ok(statusMessage ?? "Status updated successfully.");
            }

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest("Failed to update profile.");

            var responseMessage = "Profile updated successfully.";
            if (statusMessage != null)
                responseMessage += " " + statusMessage;

            return Ok(responseMessage);
        }



        //[HttpPatch("profile-picture")]
        //[Authorize]
        //public async Task<IActionResult> UpdateProfilePicture([FromBody] UpdateProfilePictureDto dto)
        //{
        //    var user = await userManager.GetUserAsync(User);
        //    if (user == null)
        //        return Unauthorized("User not found.");

        //    if (!string.IsNullOrEmpty(dto.PictureURL))
        //    {
        //        user.PictureURL = dto.PictureURL;

        //        var result = await userManager.UpdateAsync(user);
        //        if (!result.Succeeded)
        //            return BadRequest("Failed to update profile picture.");

        //        return Ok(new { message = "Profile picture updated successfully", imageUrl = user.PictureURL });
        //    }

        //    // لم يتم إرسال صورة، لا حاجة للتحديث
        //    return Ok(new { message = "No picture was provided. Profile picture unchanged." });

        //}

    }
}
