
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.services.EmailService;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.services.BackgroundServices
{
    public class TaskReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;

        public TaskReminderService(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested)
            {
                //في الـ BackgroundService، عليك إنشاء scope بنفسك.
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var today = DateTime.Now.Date;

                    var tasks = await context.Tasks
                        .Include(t => t.User)
                        .Where(t => t.Type == TaskStatus.Opened &&
                            t.Deadline == today.AddDays(2) &&
                            !t.IsDelayed)
                        .ToListAsync();

                    foreach(var task in tasks) {

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

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

            }
              
        }
    }
}
