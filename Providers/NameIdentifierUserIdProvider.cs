using Microsoft.AspNetCore.SignalR;

namespace WebApplicationFlowSync.Providers
{
    public class NameIdentifierUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

    }
}
