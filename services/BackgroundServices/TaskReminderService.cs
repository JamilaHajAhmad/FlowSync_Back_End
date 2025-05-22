
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.services.EmailService;
using WebApplicationFlowSync.services.NotificationService;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.services.BackgroundServices
{
    public class TaskReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<TaskReminderService> logger;

        public TaskReminderService(IServiceScopeFactory serviceScopeFactory, ILogger<TaskReminderService> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }
        protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = serviceScopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var now = DateTime.Now;

                        var delayedTasks = await context.Tasks
                            .Include(t => t.User)
                            .Where(t => t.Type == TaskStatus.Opened && now > t.Deadline && !t.IsDelayed)
                            .ToListAsync();

                        foreach (var task in delayedTasks)
                        {
                            logger.LogInformation($"Task {task.FRNNumber} is delayed. Updating status.");
                            task.Type = TaskStatus.Delayed;
                            task.IsDelayed = true;

                            await notificationService.SendNotificationAsync(
                                task.UserID,
                                $"You have been marked as delayed in delivering task #{task.FRNNumber}. Please follow up.",
                                NotificationType.Warning
                            );

                            var leaderId = task.User.LeaderID;
                            if (!string.IsNullOrEmpty(leaderId))
                            {
                                await notificationService.SendNotificationAsync(
                                    leaderId,
                                    $"Your team member {task.User.FirstName} {task.User.LastName} has a delayed task (#{task.FRNNumber}).",
                                    NotificationType.Warning
                                );
                            }
                        }

                        await context.SaveChangesAsync();

                        var today = now.Date;
                        var reminderTasks = await context.Tasks
                            .Include(t => t.User)
                            .Where(t => t.Type == TaskStatus.Opened &&
                                t.Deadline.Date == today.AddDays(2) &&
                                !t.IsDelayed)
                            .ToListAsync();

                        foreach (var task in reminderTasks)
                        {
                            var fullName = $"{task.User.FirstName} {task.User.LastName}";
                            var emailDto = new EmailDto
                            {
                                To = task.User.Email,
                                Subject = $"Reminder: Task #{task.FRNNumber} Deadline Approaching",
                                Body = $"Dear {fullName},<br/><br/>This is a reminder that your task <strong>#{task.FRNNumber}</strong> is due in 2 days (Deadline: {task.Deadline:yyyy-MM-dd}).<br/>Please make sure to complete it before the deadline."
                            };
                            await emailService.sendEmailAsync(emailDto);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // تسجيل الخطأ في الـ logs
                    logger.LogError(ex, "An error occurred while executing TaskReminderService.");

                }

                //await System.Threading.Tasks.Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
