
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Models;
using Task = System.Threading.Tasks.Task;

namespace WebApplicationFlowSync.services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext context;

        public NotificationService(ApplicationDbContext context) 
        {
            this.context = context;
        }

        public async Task SendNotificationAsync(string userId, string message , NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            };
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();
        }
    }
}
