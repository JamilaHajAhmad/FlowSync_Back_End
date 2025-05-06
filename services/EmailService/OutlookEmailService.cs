using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using WebApplicationFlowSync.Classes;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.services.CacheServices;
using WebApplicationFlowSync.services.ExternalServices;
using WebApplicationFlowSync.services.SettingService;

namespace WebApplicationFlowSync.services.EmailService
{
    public class OutlookEmailService : IEmailService
    {
        private readonly GraphAuthProvider _authProvider;
        private readonly ISettingsService _settingsService;
        private readonly IMicrosoftAuthorizationClient _microsoftAuthorizationClient;
        private readonly MicrosoftAuthorizationServiceSetting microsoftAuthSettings;
        private readonly ILogger<OutlookEmailService> _logger;
        private readonly ICacheService _cacheService;
        public OutlookEmailService(GraphAuthProvider authProvider, ISettingsService settingsService,
            IMicrosoftAuthorizationClient microsoftAuthorizationClient, ILogger<OutlookEmailService> logger, ICacheService cacheService)
        {
            _authProvider = authProvider;
            _settingsService = settingsService;
            _microsoftAuthorizationClient = microsoftAuthorizationClient;
            microsoftAuthSettings = _settingsService.GetMicrosoftAuthorizationServiceSetting();
            _logger = logger;
            _cacheService = cacheService;
        }
        public async Task sendEmailAsync(EmailDto request)
        {
            try
            {
                await SendEmailBySmtpClientAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email: {ex.Message}");
            }
        }



        #region Send Email

        private async Task SendEmailBySmtpClientAsync(EmailDto request)
        {
            var emailSettings = _settingsService.GetEmailSettings();
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("", emailSettings.EmailUserName));
            message.To.Add(new MailboxAddress("", request.To));
            message.Subject = request.Subject;
            //message.Body = new TextPart("plain") { Text = request.Body };
            //تحويل Body الى Html
            message.Body = new TextPart("html") { Text = request.Body };


            //get access token using offline access or Interactive (browser login needed)
            string accessToken = await GetAccessTokenWithRefreshTokenAsync(); // await GetAccessTokenInteractiveAsync(); ;

            using var client = new SmtpClient();
            // Connect to Outlook SMTP server
            await client.ConnectAsync(emailSettings.EmailHost, 587, SecureSocketOptions.StartTls);

            // Authenticate using OAuth 2.0
            await client.AuthenticateAsync(new SaslMechanismOAuth2(emailSettings.EmailUserName, accessToken));

            // Send the message
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        /// <summary>
        /// SendEmailByGraphAPIAsync
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task SendEmailByGraphAPIAsync(EmailDto request)
        {
            try
            {
                //send email by microsoft graph API, the user need to login to be able to send email
                var graphClient = _authProvider.GetAuthenticatedClient();
                var message = new Message
                {
                    Subject = request.Subject,
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Text,
                        Content = request.Body
                    },
                    ToRecipients =
                    [
                        new Recipient
                        {
                            EmailAddress = new EmailAddress
                            {
                                Address = request.To
                            }
                        }
                    ]
                };

                await graphClient.Me.SendMail.PostAsync(new Microsoft.Graph.Me.SendMail.SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email: {ex.Message}");
            }
        }

        #endregion


        #region Get Microsoft Access Token

        /// <summary>
        /// GetAccessTokenInteractiveAsync
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetAccessTokenInteractiveAsync()
        {
            try
            {
                //to get the access token by Interactive login (browser login)
                var azureAdSettings = microsoftAuthSettings.AzureAdSettings;
                string[] Scopes = ["https://outlook.office.com/SMTP.Send", "offline_access"];

                var app = PublicClientApplicationBuilder
                    .Create(azureAdSettings.ClientId)
                    .WithAuthority(AadAuthorityAudience.PersonalMicrosoftAccount)
                    .WithRedirectUri(azureAdSettings.RedirectUrl)
                    .Build();

                // Acquire token
                AuthenticationResult? result = null;
                var accounts = await app.GetAccountsAsync();
                var account = accounts.FirstOrDefault();

                try
                {
                    // Try to get token silently first (if user previously authenticated)
                    if (account != null)
                    {
                        result = await app.AcquireTokenSilent(Scopes, account)
                            .ExecuteAsync();
                    }
                }
                catch (MsalUiRequiredException ex)
                {
                    // Silent acquisition failed or no account available
                }

                // If we don't have a result yet, use interactive flow
                result ??= await app.AcquireTokenInteractive(Scopes)
                        .ExecuteAsync();

                return result.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "get access token failed in GetAccessTokenInteractiveAsync");
                return "";
            }
        }
        /// <summary>
        /// GetAccessTokenWithRefreshTokenAsync
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetAccessTokenWithRefreshTokenAsync()
        {
            try
            {

                //use to get access token by using offline token
                var accessToken = await _microsoftAuthorizationClient.GetAccessTokenUsingRefreshTokenAsync();
                return accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "get access token by refresh token failed in GetAccessTokenWithRefreshTokenAsync");
                return "";
            }
        }

        // ✅ الدالة الجديدة
        public async Task SendConfirmationEmail(string to, string subject, string link)
        {
            string htmlBody = EmailTemplateBuilder.BuildTemplate(
                 "Email Confirmation",
                 "Please confirm your email by clicking the button below:",
                 "Confirm Email",
                  link
                  );

            var emailDto = new EmailDto
            {
                To = to,
                Subject = subject,
                //Body = $"يرجى تأكيد بريدك عبر الرابط التالي: <a href=\"{link}\">{link}</a>"
                //Body = $"يرجى تأكيد بريدك عبر الرابط التالي: <a href=\"{link}\">اضغط هنا لتأكيد بريدك الإلكتروني</a>"
                Body = htmlBody
            };
            await sendEmailAsync(emailDto);
        }

        /// <summary>
        /// check if access token is expired or not
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private bool IsAccessAccountTokenNeedRefresh(string? accessToken)
        {

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return true;
            }
            else
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(accessToken);
                if (token.ValidTo.AddMinutes(-1) < DateTime.Now)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion

    }
}