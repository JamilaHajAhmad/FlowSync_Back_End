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
    [Route("[controller]")]
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

            var (from, to) = await GetDateRangeAsync(context.Tasks.Select(t => t.CreatedAt));
            return Ok(new {
                Date = data,
                DateRange = new
                {
                  from = from,
                  To = to
                }
            }) ;
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

            var (from, to) = await GetDateRangeAsync(context.Tasks.Select(t => t.CreatedAt));
            return Ok(new
            {
                Date = data,
                DateRange = new
                {
                    from = from,
                    To = to
                }
            });
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

            var (from, to) = await GetDateRangeAsync(context.Tasks.Select(t => t.CreatedAt));
            return Ok(new
            {
                Date = data,
                DateRange = new
                {
                    from = from,
                    To = to
                }
            });
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

            var (from, to) = await GetDateRangeAsync(context.Tasks.Select(t => t.CreatedAt));
            return Ok(new
            {
                Date = data,
                DateRange = new
                {
                    from = from,
                    To = to
                }
            });
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

            var (from, to) = await GetDateRangeAsync(context.Tasks.Select(t => t.CreatedAt));
            return Ok(new
            {
                Date = data,
                DateRange = new
                {
                    from = from,
                    To = to
                }
            });
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

            var (from, to) = await GetDateRangeAsync(context.PendingMemberRequests.Select(r => r.RequestedAt));
            return Ok(new
            {
                Date = data,
                DateRange = new
                {
                    from = from,
                    To = to
                }
            });
        }

        // Dashboard Repots
        [HttpGet("leader/monthly-task-status-summary")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> GetLeaderMonthlyTaskStatusSummary()
        {
            var leader = await context.Users
                .FirstOrDefaultAsync(u => u.Role == Role.Leader && !u.IsDeactivated);
            if (leader == null)
                return Unauthorized("User not found");

            var memberIds = await context.Users
                .Where(m => m.LeaderID == leader.Id)
                .Select(m => m.Id)
                .ToListAsync();

            var now = DateTime.Now;

            var summary = await context.Tasks
                .Where(t => memberIds.Contains(t.UserID) &&
                            t.CreatedAt.Year == now.Year &&
                            t.CreatedAt.Month == now.Month)
                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month, t.CreatedAt.Day, t.Type })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Day = g.Key.Day,
                    Status = g.Key.Type.ToString(),
                    Count = g.Count()
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ThenBy(g => g.Day)
                .ToListAsync();

            return Ok(summary);
        }

        [HttpGet("member/monthly-task-status-summary")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMemberMonthlyTaskStatusSummary()
        {
            var member = await userManager.GetUserAsync(User);
            if (member == null)
                return Unauthorized("User not found");

            var now = DateTime.Now;

            var summary = await context.Tasks
                .Where(t => t.UserID == member.Id &&
                            t.CreatedAt.Year == now.Year &&
                            t.CreatedAt.Month == now.Month)
                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month, t.CreatedAt.Day, t.Type })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Day = g.Key.Day,
                    Status = g.Key.Type.ToString(),
                    Count = g.Count()
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ThenBy(g => g.Day)
                .ToListAsync();

            return Ok(summary);
        }

        [HttpGet("leader/tasks-by-year")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> GetLeaderTasksByYear()
        {
            var leader = await context.Users
                      .FirstOrDefaultAsync(u => u.Role == Role.Leader && !u.IsDeactivated);

            if (leader == null)
                return Unauthorized("User not found");

            var memberIds = await context.Users
                .Where(m => m.LeaderID == leader.Id)
                .Select(m => m.Id)
                .ToListAsync();

            var tasks = await context.Tasks
                .Where(t => memberIds.Contains(t.UserID))
                .ToListAsync();

            var groupedByYear = tasks
                .GroupBy(t => t.CreatedAt.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    TaskCount = g.Count()
                })
                .OrderBy(x => x.Year)
                .ToList();

            return Ok(groupedByYear);
        }

        [HttpGet("member/tasks-by-year")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> GetMemberYearlyTaskCounts()
        {
            var member = await userManager.GetUserAsync(User);
            if (member == null)
                return Unauthorized("User not found");

            var yearlyCounts = await context.Tasks
                .Where(t => t.UserID == member.Id)
                .GroupBy(t => t.CreatedAt.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    TaskCount = g.Count()
                })
                .OrderBy(x => x.Year)
                .ToListAsync();

            return Ok(yearlyCounts);
        }

        [HttpGet("leader/yearly-kpi")]
        [Authorize(Roles = "Leader")]
        public async Task<IActionResult> GetLeaderYearlyKPI()
        {
            var leader = await context.Users
                      .FirstOrDefaultAsync(u => u.Role == Role.Leader && !u.IsDeactivated);
            if (leader == null)
                return Unauthorized("User not found");

            var kpis = await context.AnnualKPIs
                .Where(k => k.UserId == leader.Id)
                .OrderBy(k => k.Year)
                .Select(k => new
                {
                    Year = k.Year,
                    KPI = k.KPI,
                })
                .ToListAsync();

            return Ok(kpis);
        }

        [HttpGet("member/yearly-kpi")]
        [Authorize(Roles="Member")]
        public async Task<IActionResult> GetMemberYearlyKPI()
        {
            var member = await userManager.GetUserAsync(User);
            if (member == null)
                return Unauthorized("User not found");

            var kpis = await context.AnnualKPIs
                .Where(k => k.UserId == member.Id)
                .OrderBy(k => k.Year)
                .Select(k => new
                {
                    Year = k.Year,
                    KPI = k.KPI,
                })
                .ToListAsync();

            return Ok(kpis);
        }



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
            DateTime? fromDate = null;
            DateTime? toDate = null;
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
                    (fromDate, toDate) = await GetDateRangeAsync(context.Tasks.Select(t => t.CreatedAt));
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
                    (fromDate, toDate) = await GetDateRangeAsync(context.Tasks.Select(t => t.CreatedAt));
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
                    var taskDates3 = await context.Tasks.Select(t => t.CreatedAt).ToListAsync();
                    if (taskDates3.Any())
                    {
                        fromDate = taskDates3.Min();
                        toDate = taskDates3.Max();
                    }
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
                    (fromDate, toDate) = await GetDateRangeAsync(context.Tasks.Select(t => t.CreatedAt));
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
                    (fromDate, toDate) = await GetDateRangeAsync(context.Tasks.Select(t => t.CreatedAt));
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
                    (fromDate, toDate) = await GetDateRangeAsync(context.PendingMemberRequests.Select(r => r.RequestedAt));
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
                FileContentType = file.ContentType,
                FromDate = fromDate,
                ToDate = toDate
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
                    report.FromDate,
                    report.ToDate,
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
                    r.FromDate,
                    r.ToDate,
                    r.FileData
                })
                .ToListAsync();

            return Ok(reports);
        }

        private async Task<(DateTime? From, DateTime? To)> GetDateRangeAsync(IQueryable<DateTime> datesQuery)
        {
            var dates = await datesQuery.ToListAsync();
            return dates.Any() ? (dates.Min(), dates.Max()) : (null, null);
        }


    }
}
