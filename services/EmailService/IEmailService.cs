using WebApplicationFlowSync.DTOs;

namespace WebApplicationFlowSync.services.EmailService
{
     public interface IEmailService
     {
         Task sendEmailAsync(EmailDto request);
        Task SendConfirmationEmail(string to, string subject, string link);
        Task SendSubscriptionConfirmationEmailAsync(string email);
    }
    
}
