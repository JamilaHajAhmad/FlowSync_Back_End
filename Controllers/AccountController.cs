using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.Net;
using System.Security.Claims;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.DTOs.Auth;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.Models.Requests;
using WebApplicationFlowSync.services;
using WebApplicationFlowSync.services.EmailService;
using WebApplicationFlowSync.services.NotificationService;
using Task = System.Threading.Tasks.Task;

namespace WebApplicationFlowSync.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IEmailService emailService;
        private readonly ApplicationDbContext context;
        private readonly AuthServices authServices;
        private readonly INotificationService notificationService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService, ApplicationDbContext context, AuthServices authServices, INotificationService notificationService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
            this.context = context;
            this.authServices = authServices;
            this.notificationService = notificationService;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var existingUserByEmail = await userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
                throw new Exception("Email is already taken.");

            var existingUserByUsername = await userManager.FindByNameAsync(model.Email.Split('@')[0]);
            if (existingUserByUsername != null)
                throw new Exception("Username is already taken.");

            if (model.Role == Role.Member && !userManager.Users.Any(u => u.Role == Role.Leader))
                throw new Exception("A member cannot register without a leader.");

            if (model.Role == Role.Leader)
            {
                var existingLeader = await userManager.Users.FirstOrDefaultAsync(u => u.Role == Role.Leader);
                if (existingLeader != null)
                    throw new Exception("There is really only one team leader.");
            }

            var user = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Role = model.Role,
                UserName = model.Email,
                EmailConfirmed = false,
                JoinedAt = model.Role == Role.Leader ? DateTime.Now : null
            };

            try
            {
                var result = await userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

                await userManager.AddToRoleAsync(user, model.Role.ToString());

                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                //var encodedToken = WebUtility.UrlEncode(token);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userId = user.Id,
                    token = token,
                }, Request.Scheme); // بدلاً من استخدام Request.Scheme
                Console.WriteLine("Hello");
                Console.WriteLine(confirmationLink);
                if (model.Role == Role.Leader)
                {
                    await emailService.SendConfirmationEmail(user.Email, "تأكيد حسابك كـ Leader", confirmationLink);
                }
                else if (model.Role == Role.Member)
                {
                    var leader = await userManager.Users.FirstOrDefaultAsync(u => u.Role == Role.Leader);
                    if (leader is null)
                        throw new Exception("There is no Leader currently.");

                    var pendingRequest = new SignUpRequest()
                    {
                        MemberId = user.Id,
                        LeaderId = leader.Id,
                        Type = RequestType.SignUp,
                        MemberName = user.FirstName + " " + user.LastName,
                        Email = user.Email
                    };
                    await context.PendingMemberRequests.AddAsync(pendingRequest);
                    await context.SaveChangesAsync();

                    await notificationService.SendNotificationAsync(
                        leader.Id,
                        $"Member {user.FirstName} {user.LastName} has submitted a SignUp request",
                        NotificationType.SignUpRequest
                    );
                }

                return Ok(new { message = "success" });
            }
            catch (Exception ex)
            {
                // حذف المستخدم في حال حصول أي خطأ
                await userManager.DeleteAsync(user);
                context.SaveChangesAsync();
                return StatusCode(500, $"حدث خطأ أثناء إنشاء الحساب: {ex.InnerException?.Message ?? ex.Message}");
            }
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is null) return Unauthorized("Email not registered.");


            //Check Password
            if (model.Password is null) return Unauthorized();

            var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded) return Unauthorized("Invalid data");

            // Check isEmailConfirmation
            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                return Unauthorized("Please confirm your email before logging in.");
            }
            var token = await authServices.CreateTokenAsync(user, userManager);


            // الحصول على عنوان الـ IP الخاص بالمستخدم الذي أرسل الطلب الحالي
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // الحصول على معلومات المتصفح أو الجهاز (User-Agent) من هيدر الطلب
            var userAgent = Request.Headers["User-Agent"].ToString();

            // إنشاء كائن جديد من نوع UserSession لتسجيل معلومات الجلسة الحالية للمستخدم
            var session = new UserSession
            {
                UserId = user.Id,            // ربط الجلسة بالمستخدم الذي سجل الدخول (بناءً على الـ Id)
                DeviceInfo = userAgent,      // تخزين معلومات الجهاز أو المتصفح المستخدم للدخول
                IPAddress = ipAddress,       // حفظ عنوان IP الذي دخل منه المستخدم
                Token = token,               // (اختياري) حفظ رمز الـ JWT الصادر لتلك الجلسة إن وجد
                LoginTime = DateTime.UtcNow, // تسجيل توقيت الدخول باستخدام توقيت UTC العالمي
                IsActive = true              // تمييز أن هذه الجلسة لا تزال فعالة (نشطة)
            };

            // إضافة الجلسة الجديدة إلى قاعدة البيانات عبر DbContext
            context.UserSessions.Add(session);

            // حفظ التغييرات إلى قاعدة البيانات بشكل فعلي
            await context.SaveChangesAsync();


            // إرسال إشعار بتسجيل الدخول
            await notificationService.SendNotificationAsync(
                user.Id,
                $"Login detected from device: {userAgent} with IP: {ipAddress}",
                NotificationType.Security
            );


            return Ok(new
            {
                Message = "successfully logged in!!",

                User = new UserDto()
                {
                    DisplayName = user.FirstName + " " + user.LastName,
                    Email = user.Email
                },
                token = token
            });

        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (model.NewPassword != model.ConfirmPassword)
            {
                throw new Exception("The new password and confirmation  not the same!");
            }

            // نحصل على المستخدم الحالي من التوكن
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return Ok("Your password has been changed successfully.");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest("Email is required.");

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
            {
                // لا تكشف أن المستخدم غير موجود أو لم يؤكد بريده لأسباب أمنية
                return Ok("If an account with that email exists, a reset link has been sent.");
            }

            // إنشاء رمز إعادة تعيين كلمة المرور
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token); // مهم جداً لتأمين الرابط

            // إنشاء رابط إعادة تعيين كلمة المرور
            var resetLink = $"http://localhost:3001/reset-password?userId={user.Id}&token={encodedToken}";

            // إرسال الإيميل
            var emailDto = new EmailDto
            {
                To = user.Email,
                Subject = "Reset your password",
                Body = $"Click the link to reset your password: <a href=\"{resetLink}\">{resetLink}</a>"
            };

            await emailService.sendEmailAsync(emailDto);

            return Ok("If an account with that email exists, a reset link has been sent.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);

            if (user == null)
                return BadRequest("Invalid user.");

            var decodedToken = Uri.UnescapeDataString(model.Token); // فك التشفير إذا لزم


            var result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok("Password has been reset successfully.");
        }



        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest("معرف المستخدم أو الرمز غير صالح.");
            }
            //يجب فك تشفير التوكين
            token = WebUtility.UrlDecode(token);

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("المستخدم غير موجود.");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest($"فشل تأكيد البريد الإلكتروني: {errors}");
            }

            return Ok("تم تأكيد البريد الإلكتروني بنجاح.");
        }


        //private async Task SendConfirmationEmail(string to, string subject, string link)
        //{
        //    var emailDto = new EmailDto
        //    {
        //        To = to,
        //        Subject = subject,
        //        Body = $"يرجى تأكيد بريدك عبر الرابط التالي: {link}"
        //    };
        //    await emailService.sendEmailAsync(emailDto);
        //}

        [HttpPost("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized("User not found.");

            var passwordValid = await userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return BadRequest("Incorrect password.");

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new Exception($"Failed to delete account: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return Ok("Your account has been deleted successfully.");
        }


        [HttpGet("connected-devices")]
        [Authorize]
        public async Task<IActionResult> GetConnectedDevices()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            var sessions = await context.UserSessions
                .Where(s => s.UserId == user.Id && s.IsActive)
                .OrderByDescending(s => s.LoginTime)
                .Select(s => new
                {
                    s.Id,
                    s.DeviceInfo,
                    s.IPAddress,
                    s.LoginTime
                })
                .ToListAsync();

            return Ok(sessions);
        }

        [HttpPost("api/account/sessions/{sessionId}/logout")]
        [Authorize]
        public async Task<IActionResult> LogoutSession(int sessionId)
        {
            var user = await userManager.GetUserAsync(User);

            var session = await context.UserSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == user.Id);

            if (session == null) return NotFound();
            session.IsActive = false;
            await context.SaveChangesAsync();

            return Ok();
        }


        [HttpPost("enable-2fa")]
        [Authorize]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            if (await userManager.GetTwoFactorEnabledAsync(user))
                return BadRequest("Two-factor authentication is already enabled.");

            var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");

            if (string.IsNullOrEmpty(token))
                throw new Exception("Failed to generate token for 2FA.");

            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("User email address is missing.");

            var emailDto = new EmailDto()
            {
                To = user.Email,
                Subject = $"Two-Factor Authentication Code",
                Body = $"Your 2FA code is: {token}"
            };

            if (string.IsNullOrEmpty(emailDto.To) || string.IsNullOrEmpty(emailDto.Subject) || string.IsNullOrEmpty(emailDto.Body))
            {
                return StatusCode(500, "Email content is invalid.");
            }
            await emailService.sendEmailAsync(emailDto);

            return Ok("Two-factor authentication is enabled. Please check your email for the verification code.");
        }

        [HttpPost("disable-2fa")]
        [Authorize]
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            var result = await userManager.SetTwoFactorEnabledAsync(user, false);
            if (!result.Succeeded)
                throw new Exception("Failed to disable two-factor authentication.");

            return Ok("Two-factor authentication has been disabled.");
        }

        //2FAالتحقق من رمز ال 
        [HttpPost("verfiy-2fa")]
        [Authorize]
        public async Task<IActionResult> VerifyTwoFactorAuthentication(VerifyTwoFactorDto model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            //verify code
            var result = await userManager.VerifyTwoFactorTokenAsync(user, "Email", model.Code);

            if (!result)
                return BadRequest("Invalid verification code.");

            //Enable 2FA
            var enableResult = await userManager.SetTwoFactorEnabledAsync(user, true);
            if (!enableResult.Succeeded)
                throw new Exception("Failed to enable two-factor authentication.");

            return Ok("Two-factor authentication has been enabled successfully.");

        }

    }
}