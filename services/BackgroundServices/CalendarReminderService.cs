
//using Microsoft.EntityFrameworkCore;
//using WebApplicationFlowSync.Data;
//using WebApplicationFlowSync.Models;
//using WebApplicationFlowSync.services.NotificationService;
//using Task = System.Threading.Tasks.Task;

//namespace WebApplicationFlowSync.services.BackgroundServices
//{
//    public class CalendarReminderService : BackgroundService
//    {
//        private readonly IServiceScopeFactory serviceScopeFactory;
//        private readonly ILogger<CalendarReminderService> logger;

//        public CalendarReminderService(IServiceScopeFactory serviceScopeFactory , ILogger<CalendarReminderService> logger)
//        {
//            this.serviceScopeFactory = serviceScopeFactory;
//            this.logger = logger;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    using (var scope = serviceScopeFactory.CreateScope())
//                    {
//                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

//                        var now = DateTime.Now;

//                        var upcomingEvents = await context.CalendarEvents
//                            .Include(e => e.User)
//                            .Where(e =>
//                            e.EventDate > now &&
//                            e.EventDate <= now.AddHours(25) &&
//                            (!e.ReminderSent1Day || !e.ReminderSent1Hour))
//                            .ToListAsync(stoppingToken);

//                        foreach (var calendarEvent in upcomingEvents)
//                        {
//                            var user = calendarEvent.User;
//                            if (user == null) continue;

//                            var timeUntilEvent = calendarEvent.EventDate - now;

//                            // 1-day reminder at the same time of day
//                            if (!calendarEvent.ReminderSent1Day &&
//                                timeUntilEvent.TotalHours <= 24.1 && timeUntilEvent.TotalHours >= 23.9)
//                            {
//                                string message = $"Reminder: You have an event '{calendarEvent.Title}' tomorrow at {calendarEvent.EventDate:HH:mm}.";
//                                await notificationService.SendNotificationAsync(
//                                    user.Id,
//                                    message,
//                                    NotificationType.Reminder,
//                                    email: user.Email,
//                                    linkText: "View Event",
//                                    linkUrl: "/calendar"
//                                );

//                                calendarEvent.ReminderSent1Day = true;
//                                logger.LogInformation($"1-day reminder sent to {user.Email} for event '{calendarEvent.Title}'.");
//                            }
//                            // 1-hour reminder before the event
//                            if (!calendarEvent.ReminderSent1Hour &&
//                                timeUntilEvent.TotalMinutes <= 61 && timeUntilEvent.TotalMinutes >= 59)
//                            {
//                                string message = $"Reminder: Your event '{calendarEvent.Title}' will start in 1 hour (at {calendarEvent.EventDate:HH:mm}).";
//                                await notificationService.SendNotificationAsync(
//                                    user.Id,
//                                    message,
//                                    NotificationType.Reminder,
//                                    email: user.Email,
//                                    linkText: "View Event",
//                                    linkUrl: "/calendar"
//                                );

//                                calendarEvent.ReminderSent1Hour = true;
//                                logger.LogInformation($"1-hour reminder sent to {user.Email} for event '{calendarEvent.Title}'.");
//                            }
//                        }
//                        await context.SaveChangesAsync(stoppingToken);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "An error occurred in CalendarReminderService.");
//                }

//                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

//            }  
//        }
//    }
//}



using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.services.NotificationService;
using Task = System.Threading.Tasks.Task;

namespace WebApplicationFlowSync.services.BackgroundServices
{
    public class CalendarReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<CalendarReminderService> logger;

        public CalendarReminderService(IServiceScopeFactory serviceScopeFactory, ILogger<CalendarReminderService> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = serviceScopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var now = DateTime.Now;

                        var upcomingEvents = await context.CalendarEvents
                            .Include(e => e.User)
                            .Where(e =>
                                e.EventDate > now &&
                                e.EventDate <= now.AddHours(25) &&
                                (!e.ReminderSent1Day || !e.ReminderSent1Hour))
                            .ToListAsync(stoppingToken);

                        foreach (var calendarEvent in upcomingEvents)
                        {
                            var user = calendarEvent.User;
                            if (user == null) continue;

                            var timeUntilEvent = calendarEvent.EventDate - now;

                            logger.LogInformation($"Checking event '{calendarEvent.Title}' for user {user.Email}. Time until event: {timeUntilEvent.TotalMinutes} minutes.");

                            // 1-day reminder
                            if (!calendarEvent.ReminderSent1Day &&
                                timeUntilEvent.TotalHours <= 25 && timeUntilEvent.TotalHours >= 23)
                            {
                                string message = $"Reminder: You have an event '{calendarEvent.Title}' tomorrow at {calendarEvent.EventDate:HH:mm}.";
                                await notificationService.SendNotificationAsync(
                                    user.Id,
                                    message,
                                    NotificationType.Reminder,
                                    email: user.Email,
                                    linkText: "View Event",
                                    linkUrl: "/calendar"
                                );

                                calendarEvent.ReminderSent1Day = true;
                                logger.LogInformation($"1-day reminder sent to {user.Email} for event '{calendarEvent.Title}'.");
                            }

                            // 1-hour reminder
                            if (!calendarEvent.ReminderSent1Hour &&
                                timeUntilEvent.TotalMinutes <= 65 && timeUntilEvent.TotalMinutes >= 55)
                            {
                                string message = $"Reminder: Your event '{calendarEvent.Title}' will start in 1 hour (at {calendarEvent.EventDate:HH:mm}).";
                                await notificationService.SendNotificationAsync(
                                    user.Id,
                                    message,
                                    NotificationType.Reminder,
                                    email: user.Email,
                                    linkText: "View Event",
                                    linkUrl: "/calendar"
                                );

                                calendarEvent.ReminderSent1Hour = true;
                                logger.LogInformation($"1-hour reminder sent to {user.Email} for event '{calendarEvent.Title}'.");
                            }
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred in CalendarReminderService.");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
