using MailKit.Security;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MimeKit;
using WebApplicationFlowSync.DTOs;

namespace WebApplicationFlowSync.services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration config;

        public EmailService(IConfiguration config)
        {
            this.config = config;
        }
        public async Task sendEmailAsync(EmailDto request)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(config.GetSection("EmailSettings")["EmailUserName"]));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = request.Body };


            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(config.GetSection("EmailSettings")["EmailHost"], 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(config.GetSection("EmailSettings")["EmailUserName"], config.GetSection("EmailSettings")["EmailPassword"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
    }
