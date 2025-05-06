
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.services.EmailService;
using Task = System.Threading.Tasks.Task;

namespace WebApplicationFlowSync.services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext context;
        private readonly IEmailService emailService;

        public NotificationService(ApplicationDbContext context , IEmailService emailService) 
        {
            this.context = context;
            this.emailService = emailService;
        }

        public async Task SendNotificationAsync(string userId, string message , NotificationType type , string email = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.Now,
            };
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            // إرسال الإشعار عبر البريد الإلكتروني إذا تم تمرير البريد الإلكتروني
            if(!string.IsNullOrEmpty(email))
            {
                var htmlBody = EmailTemplateBuilder.BuildTemplate(
                    "New Notification",
                    message
                );
                var emailDto = new EmailDto()
                {
                    To = email,
                    Subject = $"New Notification: {type}",
                    Body = htmlBody
                };

                await emailService.sendEmailAsync(emailDto);
            }
        }
    }
}
