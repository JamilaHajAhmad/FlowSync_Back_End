using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.DTOs;
using WebApplicationFlowSync.Models;
using TaskStatus = WebApplicationFlowSync.Models.TaskStatus;

namespace WebApplicationFlowSync.Controllers
{
    //[Authorize(Roles = "Leader")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;
        private readonly IWebHostEnvironment env;

        public ReportsController(ApplicationDbContext context , UserManager<AppUser> userManager , IWebHostEnvironment env)
        {
            this.context = context;
            this.userManager = userManager;
            this.env = env;
        }

        //Bar Chart - Task Distribution by Team Member
        [HttpGet("task-distribution-by-member")]
        public async Task<IActionResult> GetTaskDistributionByMember()
        {
            var data = await context.Tasks
                .GroupBy(t => new { t.UserID, t.User.FirstName, t.User.LastName, t.Type })
                .Select(g => new
                {
                    Member = g.Key.FirstName + " " + g.Key.LastName,
                    Status = g.Key.Type.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("tasks-over-months")]
        public async Task<IActionResult> GetTasksOverMonths()
        {
            var data = await context.Tasks
                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Created = g.Count(),
                    Completed = g.Count(t => t.Type == TaskStatus.Completed)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("task-status-summary")]
        public async Task<IActionResult> GetTaskStatusSummary()
        {
            var data = await context.Tasks
                .GroupBy(t => new { t.Type })
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("calendar-activity")]
        public async Task<IActionResult> GetCalendarActivity()
        {
            var data = await context.Tasks
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("tasks-by-case-source")]
        public async Task<IActionResult> GetTasksByDepartmentAndStatus()
        {
            var data = await context.Tasks
                .GroupBy(t => new { t.CaseSource, t.Type })
                .Select(g => new
                {
                    Department = g.Key.CaseSource.ToString(),
                    Status = g.Key.Type,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("requests-stream-by-type")]
        public async Task<IActionResult> GetRequestsStreamByType()
        {
            var data = await context.PendingMemberRequests
                .GroupBy(r => new
                {
                    r.RequestedAt.Year,
                    r.RequestedAt.Month,
                    r.Type
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Type = g.Key.Type,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return Ok(data);
        }

        //[HttpPost("save-report/{reportType}")]
        //public async Task<IActionResult> SaveReport(string reportType, [FromForm] SaveReportRequestDto dto, IFormFile? file)
        //{
        //    object data;
        //    string filtersApplied;

        //    switch (reportType.ToLower())
        //    {
        //        case "task-distribution-by-member":
        //            data = await context.Tasks
        //                .GroupBy(t => new { t.UserID, t.User.FirstName, t.User.LastName, t.Type })
        //                .Select(g => new
        //                {
        //                    Member = g.Key.FirstName + " " + g.Key.LastName,
        //                    Status = g.Key.Type.ToString(),
        //                    Count = g.Count()
        //                })
        //                .ToListAsync();

        //            filtersApplied = JsonConvert.SerializeObject(new { GroupBy = "Member Name and Task Status" });
        //            break;

        //        case "tasks-over-months":
        //            data = await context.Tasks
        //                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
        //                .Select(g => new
        //                {
        //                    Year = g.Key.Year,
        //                    Month = g.Key.Month,
        //                    Created = g.Count(),
        //                    Completed = g.Count(t => t.Type == TaskStatus.Completed)
        //                })
        //                .OrderBy(x => x.Year).ThenBy(x => x.Month)
        //                .ToListAsync();

        //            filtersApplied = JsonConvert.SerializeObject(new { GroupBy = "CreatedAt.Year + CreatedAt.Month" });
        //            break;

        //        case "task-status-summary":
        //            data = await context.Tasks
        //                .GroupBy(t => new { t.Type })
        //                .Select(g => new
        //                {
        //                    Status = g.Key.Type.ToString(),
        //                    Count = g.Count()
        //                })
        //                .ToListAsync();

        //            filtersApplied = JsonConvert.SerializeObject(new { GroupBy = "Task Type" });
        //            break;

        //        case "calendar-activity":
        //            data = await context.Tasks
        //                .GroupBy(t => t.CreatedAt.Date)
        //                .Select(g => new
        //                {
        //                    Date = g.Key,
        //                    Count = g.Count()
        //                })
        //                .OrderBy(x => x.Date)
        //                .ToListAsync();

        //            filtersApplied = JsonConvert.SerializeObject(new { GroupBy = "CreatedAt.Date" });
        //            break;

        //        case "tasks-by-case-source":
        //            data = await context.Tasks
        //                .GroupBy(t => new { t.CaseSource, t.Type })
        //                .Select(g => new
        //                {
        //                    Department = g.Key.CaseSource.ToString(),
        //                    Status = g.Key.Type,
        //                    Count = g.Count()
        //                })
        //                .ToListAsync();

        //            filtersApplied = JsonConvert.SerializeObject(new { GroupBy = "CaseSource + Type" });
        //            break;

        //        case "requests-stream-by-type":
        //            data = await context.PendingMemberRequests
        //                .GroupBy(r => new { r.RequestedAt.Year, r.RequestedAt.Month, r.Type })
        //                .Select(g => new
        //                {
        //                    Year = g.Key.Year,
        //                    Month = g.Key.Month,
        //                    Type = g.Key.Type,
        //                    Count = g.Count()
        //                })
        //                .OrderBy(x => x.Year).ThenBy(x => x.Month)
        //                .ToListAsync();

        //            filtersApplied = JsonConvert.SerializeObject(new { GroupBy = "RequestedAt.Year + Month + Request Type" });
        //            break;

        //        default:
        //            return BadRequest("Unknown report type");
        //    }
        //    // تحويل البيانات إلى JSON
        //    var dataJson = JsonConvert.SerializeObject(data);
        //    var user = await userManager.GetUserAsync(User);


        //    // Prepare file data if provided
        //    byte[]? fileData = null;
        //    string? fileName = null;
        //    string? fileContentType = null;

        //    var allowedTypes = new[] { 
        //        "application/pdf",
        //        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //        "text/csv" 
        //    };

        //    if (file != null)
        //    {
        //        if (file.Length > 10 * 1024 * 1024) // 10 MB
        //            return BadRequest("Maximum allowed file size is 10 MB.");

        //        if (!allowedTypes.Contains(file.ContentType))
        //            return BadRequest("Only PDF and Excel files are allowed.");

        //        using var memoryStream = new MemoryStream();
        //        await file.CopyToAsync(memoryStream);
        //        fileData = memoryStream.ToArray();
        //        fileName = file.FileName;
        //        fileContentType = file.ContentType;
        //    }

        //    var report = new Report
        //    {
        //        UserID = user.Id,
        //        Title = reportType.Replace("-", " ").ToUpperInvariant(),
        //        DataJson = dataJson,
        //        FiltersApplied = filtersApplied,
        //        CreatedAt = DateTime.Now,
        //        Description = dto.Description ?? string.Empty,
        //        FileData = fileData,
        //        FileName = fileName,
        //        FileContentType = fileContentType
        //    };
        //    context.Reports.Add(report);
        //    await context.SaveChangesAsync();

        //    return Ok(new { message = "Report saved." , 
        //        report = new
        //        {
        //            report.ReportID,
        //            report.Title,
        //            report.Description,
        //            report.CreatedAt,
        //            report.DataJson,
        //            report.FileName,
        //            report.FileContentType,
        //            report.FileData
        //        }
        //    });
        //}


        [HttpPost("save-report/{reportType}")]
        public async Task<IActionResult> SaveReport(string reportType, [FromForm] SaveReportRequestDto dto, IFormFile? file)
        {
            if (file == null)
                return BadRequest("File is required when saving a report.");

            if (file.Length > 10 * 1024 * 1024) // 10 MB
                return BadRequest("Maximum allowed file size is 10 MB.");

            var allowedTypes = new[] {
                "application/pdf",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "text/csv"
            };

            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest("Only PDF, Excel, or CSV files are allowed.");

            object data;
            string filtersApplied;

            switch (reportType.ToLower())
            {
                case "task-distribution-by-member":
                    data = await context.Tasks
                        .GroupBy(t => new { t.UserID, t.User.FirstName, t.User.LastName, t.Type })
                        .Select(g => new
                        {
                            Member = g.Key.FirstName + " " + g.Key.LastName,
                            Status = g.Key.Type.ToString(),
                            Count = g.Count()
                        })
                        .ToListAsync();
                    filtersApplied = JsonConvert.SerializeObject(new { GroupBy = "Member Name and Task Status" });
                    break;

                case "tasks-over-months":
                    data = await context.Tasks
                        .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            Created = g.Count(),
                            Completed = g.Count(t => t.Type == TaskStatus.Completed)
                        })
                        .OrderBy(x => x.Year).ThenBy(x => x.Month)
                        .ToListAsync();
                    filtersApplied = JsonConvert.SerializeObject(new { GroupBy = "CreatedAt.Year + CreatedAt.Month" });
                    break;

                case "task-status-summary":
                    data = await context.Tasks
                        .GroupBy(t => new { t.Type })
                        .Select(g => new
                        {
                            Status = g.Key.Type.ToString(),
                            Count = g.Count()
                        })
                        .ToListAsync();
                    filtersApplied = JsonConvert.SerializeObject(new { GroupBy = "Task Type" });
                    break;

                case "calendar-activity":
                    data = await context.Tasks
                        .GroupBy(t => t.CreatedAt.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            Count = g.Count()
                        })
                        .OrderBy(x => x.Date)
                        .ToListAsync();
                    filtersApplied = JsonConvert.SerializeObject(new { GroupBy = "CreatedAt.Date" });
                    break;

                case "tasks-by-case-source":
                    data = await context.Tasks
                        .GroupBy(t => new { t.CaseSource, t.Type })
                        .Select(g => new
                        {
                            Department = g.Key.CaseSource.ToString(),
                            Status = g.Key.Type,
                            Count = g.Count()
                        })
                        .ToListAsync();
                    filtersApplied = JsonConvert.SerializeObject(new { GroupBy = "CaseSource + Type" });
                    break;

                case "requests-stream-by-type":
                    data = await context.PendingMemberRequests
                        .GroupBy(r => new { r.RequestedAt.Year, r.RequestedAt.Month, r.Type })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            Type = g.Key.Type,
                            Count = g.Count()
                        })
                        .OrderBy(x => x.Year).ThenBy(x => x.Month)
                        .ToListAsync();
                    filtersApplied = JsonConvert.SerializeObject(new { GroupBy = "RequestedAt.Year + Month + Request Type" });
                    break;

                default:
                    return BadRequest("Unknown report type");
            }

            var dataJson = JsonConvert.SerializeObject(data);
            var user = await userManager.GetUserAsync(User);

            // اقرأ محتوى الملف
            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            var report = new Report
            {
                UserID = user.Id,
                Title = reportType.Replace("-", " ").ToUpperInvariant(),
                DataJson = dataJson,
                FiltersApplied = filtersApplied,
                CreatedAt = DateTime.Now,
                Description = dto.Description ?? string.Empty,
                FileData = fileData,
                FileName = file.FileName,
                FileContentType = file.ContentType
            };

            context.Reports.Add(report);
            await context.SaveChangesAsync();

            return Ok(new
            {
                message = "Report saved.",
                report = new
                {
                    report.ReportID,
                    report.Title,
                    report.Description,
                    report.CreatedAt,
                    report.DataJson,
                    report.FileName,
                    report.FileContentType,
                    report.FileData
                }
            });
        }

        [HttpGet("all-reports")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await context.Reports
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.ReportID,
                    r.Title,
                    r.Description,
                    r.FiltersApplied,
                    r.CreatedAt,
                    r.DataJson,
                    r.FileName,
                    r.FileData,
                })
                .ToListAsync();

            return Ok(reports);
        }
    }
}
