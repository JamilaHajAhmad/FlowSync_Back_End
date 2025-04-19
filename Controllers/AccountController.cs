using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.DTOs.Auth;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.Models.Requests;
using WebApplicationFlowSync.services;
using WebApplicationFlowSync.services.EmailService;
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

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService, ApplicationDbContext context, AuthServices authServices)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
            this.context = context;
            this.authServices = authServices;
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
                EmailConfirmed = false
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
                    //var pendingRequest = new PendingMemberRequest()
                    //{
                    //    MemberId = user.Id,
                    //    LeaderId = leader.Id,
                    //};

                    await context.PendingMemberRequests.AddAsync(pendingRequest);
                    await context.SaveChangesAsync();
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

        //// موافقة القائد على العضو
        //[HttpPost("approve-member/{requestId}")]
        //[Authorize(Roles = "Leader")]
        //public async Task<IActionResult> ApproveMember(int requestId)
        //{
        //    var pendingRequest = await context.PendingMemberRequests
        //        .FirstOrDefaultAsync(r => r.Id == requestId);
        //    if (pendingRequest == null)
        //    {
        //        throw new Exception("Membership request not found.");
        //    }

        //    var currentUser = await userManager.GetUserAsync(User);
        //    //Console.WriteLine($"Current user: {currentUser?.UserName}");  // لازم يظهر اسم
        //    if (currentUser == null)
        //    {
        //        throw new Exception("User identity not verified (member information not found in the request)");
        //    }

        //    if (pendingRequest.LeaderId == null)
        //    {
        //        //return BadRequest("الطلب لا يحتوي على معرف القائد.");
        //        throw new Exception("The request does not contain a leader ID.");
        //    }

        //    // الموافقة على الطلب
        //    pendingRequest.RequestStatus = RequestStatus.Approved;
        //    await context.SaveChangesAsync(); // حفظ التغييرات في قاعدة البيانات

        //    // إرسال بريد إلكتروني للميمبر بعد الموافقة
        //    var member = await userManager.FindByIdAsync(pendingRequest.MemberId);
        //    if (member != null)
        //    {
        //        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(member);
        //        var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = member.Id, token = confirmationToken }, Request.Scheme);

        //        await SendConfirmationEmail(member.Email, "The Leader has accepted your request to join the Team. Confirm your account as a Member.", confirmationLink);
        //    }

        //    return Ok("Membership has been successfully approved, please check your email.");
        //}


        //[HttpPost("reject-member/{requestId}")]
        //[Authorize(Roles = "Leader")]
        //public async Task<IActionResult> RejectMember(int requestId)
        //{
        //    var pendingRequest = await context.PendingMemberRequests
        //        .FirstOrDefaultAsync(r => r.Id == requestId);

        //    if (pendingRequest == null)
        //    {
        //        //return NotFound("طلب العضوية غير موجود.");
        //        throw new Exception("Membership request not found.");
        //    }

        //    // التأكد من أن القائد هو من يرفض
        //    var currentUser = await userManager.GetUserAsync(User);
        //    if (pendingRequest.LeaderId != currentUser.Id)
        //    {
        //        //return Unauthorized("أنت لست القائد المعني.");
        //        throw new Exception("You are not the leader required in the request.");
        //    }

        //    var member = await userManager.FindByIdAsync(pendingRequest.MemberId);
        //    // رفض الطلب
        //    pendingRequest.RequestStatus = RequestStatus.Rejected;
        //    //حذف الطلب 
        //    //context.PendingMemberRequests.Remove(pendingRequest);
        //    await context.SaveChangesAsync(); /// نحفظ هنا قبل حذف العضو
        //    // حذف المستخدم من النظام
        //    if (member != null)
        //    {
        //        await userManager.DeleteAsync(member);
        //        await context.SaveChangesAsync();
        //    }

        //    return Ok("The request was rejected and the member was removed from the system.");
        //}

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

            // إنشاء رابط إعادة تعيين كلمة المرور
            var resetLink = Url.Action("ResetPassword", "Account", new
            {
                userId = user.Id,
                token = token
            }, Request.Scheme);

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

            var result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok("Password has been reset successfully.");
        }


        // تأكيد البريد الإلكتروني
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                throw new Exception("معرف المستخدم أو الرمز غير صالح.");
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("المستخدم غير موجود.");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                throw new Exception("فشل تأكيد البريد الإلكتروني.");
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
    }
}
