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
    
    // GET: Reports/NominationsWithScores
    public async Task<IActionResult> NominationsWithScores(int? cycleId)
    {
        // Get all award cycles for dropdown
        ViewBag.AwardCycles = await _context.AwardCycles
            .Include(c => c.AwardType)
            .OrderByDescending(c => c.Year)
            .ThenByDescending(c => c.Month)
            .Select(c => new { 
                c.CycleId, 
                Name = $"{c.AwardType.Name} - {c.Month}/{c.Year}" 
            })
            .ToListAsync();
        
        // If no cycle selected, get the latest one
        if (!cycleId.HasValue)
        {
            cycleId = await _context.AwardCycles
                .OrderByDescending(c => c.Year)
                .ThenByDescending(c => c.Month)
                .Select(c => c.CycleId)
                .FirstOrDefaultAsync();
        }
        
        ViewBag.SelectedCycleId = cycleId;
        
        if (!cycleId.HasValue)
        {
            return View(new List<dynamic>());
        }
        
        // Step 1: Get basic nomination data only
        var nominations = await _context.Nominations
            .Where(n => n.CycleId == cycleId)
            .Select(n => new {
                n.NominationId,
                n.EmployeeId,
                n.ManagerId,
                EmployeeName = n.Employee.FirstName + " " + n.Employee.LastName,
                EmployeeJobTitle = n.Employee.JobTitle,
                DepartmentName = n.Employee.Department.Description ?? n.Employee.Department.Name,
                ManagerName = n.Manager.FirstName + " " + n.Manager.LastName,
                AwardTypeName = n.AwardCycle.AwardType.Name,
                AwardTypeId = n.AwardCycle.AwardTypeId,
                n.IsWinner,
                n.CreatedAt,
                n.Title,
                n.InitiativeDetails,
                n.AttachmentPath,
                n.IsSelfNomination
            })
            .ToListAsync();
        
        if (!nominations.Any())
        {
            return View(new List<dynamic>());
        }
        
        var nominationIds = nominations.Select(n => n.NominationId).ToList();
        
        // Get team members for awards 2 and 3
        var teamMembers = await _context.GroupNominationMembers
            .Include(gm => gm.Employee)
            .Where(gm => nominationIds.Contains(gm.NominationId))
            .Select(gm => new {
                gm.NominationId,
                gm.EmployeeId,
                EmployeeName = gm.Employee.FirstName + " " + gm.Employee.LastName
            })
            .ToListAsync();
        
        // Debug: Log team members count
        System.Diagnostics.Debug.WriteLine($"Found {teamMembers.Count} team members for cycle {cycleId}");
        
        var teamMembersByNomination = teamMembers
            .GroupBy(tm => tm.NominationId)
            .ToDictionary(g => g.Key, g => g.Select(tm => tm.EmployeeName).ToList());
        
        // Step 2: Get manager scores (sum all ManagerScores for the nomination)
        var managerScoreData = await _context.ManagerScores
            .Where(ms => nominationIds.Contains(ms.NominationId) && ms.Score != null)
            .GroupBy(ms => ms.NominationId)
            .Select(g => new {
                NominationId = g.Key,
                TotalScore = g.Sum(ms => (double)ms.Score.Value)
            })
            .ToDictionaryAsync(x => x.NominationId, x => x.TotalScore);
        
        // Step 3: Get each committee member's total score (sum of their EvaluationScores)
        var committeeScoreData = await (from ev in _context.Evaluations
                                       join es in _context.EvaluationScores on ev.EvaluationId equals es.EvaluationId
                                       where nominationIds.Contains(ev.NominationId) && es.Score != null
                                       group es by new { ev.NominationId, ev.EvaluationId } into g
                                       select new {
                                           NominationId = g.Key.NominationId,
                                           EvaluationId = g.Key.EvaluationId,
                                           TotalScore = g.Sum(x => (double)x.Score.Value)
                                       })
                                       .GroupBy(x => x.NominationId)
                                       .Select(g => new {
                                           NominationId = g.Key,
                                           CommitteeScores = g.Select(x => x.TotalScore).ToList(),
                                           AvgScore = g.Average(x => x.TotalScore),
                                           Count = g.Count()
                                       })
                                       .ToDictionaryAsync(x => x.NominationId, x => new { x.CommitteeScores, x.AvgScore, x.Count });
        
        // Step 4: Get committee member details with their total scores
        var committeeDetails = await (from ev in _context.Evaluations
                                     join cm in _context.CommitteeMembers on ev.CommitteeMemberId equals cm.Id
                                     join emp in _context.Employees on cm.EmployeeId equals emp.EmployeeId
                                     join es in _context.EvaluationScores on ev.EvaluationId equals es.EvaluationId
                                     where nominationIds.Contains(ev.NominationId) && es.Score != null
                                     group new { ev, emp, es } by new { ev.NominationId, ev.EvaluationId, emp.FirstName, emp.LastName } into g
                                     select new {
                                         NominationId = g.Key.NominationId,
                                         EvaluationId = g.Key.EvaluationId,
                                         MemberName = g.Key.FirstName + " " + g.Key.LastName,
                                         Score = g.Sum(x => (double)x.es.Score.Value)  // Sum, not average
                                     })
                                     .ToListAsync();
        
        var committeeByNomination = committeeDetails.GroupBy(x => x.NominationId)
                                                    .ToDictionary(g => g.Key, g => g.ToList());
        
        // Step 5: Combine all data
        var nominationsWithScores = nominations.Select(n => {
            var managerScore = managerScoreData.ContainsKey(n.NominationId) ? managerScoreData[n.NominationId] : 0;
            var committeeData = committeeScoreData.ContainsKey(n.NominationId) ? committeeScoreData[n.NominationId] : null;
            var committeeMembers = committeeByNomination.ContainsKey(n.NominationId) ? committeeByNomination[n.NominationId].Cast<dynamic>().ToList() : new List<dynamic>();
            var teamMemberNames = teamMembersByNomination.ContainsKey(n.NominationId) ? teamMembersByNomination[n.NominationId] : new List<string>();
            
            // Calculate final score as average of manager + all committee members
            double finalScore = 0;
            var allScores = new List<double>();
            
            if (managerScore > 0)
                allScores.Add(managerScore);
            
            if (committeeData != null && committeeData.CommitteeScores.Any())
                allScores.AddRange(committeeData.CommitteeScores);
            
            if (allScores.Any())
                finalScore = allScores.Average();
            
            return new {
                Nomination = new {
                    NominationId = n.NominationId,
                    Employee = new { 
                        EmployeeId = n.EmployeeId,
                        FirstName = n.EmployeeName.Split(' ')[0],
                        LastName = n.EmployeeName.Contains(' ') ? n.EmployeeName.Substring(n.EmployeeName.IndexOf(' ') + 1) : "",
                        JobTitle = n.EmployeeJobTitle,
                        Department = new { 
                            Name = n.DepartmentName,
                            Description = n.DepartmentName 
                        }
                    },
                    Manager = new {
                        FirstName = n.ManagerName != null && n.ManagerName.Contains(' ') ? n.ManagerName.Split(' ')[0] : n.ManagerName ?? "",
                        LastName = n.ManagerName != null && n.ManagerName.Contains(' ') ? n.ManagerName.Substring(n.ManagerName.IndexOf(' ') + 1) : ""
                    },
                    AwardCycle = new {
                        AwardType = new {
                            Name = n.AwardTypeName,
                            AwardTypeId = n.AwardTypeId
                        }
                    },
                    IsWinner = n.IsWinner,
                    CreatedAt = n.CreatedAt,
                    Title = n.Title,
                    InitiativeDetails = n.InitiativeDetails,
                    AttachmentPath = n.AttachmentPath,
                    IsSelfNomination = n.IsSelfNomination
                },
                TeamMembers = teamMemberNames,
                ShowTeamMembers = (n.AwardTypeId == 2 || n.AwardTypeId == 3),
                ManagerScore = Math.Round(managerScore, 1),
                CommitteeScores = committeeMembers.Select(cm => new {
                    CommitteeMember = cm.MemberName,
                    Score = Math.Round(cm.Score, 1)
                }).ToList(),
                AverageCommitteeScore = committeeData != null ? Math.Round(committeeData.AvgScore, 1) : 0,
                FinalScore = Math.Round(finalScore, 1)
            };
        }).OrderByDescending(x => x.FinalScore).ToList();
        
        return View(nominationsWithScores);
    }
    
    // GET: Reports/ExportNominationsWithScores
    public async Task<IActionResult> ExportNominationsWithScores(int? cycleId)
    {
        // If no cycle selected, get the latest one
        if (!cycleId.HasValue)
        {
            cycleId = await _context.AwardCycles
                .OrderByDescending(c => c.Year)
                .ThenByDescending(c => c.Month)
                .Select(c => c.CycleId)
                .FirstOrDefaultAsync();
        }
        
        if (!cycleId.HasValue)
        {
            return NotFound();
        }
        
        // Get nominations with minimal necessary data for performance
        var nominations = await _context.Nominations
            .Include(n => n.Employee)
                .ThenInclude(e => e.Department)
            .Include(n => n.Manager)
            .Include(n => n.AwardCycle)
                .ThenInclude(ac => ac.AwardType)
            .Include(n => n.ManagerScores)
            .Include(n => n.Evaluations)
                .ThenInclude(e => e.CommitteeMember)
                    .ThenInclude(cm => cm.Employee)
            .Include(n => n.Evaluations)
                .ThenInclude(e => e.EvaluationScores)
            .Include(n => n.GroupMembers)
                .ThenInclude(gm => gm.Employee)
            .Where(n => n.CycleId == cycleId)
            .ToListAsync();
            
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("تقرير الترشيحات والدرجات");
            
            // RTL support
            worksheet.View.RightToLeft = true;
            
            // Headers
            worksheet.Cells[1, 1].Value = "الموظف";
            worksheet.Cells[1, 2].Value = "العنوان";
            worksheet.Cells[1, 3].Value = "تفاصيل المبادرة";
            worksheet.Cells[1, 4].Value = "أعضاء الفريق";
            worksheet.Cells[1, 5].Value = "الدائرة";
            worksheet.Cells[1, 6].Value = "المدير";
            worksheet.Cells[1, 7].Value = "نوع الجائزة";
            worksheet.Cells[1, 8].Value = "درجة المدير";
            worksheet.Cells[1, 9].Value = "متوسط درجة اللجنة";
            worksheet.Cells[1, 10].Value = "الدرجة النهائية";
            worksheet.Cells[1, 11].Value = "حالة الترشيح";
            
            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, 11])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }
            
            // Data
            int row = 2;
            foreach (var nomination in nominations.OrderByDescending(n => CalculateFinalScore(n)))
            {
                worksheet.Cells[row, 1].Value = $"{nomination.Employee?.FirstName} {nomination.Employee?.LastName}";
                
                // Title
                worksheet.Cells[row, 2].Value = string.IsNullOrEmpty(nomination.Title) ? "-" : nomination.Title;
                
                // Initiative Details
                worksheet.Cells[row, 3].Value = string.IsNullOrEmpty(nomination.InitiativeDetails) ? "-" : nomination.InitiativeDetails;
                worksheet.Cells[row, 3].Style.WrapText = true; // Wrap text for long details
                
                // Team members for award types 2 and 3
                string teamMembers = "";
                if ((nomination.AwardCycle?.AwardType?.AwardTypeId == 2 || nomination.AwardCycle?.AwardType?.AwardTypeId == 3) 
                    && nomination.GroupMembers != null && nomination.GroupMembers.Any())
                {
                    teamMembers = string.Join(", ", nomination.GroupMembers.Select(gm => 
                        $"{gm.Employee?.FirstName} {gm.Employee?.LastName}"));
                }
                worksheet.Cells[row, 4].Value = string.IsNullOrEmpty(teamMembers) ? "-" : teamMembers;
                
                worksheet.Cells[row, 5].Value = nomination.Employee?.Department?.Description ?? nomination.Employee?.Department?.Name;
                worksheet.Cells[row, 6].Value = nomination.Manager != null ? $"{nomination.Manager?.FirstName} {nomination.Manager?.LastName}" : "-";
                worksheet.Cells[row, 7].Value = nomination.AwardCycle?.AwardType?.Name;
                worksheet.Cells[row, 8].Value = Math.Round(CalculateManagerScore(nomination), 2);
                
                var avgCommitteeScore = nomination.Evaluations.Any() ? 
                    nomination.Evaluations.Average(e => CalculateEvaluationScore(e, nomination.AwardCycle.AwardType.Criteria)) : 0;
                worksheet.Cells[row, 9].Value = Math.Round(avgCommitteeScore, 2);
                
                worksheet.Cells[row, 10].Value = Math.Round(CalculateFinalScore(nomination), 2);
                worksheet.Cells[row, 11].Value = nomination.IsWinner == 1 ? "فائز" : "مرشح";
                
                row++;
            }
            
            // Auto-fit columns and set specific widths
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            
            // Set specific column widths for better readability
            worksheet.Column(1).Width = 20;  // Employee name
            worksheet.Column(2).Width = 30;  // Title
            worksheet.Column(3).Width = 50;  // Initiative Details - wider for text
            worksheet.Column(4).Width = 25;  // Team members
            worksheet.Column(5).Width = 20;  // Department
            worksheet.Column(6).Width = 20;  // Manager
            worksheet.Column(7).Width = 20;  // Award type
            
            // Set row height for rows with wrapped text
            for (int i = 2; i < row; i++)
            {
                worksheet.Row(i).Height = 30; // Set minimum height for better readability
            }
            
            // Return as file
            var cycleInfo = await _context.AwardCycles
                .Include(c => c.AwardType)
                .FirstOrDefaultAsync(c => c.CycleId == cycleId);
                
            var fileName = $"تقرير_الترشيحات_{cycleInfo?.AwardType?.Name}_{cycleInfo?.Month}_{cycleInfo?.Year}.xlsx";
            
            return File(package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
    
    // Simplified scoring methods - calculate total score out of 100
    private double CalculateManagerScore(Nomination nomination)
    {
        if (!nomination.ManagerScores.Any()) return 0;
        
        // Sum all manager scores (already out of their max values)
        var validScores = nomination.ManagerScores.Where(ms => ms.Score.HasValue);
        if (!validScores.Any()) return 0;
        
        // Since subcriteria are designed to total 100, just sum them
        return validScores.Sum(ms => (double)ms.Score.Value);
    }
    
    private double CalculateEvaluationScore(Evaluation evaluation, ICollection<Criterion> criteria = null)
    {
        if (!evaluation.EvaluationScores.Any()) return 0;
        
        // Sum all evaluation scores (already out of their max values)
        var validScores = evaluation.EvaluationScores.Where(es => es.Score.HasValue);
        if (!validScores.Any()) return 0;
        
        // Since subcriteria are designed to total 100, just sum them
        return validScores.Sum(es => (double)es.Score.Value);
    }
    
    private double CalculateFinalScore(Nomination nomination)
    {
        // Calculate average of manager score + all committee member scores
        var allScores = new List<double>();
        
        // Add manager score (sum of all ManagerScores)
        var managerScore = CalculateManagerScore(nomination);
        if (managerScore > 0) allScores.Add(managerScore);
        
        // Add each committee member's score (sum of their EvaluationScores)
        foreach (var evaluation in nomination.Evaluations)
        {
            var evalScore = CalculateEvaluationScore(evaluation);
            if (evalScore > 0) allScores.Add(evalScore);
        }
        
        // Return average: (manager_total + committee1_total + committee2_total + ...) / number_of_evaluators
        return allScores.Any() ? allScores.Average() : 0;
    }

    // GET: Reports/DepartmentNominations
    public async Task<IActionResult> DepartmentNominations()
    {
        // Get all nominations with only the necessary data
        var nominations = await _context.Nominations
            .Where(n => n.Employee != null && n.Employee.Department != null)
            .Select(n => new {
                n.NominationId,
                DepartmentId = n.Employee.DepartmentId,
                DepartmentName = n.Employee.Department.Name,
                CycleStatus = n.AwardCycle.Status,
                n.CreatedAt
            })
            .Distinct()
            .ToListAsync();

        // Group and count in memory
        var departmentStats = nominations
            .GroupBy(n => new { n.DepartmentId, n.DepartmentName })
            .Select(g => new DepartmentNominationStats
            {
                DepartmentId = (int)g.Key.DepartmentId!,
                DepartmentName = g.Key.DepartmentName!,
                TotalNominations = g.Select(n => n.NominationId).Distinct().Count(),
                CurrentCycleNominations = g.Where(n => n.CycleStatus == CycleStatus.Nomination || n.CycleStatus == CycleStatus.Evaluating)
                    .Select(n => n.NominationId).Distinct().Count(),
                LastNominationDate = g.Max(n => n.CreatedAt)
            })
            .OrderByDescending(d => d.TotalNominations)
            .ToList();

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

        // Get all nominations with only the necessary data
        var nominations = await _context.Nominations
            .Where(n => n.Employee != null && n.Employee.Department != null)
            .Select(n => new {
                n.NominationId,
                DepartmentId = n.Employee.DepartmentId,
                DepartmentName = n.Employee.Department.Name,
                CycleStatus = n.AwardCycle.Status,
                n.CreatedAt
            })
            .Distinct()
            .ToListAsync();

        // Group and count in memory
        var departmentStats = nominations
            .GroupBy(n => new { n.DepartmentId, n.DepartmentName })
            .Select(g => new DepartmentNominationStats
            {
                DepartmentId = (int)g.Key.DepartmentId!,
                DepartmentName = g.Key.DepartmentName!,
                TotalNominations = g.Select(n => n.NominationId).Distinct().Count(),
                CurrentCycleNominations = g.Where(n => n.CycleStatus == CycleStatus.Nomination || n.CycleStatus == CycleStatus.Evaluating)
                    .Select(n => n.NominationId).Distinct().Count(),
                LastNominationDate = g.Max(n => n.CreatedAt)
            })
            .OrderByDescending(d => d.TotalNominations)
            .ToList();

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

    // GET: Reports/Winners
    public async Task<IActionResult> Winners(int? cycleId)
    {
        // Get all award cycles for dropdown
        ViewBag.AwardCycles = await _context.AwardCycles
            .Include(c => c.AwardType)
            .OrderByDescending(c => c.Year)
            .ThenByDescending(c => c.Month)
            .Select(c => new { 
                c.CycleId, 
                Name = $"{c.AwardType.Name} - {c.Month}/{c.Year}" 
            })
            .ToListAsync();
        
        // If no cycle selected, get the latest one
        if (!cycleId.HasValue)
        {
            cycleId = await _context.AwardCycles
                .OrderByDescending(c => c.Year)
                .ThenByDescending(c => c.Month)
                .Select(c => c.CycleId)
                .FirstOrDefaultAsync();
        }
        
        ViewBag.SelectedCycleId = cycleId;
        
        if (!cycleId.HasValue)
        {
            return View(new List<Nomination>());
        }
        
        // Get winners for selected cycle
        var winners = await _context.Nominations
            .Include(n => n.Employee)
                .ThenInclude(e => e.Department)
            .Include(n => n.Manager)
            .Include(n => n.AwardCycle)
                .ThenInclude(ac => ac.AwardType)
            .Where(n => n.CycleId == cycleId && n.IsWinner == 1)
            .OrderBy(n => n.Employee.Department != null ? n.Employee.Department.Name : "")
            .ThenBy(n => n.Employee != null ? n.Employee.FirstName : "")
            .ToListAsync();
            
        return View(winners);
    }

    // GET: Reports/ExportWinners
    public async Task<IActionResult> ExportWinners(int? cycleId)
    {
        // If no cycle selected, get the latest one
        if (!cycleId.HasValue)
        {
            cycleId = await _context.AwardCycles
                .OrderByDescending(c => c.Year)
                .ThenByDescending(c => c.Month)
                .Select(c => c.CycleId)
                .FirstOrDefaultAsync();
        }
        
        if (!cycleId.HasValue)
        {
            return NotFound();
        }
        
        // Get winners for selected cycle
        var winners = await _context.Nominations
            .Include(n => n.Employee)
                .ThenInclude(e => e.Department)
            .Include(n => n.Manager)
            .Include(n => n.AwardCycle)
                .ThenInclude(ac => ac.AwardType)
            .Where(n => n.CycleId == cycleId && n.IsWinner == 1)
            .OrderBy(n => n.Employee.Department != null ? n.Employee.Department.Name : "")
            .ThenBy(n => n.Employee != null ? n.Employee.FirstName : "")
            .ToListAsync();
            
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("الفائزون");
            
            // RTL support
            worksheet.View.RightToLeft = true;
            
            // Headers
            worksheet.Cells[1, 1].Value = "الموظف";
            worksheet.Cells[1, 2].Value = "رقم الموظف";
            worksheet.Cells[1, 3].Value = "الدائرة";
            worksheet.Cells[1, 4].Value = "المدير";
            worksheet.Cells[1, 5].Value = "نوع الجائزة";
            worksheet.Cells[1, 6].Value = "الشهر";
            worksheet.Cells[1, 7].Value = "السنة";
            
            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 215, 0)); // Gold color for winners
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }
            
            // Data
            int row = 2;
            foreach (var winner in winners)
            {
                worksheet.Cells[row, 1].Value = $"{winner.Employee?.FirstName} {winner.Employee?.LastName}";
                worksheet.Cells[row, 2].Value = winner.Employee?.EmployeeId;
                worksheet.Cells[row, 3].Value = winner.Employee?.Department?.Name;
                worksheet.Cells[row, 4].Value = $"{winner.Manager?.FirstName} {winner.Manager?.LastName}";
                worksheet.Cells[row, 5].Value = winner.AwardCycle?.AwardType?.Name;
                worksheet.Cells[row, 6].Value = winner.AwardCycle?.Month;
                worksheet.Cells[row, 7].Value = winner.AwardCycle?.Year;
                
                row++;
            }
            
            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            
            // Return as file
            var cycleInfo = await _context.AwardCycles
                .Include(c => c.AwardType)
                .FirstOrDefaultAsync(c => c.CycleId == cycleId);
                
            var fileName = $"الفائزون_{cycleInfo?.AwardType?.Name}_{cycleInfo?.Month}_{cycleInfo?.Year}.xlsx";
            
            return File(package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
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