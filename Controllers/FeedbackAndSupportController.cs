using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.services.EmailService;

namespace WebApplicationFlowSync.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FeedbackAndSupportController : ControllerBase
    {
        private readonly IEmailService emailService;
        private readonly UserManager<AppUser> userManager;

        public FeedbackAndSupportController(IEmailService emailService, UserManager<AppUser> userManager)
        {
            this.emailService = emailService;
            this.userManager = userManager;
        }

        [HttpPost("feedback")]
        [Authorize]
        public async Task<IActionResult> SendFeedback([FromBody] FeedbackDto feedback)
        {
            if (feedback.Rating < 1 || feedback.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var role = await userManager.IsInRoleAsync(user, "Leader") ? "Leader" : "Member";

            var body = $@"
                <h3>Feedback Received</h3>
                <ul>
                    <li><strong>From:</strong> {user.FirstName} {user.LastName}</li>
                    <li><strong>Email:</strong> {user.Email}</li>
                    <li><strong>Role:</strong> {role}</li>
                    <li><strong>Rating:</strong> {feedback.Rating}/5</li>
                    <li><strong>Message:</strong> {(string.IsNullOrWhiteSpace(feedback.Message) ? "(No message)" : feedback.Message)}</li>
                    <li><strong>Can Follow Up:</strong> {(feedback.CanFollowUp ? "Yes" : "No")}</li>
                </ul>";

            // Send to support team
            var emailDto = new EmailDto
            {
                To = "flowsync2027@outlook.com",
                Subject = "New Feedback from User",
                Body = body
            };
            await emailService.sendEmailAsync(emailDto);

            // Confirmation to user
            var confirmationToUser = new EmailDto
            {
                To = user.Email,
                Subject = "Thank you for your feedback – FlowSync",
                Body = $@"
                    <p>Dear {user.FirstName},</p>
                    <p>Thank you for your feedback! We’ve received your message and appreciate your input.</p>
                    <p>The FlowSync Team</p>"
            };
            await emailService.sendEmailAsync(confirmationToUser);

            return Ok(new { message = "Feedback sent successfully." });
        }

        [HttpPost("support")]
        [Authorize]
        public async Task<IActionResult> SendSupportRequest([FromBody] SupportRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Description))
                return BadRequest("Subject and Description are required.");

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var role = await userManager.IsInRoleAsync(user, "Leader") ? "Leader" : "Member";

            var body = $@"
                <h3>New Support Request</h3>
                <ul>
                    <li><strong>From:</strong> {user.FirstName} {user.LastName}</li>
                    <li><strong>Email:</strong> {user.Email}</li>
                    <li><strong>Role:</strong> {role}</li>
                    <li><strong>Type:</strong> {request.RequestType}</li>
                    <li><strong>Subject:</strong> {request.Subject}</li>
                    <li><strong>Description:</strong> {request.Description}</li>
                    <li><strong>Priority:</strong> {request.PriorityLevel}</li>
                </ul>";

            // Send to support team
            var emailDto = new EmailDto
            {
                To = "flowsync2027@outlook.com",
                Subject = $"[Support] {request.Subject} - {request.PriorityLevel}",
                Body = body
            };
            await emailService.sendEmailAsync(emailDto);

            // Confirmation to user
            var confirmationToUser = new EmailDto
            {
                To = user.Email,
                Subject = "Support Request Received – FlowSync",
                Body = $@"
                    <p>Dear {user.FirstName},</p>
                    <p>Your support request has been received. We will get back to you as soon as possible.</p>
                    <p>The FlowSync Team</p>"
            };
            await emailService.sendEmailAsync(confirmationToUser);

            return Ok(new { message = "Support request sent successfully." });
        }
    }
}
