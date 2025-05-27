using Microsoft.AspNetCore.Mvc;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.services.EmailService;

namespace WebApplicationFlowSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscribeController : ControllerBase
    {
        private readonly IEmailService emailService;
        private readonly ApplicationDbContext context;
        private readonly ILogger<SubscribeController> logger;

        public SubscribeController(IEmailService emailService, ApplicationDbContext context, ILogger<SubscribeController> logger)
        {
            this.emailService = emailService;
            this.context = context;
            this.logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest("Email is required.");

            var existing = context.Subscribers.FirstOrDefault(s => s.Email == model.Email);
            if (existing != null)
                return Ok ("You are already subscribed.");

            var subscriber = new Subscriber
            {
                Email = model.Email
            };

            context.Subscribers.Add(subscriber);
            await context.SaveChangesAsync();

            try
            {
                await emailService.SendSubscriptionConfirmationEmailAsync(model.Email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send subscription confirmation email.");
            }

            return Ok("Subscription successful! Please check your email for confirmation.");
        }
    }
}
