using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;
using OfficeOpenXml;

namespace EOM.Web.Controllers;

[Authorize(Roles = "EOM-Admin,EOM-Committee-Lead")]
public class ReportsController : BaseController
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Reports
    public IActionResult Index()
    {
        return View();
    }

    // GET: Reports/DepartmentNominations
    public async Task<IActionResult> DepartmentNominations()
    {
        var departmentStats = await _context.Nominations
            .Include(n => n.Employee)
            .ThenInclude(e => e.Department)
            .Include(n => n.AwardCycle)
            .Where(n => n.Employee.Department != null)
            .GroupBy(n => new { 
                DepartmentId = n.Employee.DepartmentId, 
                DepartmentName = n.Employee.Department.Name 
            })
            .Select(g => new DepartmentNominationStats
            {
                DepartmentId = (int)g.Key.DepartmentId,
                DepartmentName = g.Key.DepartmentName,
                TotalNominations = (int)g.LongCount(),
                CurrentCycleNominations = (int)g.LongCount(n => n.AwardCycle.Status == CycleStatus.Nomination || n.AwardCycle.Status == CycleStatus.Evaluating),
                LastNominationDate = g.Max(n => n.CreatedAt)
            })
            .OrderByDescending(d => d.TotalNominations)
            .ToListAsync();

        return View(departmentStats);
    }

    // GET: Reports/IncompleteManagerScores
    public async Task<IActionResult> IncompleteManagerScores()
    {
        var incompleteNominations = await _context.Nominations
            .Include(n => n.Employee)
            .ThenInclude(e => e.Department)
            .Include(n => n.Manager)
            .Include(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .Include(n => n.ManagerScores)
            .Where(n => n.AwardCycle.Status == CycleStatus.Nomination || n.AwardCycle.Status == CycleStatus.Evaluating)
            .Where(n => !n.ManagerScores.Any() || n.ManagerScores.Any(ms => ms.Score == null))
            .OrderBy(n => n.AwardCycle.Year)
            .ThenBy(n => n.AwardCycle.Month)
            .ThenBy(n => n.Employee.Department.Name)
            .ToListAsync();

        return View(incompleteNominations);
    }

    // GET: Reports/ExportDepartmentNominations
    public async Task<IActionResult> ExportDepartmentNominations()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var departmentStats = await _context.Nominations
            .Include(n => n.Employee)
            .ThenInclude(e => e.Department)
            .Include(n => n.AwardCycle)
            .Where(n => n.Employee.Department != null)
            .GroupBy(n => new { 
                DepartmentId = n.Employee.DepartmentId, 
                DepartmentName = n.Employee.Department.Name 
            })
            .Select(g => new DepartmentNominationStats
            {
                DepartmentId = (int)g.Key.DepartmentId,
                DepartmentName = g.Key.DepartmentName,
                TotalNominations = (int)g.LongCount(),
                CurrentCycleNominations = (int)g.LongCount(n => n.AwardCycle.Status == CycleStatus.Nomination || n.AwardCycle.Status == CycleStatus.Evaluating),
                LastNominationDate = g.Max(n => n.CreatedAt)
            })
            .OrderByDescending(d => d.TotalNominations)
            .ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Department Nominations");
        
        // Headers
        worksheet.Cells[1, 1].Value = "اسم الدائرة";
        worksheet.Cells[1, 2].Value = "إجمالي الترشيحات";
        worksheet.Cells[1, 3].Value = "ترشيحات الدورة الحالية";
        worksheet.Cells[1, 4].Value = "آخر تاريخ ترشيح";

        // Data
        for (int i = 0; i < departmentStats.Count(); i++)
        {
            var dept = departmentStats[i];
            worksheet.Cells[i + 2, 1].Value = dept.DepartmentName;
            worksheet.Cells[i + 2, 2].Value = dept.TotalNominations;
            worksheet.Cells[i + 2, 3].Value = dept.CurrentCycleNominations;
            worksheet.Cells[i + 2, 4].Value = dept.LastNominationDate?.ToString("yyyy-MM-dd");
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Department_Nominations_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // GET: Reports/ExportIncompleteManagerScores  
    public async Task<IActionResult> ExportIncompleteManagerScores()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var incompleteNominations = await _context.Nominations
            .Include(n => n.Employee)
            .ThenInclude(e => e.Department)
            .Include(n => n.Manager)
            .Include(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .Include(n => n.ManagerScores)
            .Where(n => n.AwardCycle.Status == CycleStatus.Nomination || n.AwardCycle.Status == CycleStatus.Evaluating)
            .Where(n => !n.ManagerScores.Any() || n.ManagerScores.Any(ms => ms.Score == null))
            .ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Incomplete Manager Scores");
        
        // Headers
        worksheet.Cells[1, 1].Value = "دورة الجائزة";
        worksheet.Cells[1, 2].Value = "نوع الجائزة";
        worksheet.Cells[1, 3].Value = "اسم الموظف";
        worksheet.Cells[1, 4].Value = "الدائرة";
        worksheet.Cells[1, 5].Value = "اسم المدير";
        worksheet.Cells[1, 6].Value = "تاريخ الترشيح";
        worksheet.Cells[1, 7].Value = "حالة التقييم";

        // Data
        for (int i = 0; i < incompleteNominations.Count(); i++)
        {
            var nomination = incompleteNominations[i];
            worksheet.Cells[i + 2, 1].Value = $"{nomination.AwardCycle?.Month}/{nomination.AwardCycle?.Year}";
            worksheet.Cells[i + 2, 2].Value = nomination.AwardCycle?.AwardType?.Name;
            worksheet.Cells[i + 2, 3].Value = $"{nomination.Employee?.FirstName} {nomination.Employee?.LastName}";
            worksheet.Cells[i + 2, 4].Value = nomination.Employee?.Department?.Name;
            worksheet.Cells[i + 2, 5].Value = $"{nomination.Manager?.FirstName} {nomination.Manager?.LastName}";
            worksheet.Cells[i + 2, 6].Value = nomination.CreatedAt.ToString("yyyy-MM-dd");
            worksheet.Cells[i + 2, 7].Value = !nomination.ManagerScores.Any() ? "لم يتم التقييم" : "تقييم ناقص";
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Incomplete_Manager_Scores_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}

// Helper class for department statistics
public class DepartmentNominationStats
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalNominations { get; set; }
    public int CurrentCycleNominations { get; set; }
    public DateTime? LastNominationDate { get; set; }
}