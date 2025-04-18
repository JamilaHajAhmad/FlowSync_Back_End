using WebApplicationFlowSync.DTOs;

namespace WebApplicationFlowSync.services.EmailService
{
     public interface IEmailService
     {
         Task sendEmailAsync(EmailDto request);
     }
    
}
