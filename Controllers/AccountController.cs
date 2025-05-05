using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.Net;
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

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService, ApplicationDbContext context, AuthServices authServices , INotificationService notificationService)
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
                JoinedAt = model.Role == Role.Leader ? DateTime.UtcNow : null
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
            if(!passwordValid)
                return BadRequest("Incorrect password.");

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new Exception($"Failed to delete account: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return Ok("Your account has been deleted successfully.");
        }
    }
}
