using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Models;

namespace WebApplicationFlowSync.services.BackgroundServices
{
    public class LeaderLeaveMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<LeaderLeaveMonitorService> logger;

        public LeaderLeaveMonitorService(IServiceScopeFactory serviceScopeFactory, ILogger<LeaderLeaveMonitorService> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }

        protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                    var leader = await context.Users.FirstOrDefaultAsync(u => u.Role == Role.Leader);
                    var admin = await userManager.FindByEmailAsync("admin@dubaipolicegov.ae");

                    if (leader != null && admin != null && await userManager.IsInRoleAsync(admin, "Admin"))
                    {
                        var isLeaderOnLeave = leader.Status == UserStatus.Temporarily_Leave || leader.Status == UserStatus.Annually_Leave;
                        var isAdminInLeaderRole = await userManager.IsInRoleAsync(admin, "Leader");

                        if (isLeaderOnLeave && !isAdminInLeaderRole)
                        {
                            await userManager.AddToRoleAsync(admin, "Leader");
                            logger.LogInformation("Admin has been granted 'Leader' role because leader is on leave.");
                        }
                        else if (!isLeaderOnLeave && isAdminInLeaderRole)
                        {
                            await userManager.RemoveFromRoleAsync(admin, "Leader");
                            logger.LogInformation("Admin 'Leader' role removed because leader is back on duty.");
                        }
                    }

                }
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            }

        }
    }
}
