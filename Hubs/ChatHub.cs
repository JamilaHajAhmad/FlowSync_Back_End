using Microsoft.AspNetCore.SignalR; // تفعيل الاتصال اللحظي (real-time)
using WebApplicationFlowSync.services;
namespace WebApplicationFlowSync.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            this.logger = logger;
        }
        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            logger.LogInformation("SendMessage invoked: {Sender} => {Receiver}: {Message}", senderId, receiverId, message);
            // هذا يُرسل الرسالة مباشرة للطرف الآخر في الوقت الحقيقي
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            logger.LogInformation("User connected: {UserId}, ConnectionId: {ConnectionId}", userId, Context.ConnectionId);

            if (!string.IsNullOrEmpty(userId))
            {
                ConnectedUsersTracker.AddConnection(userId, Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                logger.LogInformation("User {UserId} added to group.", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            logger.LogInformation("User disconnected: {UserId}, ConnectionId: {ConnectionId}", userId, Context.ConnectionId);

            if (!string.IsNullOrEmpty(userId))
            {
                ConnectedUsersTracker.RemoveConnection(userId, Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                logger.LogInformation("User {UserId} removed from group.", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }


    }
}
