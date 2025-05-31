using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Errors;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.services.EmailService;
using WebApplicationFlowSync.services;
using Task = System.Threading.Tasks.Task;
using System.Text.Json.Serialization;
using WebApplicationFlowSync.services.CacheServices;
using WebApplicationFlowSync.services.SettingService;
using WebApplicationFlowSync.services.ExternalServices;
using WebApplicationFlowSync.Classes;
using WebApplicationFlowSync.services.NotificationService;
using WebApplicationFlowSync.services.BackgroundServices;
using WebApplicationFlowSync.services.KpiService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebApplicationFlowSync.Hubs;
using Microsoft.AspNetCore.SignalR;
using WebApplicationFlowSync.Providers;

namespace WebApplicationFlowSync
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            // ????? ??? Identity ?? ???????

            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {

                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;


                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                //options.User.RequireUniqueEmail = true;       
            }).AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();

            //2FA service 
            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
               options.TokenLifespan = TimeSpan.FromHours(3)); // مدة صلاحية رمز 2FA




            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Add Cors
            builder.Services.AddCors(
               options =>
               {
                   options.AddPolicy("AllowAll",
                   builder =>
                   {
                       //builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                       builder.WithOrigins("http://localhost:3001") // ← ضع عنوان الواجهة
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials(); // ← مطلوب مع SignalR


                   });

               });


            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();




            builder.Services.Configure<ApplicationSettings>(builder.Configuration);
            builder.Services.AddScoped<ISettingsService, SettingsService>();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<ICacheService, CacheService>();

            // Email Service 
            RegisterMailServices(builder);

            // Email Service (outlook)
            //builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.AddSignalR();

            builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

            //Add Auth Service (generate token)
            builder.Services.AddScoped<AuthServices>();

            //Notification Service
            builder.Services.AddScoped<INotificationService, NotificationService>();
            // Serilog
            builder.Host.UseSerilog((context, config) =>
            {
                config.ReadFrom.Configuration(context.Configuration);
            });


            // JWT
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetSection("jwt")["ValidIssuer"],
            ValidAudience = builder.Configuration.GetSection("jwt")["ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("jwt")["secretKey"])),

            // ✅ هذا مهم جدًا للسماح باستخدام [Authorize(Roles = "...")]
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // استخراج التوكن من الكويري سترينغ إذا لم يكن موجود في الهيدر (مفيد لـ SignalR)
                var accessToken = context.Request.Query["access_token"];

                // تأكد أن الطلب خاص بـ SignalR
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

            //BackgroundService (TaskReminderService)
            builder.Services.AddHostedService<TaskReminderService>();

            //BackgroundService (CalenderReminderService)
            builder.Services.AddHostedService<CalendarReminderService>();

            //calculate Kpi Service
            builder.Services.AddScoped<IKpiService , KpiService>();

            builder.Services.AddAuthorization();

            builder.Services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ✅ إضافة هذا البلوك لإنشاء الأدوار عند بداية التشغيل
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                Task.Run(async () =>
                {
                    string[] roles = { "Leader", "Member" };
                    foreach (var role in roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                        }
                    }
                }).GetAwaiter().GetResult();
            }

            app.MapHub<ChatHub>("/chatHub");

            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseAuthentication();  // لازم قبل UseAuthorization
            app.UseAuthorization();


            app.MapControllers();

            app.UseExceptionHandler();
            app.Run();
        }

        /// <summary>
        /// RegisterMailServices
        /// </summary>
        /// <param name="builder"></param>
        private static void RegisterMailServices(WebApplicationBuilder builder)
        {
            var config = builder.Configuration;
            var microsoftAuthorizationUrl = config["MicrosoftAuthorizationServiceSettings:BaseUrl"] ?? "";
            var emailHost = config.GetSection("EmailSettings")["EmailHost"];

            //Register microsoft authorization and graph servcies
            builder.Services.AddScoped<GraphAuthProvider>();
            builder.Services.AddHttpClient<IMicrosoftAuthorizationClient, MicrosoftAuthorizationClient>(client =>
            {
                client.BaseAddress = new Uri(microsoftAuthorizationUrl);
            });

            //Register mail services
            if (emailHost != null && emailHost == "smtp.office365.com")
            {
                builder.Services.AddScoped<IEmailService, OutlookEmailService>();
            }
            else
            {
                builder.Services.AddScoped<IEmailService, EmailService>();
            }
        }
    }
}
