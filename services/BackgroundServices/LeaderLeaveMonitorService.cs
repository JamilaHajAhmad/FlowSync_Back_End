
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Models;

namespace WebApplicationFlowSync.services.BackgroundServices
{
    public class LeaderLeaveMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;

        public LeaderLeaveMonitorService(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
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
                    var admin = await context.Users.FirstOrDefaultAsync(u => u.Role == Role.Admin && u.Email == "admin@dubaipolicegov.ae");

                    if (leader != null && admin != null)
                    {
                        var isLeaderOnLeave = leader.Status == UserStatus.Temporarily_Leave || leader.Status == UserStatus.Annually_Leave;
                        var isAdminInLeaderRole = await userManager.IsInRoleAsync(admin, "Leader");

                        if (isLeaderOnLeave && !isAdminInLeaderRole)
                        {
                            await userManager.AddToRoleAsync(admin, "Leader");
                        }
                        else if (!isLeaderOnLeave && isAdminInLeaderRole)
                        {
                            await userManager.RemoveFromRoleAsync(admin, "Leader");
                        }
                    }

                }
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            }

        }
    }
}
