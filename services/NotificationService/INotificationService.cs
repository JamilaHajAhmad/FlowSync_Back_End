using WebApplicationFlowSync.Models;
using Task = System.Threading.Tasks.Task;

namespace WebApplicationFlowSync.services.NotificationService
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message , NotificationType type ,string email = null);
    }
}
