using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Hubs;
using WebApplicationFlowSync.Models;
namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly IHubContext<ChatHub> chatHub;

        public ChatController(ApplicationDbContext context, UserManager<AppUser> userManager, IHubContext<ChatHub> chatHub)
        {
            this.context = context;
            this.userManager = userManager;
            this.chatHub = chatHub;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var sender = await userManager.GetUserAsync(User);
            var reciver = await userManager.FindByIdAsync(dto.ReceiverId);

            if (sender == null || reciver == null)
                return NotFound("Sender or Receiver not found.");

            var message = new Models.ChatMessage
            {
                SenderId = sender.Id,
                ReceiverId = dto.ReceiverId,
                Message = dto.Message
            };

            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();

            // إشعار لحظي للطرف الآخر
            await chatHub.Clients.User(dto.ReceiverId).SendAsync("ReceiveMessage", sender.Id, dto.Message);

            // تحويل إلى DTO
            var messageDto = new ChatMessageDto
            {
                Id = message.Id,
                Message = message.Message,
                SentAt = message.SentAt,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId
            };

            return Ok(messageDto);
        }

        //جلب المحادثة بين الطرفين
        [HttpGet("conversation")]
        public async Task<IActionResult> GetConversation(string userId)
        {
            var currentUser = await userManager.GetUserAsync(User);
            var messages = await context.ChatMessages
                .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == userId) ||
                             (m.SenderId == userId && m.ReceiverId == currentUser.Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Ok(messages.Select(m => new ChatMessageDto
            {
                Id = m.Id,
                Message = m.Message,
                SentAt = m.SentAt,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                IsRead = m.IsRead
            }));
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadMessages()
        {
            var user = await userManager.GetUserAsync(User);
            var unreadMessages = await context.ChatMessages
                .Where(m => m.ReceiverId == user.Id && !m.IsRead)
                .ToListAsync();

            return Ok(unreadMessages.Select(m => new ChatMessageDto
            {
                Id = m.Id,
                Message = m.Message,
                SentAt = m.SentAt,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId
            }));
        }

        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkMessagesAsRead([FromBody] List<int> messageIds)
        {
            var user = await userManager.GetUserAsync(User);
            var messages = await context.ChatMessages
                .Where(m => messageIds.Contains(m.Id) && m.ReceiverId == user.Id)
                .ToListAsync();

            foreach(var msg in messages)
            {
                msg.IsRead = true;
            }

            await context.SaveChangesAsync();

            return Ok();
        }



    }
}
