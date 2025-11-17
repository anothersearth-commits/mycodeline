using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;
using System.Linq;
using OfficeOpenXml;

namespace EOM.Web.Controllers;

[Authorize(Roles = "EOM-Admin,EOM-Committee-Lead")]
public class AwardCyclesController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly Services.CycleRankingService _rankingService;

    public AwardCyclesController(ApplicationDbContext context)
    {
        _context = context;
        _rankingService = new Services.CycleRankingService(context);
    }

    // GET: AwardCycles
    public async Task<IActionResult> Index()
    {
        var cycles = await _context.AwardCycles
            .Include(a => a.AwardType)
            .OrderByDescending(a => a.Year)
            .ThenByDescending(a => a.Month)
            .ToListAsync();

        // Calculate evaluation completion status for each cycle
        var evaluationStatus = new Dictionary<int, bool>();
        foreach (var cycle in cycles)
        {
            if (cycle.Status == CycleStatus.Evaluating)
            {
                // For Mode A (by-directorate committees):
                // Expected evaluations = sum over each directorate of:
                //   (#nominations in that directorate) * (#committee members for (AwardTypeId, Directorate))
                var nominations = await _context.Nominations
                    .Include(n => n.Employee)
                    .Where(n => n.CycleId == cycle.CycleId)
                    .ToListAsync();

                var expectedEvaluations = 0;

                foreach (var group in nominations.GroupBy(n => n.Employee!.Directorate))
                {
                    var directorate = group.Key;
                    var nominationsInDirectorate = group.Count();

                    var committeeCountForDirectorate = await _context.CommitteeMembers
                        .Where(cm => cm.IsActive &&
                                     cm.AwardTypeId == cycle.AwardTypeId &&
                                     cm.Directorate == directorate)
                        .CountAsync();

                    expectedEvaluations += nominationsInDirectorate * committeeCountForDirectorate;
                }

                var actualEvaluations = await _context.Evaluations
                    .Where(e => e.Nomination.CycleId == cycle.CycleId)
                    .CountAsync();

                evaluationStatus[cycle.CycleId] = actualEvaluations >= expectedEvaluations;
            }
            else
            {
                evaluationStatus[cycle.CycleId] = true; // Not applicable for non-evaluating cycles
            }
        }

        ViewBag.EvaluationStatus = evaluationStatus;
        return View(cycles);
    }

    // GET: AwardCycles/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var awardCycle = await _context.AwardCycles
            .Include(a => a.AwardType)
            .Include(a => a.Nominations)
                .ThenInclude(n => n.Employee)
                .ThenInclude(e => e.Department)
            .Include(a => a.Nominations)
                .ThenInclude(n => n.Manager)
            .Include(a => a.Nominations)
                .ThenInclude(n => n.Evaluations)
            .FirstOrDefaultAsync(m => m.CycleId == id);
        
        if (awardCycle == null)
        {
            return NotFound();
        }

        return View(awardCycle);
    }

    // GET: AwardCycles/Create
    public async Task<IActionResult> Create()
    {
        // Allow multiple active cycles of different award types
        // No longer blocking new cycle creation based on existing active cycles

        var awardTypes = _context.AwardTypes.Where(at => at.IsActive).ToList();

        if (!awardTypes.Any())
        {
            TempData["Error"] = "لا يوجد أي أنواع جوائز مفعّلة. الرجاء إنشاء نوع جائزة أولاً من لوحة المسؤول.";
            return RedirectToAction(nameof(Index));
        }

        // If only one active award type exists, select it by default
        int? selectedAwardTypeId = awardTypes.Count == 1 ? awardTypes.First().AwardTypeId : null;

        ViewData["AwardTypeId"] = new SelectList(awardTypes, "AwardTypeId", "Name", selectedAwardTypeId);

        // Debug: Check if we have award types
        System.Diagnostics.Debug.WriteLine($"Found {awardTypes.Count} active award types");

        return View();
    }

    // POST: AwardCycles/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("AwardTypeId,Month,Year,NominationStart,NominationEnd,Status")] AwardCycle awardCycle)
    {
        // Check if there's already an active cycle for the SAME award type and period
        var activeCycleForSameType = await _context.AwardCycles
            .Where(c => c.AwardTypeId == awardCycle.AwardTypeId 
                       && c.Month == awardCycle.Month 
                       && c.Year == awardCycle.Year
                       && (c.Status == CycleStatus.Nomination || c.Status == CycleStatus.Evaluating))
            .FirstOrDefaultAsync();

        if (activeCycleForSameType != null)
        {
            TempData["Error"] = "يوجد دورة نشطة بالفعل لنفس نوع الجائزة والفترة الزمنية.";
            return RedirectToAction(nameof(Index));
        }

        // Debug: Log model state errors (removed user-facing error message)
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value?.Errors.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>() })
                .ToList();
            
            // For debugging only - log to console
            System.Diagnostics.Debug.WriteLine($"Model validation errors: {string.Join(", ", errors.Select(e => $"{e.Field}: {string.Join(", ", e.Errors)}"))}");
        }
        
        if (ModelState.IsValid)
        {
            _context.Add(awardCycle);
            await _context.SaveChangesAsync();
            TempData.Remove("Error"); // Clear any previous error messages
            TempData["Success"] = "تم إنشاء الدورة بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        
        ViewData["AwardTypeId"] = new SelectList(_context.AwardTypes.Where(at => at.IsActive), "AwardTypeId", "Name", awardCycle.AwardTypeId);
        return View(awardCycle);
    }

    // GET: AwardCycles/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var awardCycle = await _context.AwardCycles.FindAsync(id);
        if (awardCycle == null)
        {
            return NotFound();
        }
        ViewData["AwardTypeId"] = new SelectList(_context.AwardTypes.Where(at => at.IsActive), "AwardTypeId", "Name", awardCycle.AwardTypeId);
        return View(awardCycle);
    }

    // POST: AwardCycles/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CycleId,AwardTypeId,Month,Year,NominationStart,NominationEnd,Status")] AwardCycle awardCycle)
    {
        if (id != awardCycle.CycleId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(awardCycle);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AwardCycleExists(awardCycle.CycleId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["AwardTypeId"] = new SelectList(_context.AwardTypes.Where(at => at.IsActive), "AwardTypeId", "Name", awardCycle.AwardTypeId);
        return View(awardCycle);
    }

    // GET: AwardCycles/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var awardCycle = await _context.AwardCycles
            .Include(a => a.AwardType)
            .FirstOrDefaultAsync(m => m.CycleId == id);
        if (awardCycle == null)
        {
            return NotFound();
        }

        return View(awardCycle);
    }

    // POST: AwardCycles/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var awardCycle = await _context.AwardCycles.FindAsync(id);
        if (awardCycle != null)
        {
            _context.AwardCycles.Remove(awardCycle);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: AwardCycles/OpenNomination/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OpenNomination(int id)
    {
        var cycle = await _context.AwardCycles.FindAsync(id);
        if (cycle == null)
        {
            return NotFound();
        }

        // Check if there's already an active cycle for the SAME award type
        var cycleToOpen = await _context.AwardCycles
            .Include(c => c.AwardType)
            .FirstOrDefaultAsync(c => c.CycleId == id);
            
        if (cycleToOpen == null)
        {
            return NotFound();
        }
        
        var activeCycleForSameType = await _context.AwardCycles
            .Where(c => c.AwardTypeId == cycleToOpen.AwardTypeId
                       && c.CycleId != id
                       && (c.Status == CycleStatus.Nomination || c.Status == CycleStatus.Evaluating))
            .FirstOrDefaultAsync();

        if (activeCycleForSameType != null)
        {
            TempData["Error"] = $"يوجد دورة نشطة بالفعل لنفس نوع الجائزة ({cycleToOpen.AwardType?.Name}). يجب إغلاقها أولاً.";
            return RedirectToAction(nameof(Index));
        }

        cycle.Status = CycleStatus.Nomination;
        await _context.SaveChangesAsync();

        TempData["Success"] = "تم فتح باب الترشيحات بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    // POST: AwardCycles/CloseNomination/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseNomination(int id)
    {
        var cycle = await _context.AwardCycles.FindAsync(id);
        if (cycle == null)
        {
            return NotFound();
        }

        cycle.Status = CycleStatus.Evaluating;
        await _context.SaveChangesAsync();

        TempData["Success"] = "تم إغلاق باب الترشيحات وفتح باب التقييم.";
        return RedirectToAction(nameof(Index));
    }

    // POST: AwardCycles/CloseCycle/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseCycle(int id)
    {
        var cycle = await _context.AwardCycles.FindAsync(id);
        if (cycle == null)
        {
            return NotFound();
        }

        cycle.Status = CycleStatus.Closed;
        await _context.SaveChangesAsync();

        TempData["Success"] = "تم إغلاق الدورة بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    // POST: AwardCycles/ReopenForEvaluation/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReopenForEvaluation(int id)
    {
        var cycle = await _context.AwardCycles.FindAsync(id);
        if (cycle == null)
        {
            return NotFound();
        }

        // Only allow reopening if cycle is closed and no winner was selected
        if (cycle.Status == CycleStatus.Closed)
        {
            var hasWinner = await _context.Nominations
                .AnyAsync(n => n.CycleId == id && n.IsWinner == 1);

            if (!hasWinner)
            {
                cycle.Status = CycleStatus.Evaluating;
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إعادة فتح الدورة للتقييم. يمكنك الآن اختيار الفائز.";
            }
            else
            {
                TempData["Error"] = "لا يمكن إعادة فتح الدورة لأنه تم اختيار فائز بالفعل.";
            }
        }
        else
        {
            TempData["Error"] = "يمكن إعادة فتح الدورات المغلقة فقط.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: AwardCycles/SelectWinner/5
    [HttpGet]
    public async Task<IActionResult> SelectWinner(int id)
    {
        var cycle = await _context.AwardCycles.Include(c => c.AwardType).FirstOrDefaultAsync(c => c.CycleId == id);
        if (cycle == null)
        {
            return NotFound();
        }

        // Check if there are preliminary winners (stage 2)
        var hasPreliminaryWinners = await _context.Nominations
            .AnyAsync(n => n.CycleId == id && n.IsWinner == 2);

        if (hasPreliminaryWinners)
        {
            // Stage 2: Show only preliminary winners for final confirmation
            var preliminaryWinners = await _rankingService.GetRankedNominationsFastAsync(id);
            var preliminaryFiltered = preliminaryWinners.Where(rn => rn.Nomination.IsWinner == 2).ToList();

            var vm2 = new NominationRankingViewModel
            {
                CycleId = id,
                AwardType = cycle.AwardType,
                RankedNominations = preliminaryFiltered,
                IsSecondStage = true
            };

            return View(vm2);
        }

        // Stage 1: Normal winner selection process
        // Use more efficient counting queries
        var activeCommitteeCount = await _context.CommitteeMembers
            .Where(cm => cm.IsActive)
            .CountAsync();

        var nominationCount = await _context.Nominations
            .Where(n => n.CycleId == id)
            .CountAsync();

        // Calculate total expected evaluations
        int expectedEvaluations = activeCommitteeCount * nominationCount;

        // Calculate actual evaluations - more efficient query
        int actualEvaluations = await _context.Evaluations
            .Join(_context.Nominations,
                e => e.NominationId,
                n => n.NominationId,
                (e, n) => new { e, n })
            .Where(en => en.n.CycleId == id)
            .CountAsync();

        // If not all evaluations are complete, redirect with error message
        if (actualEvaluations < expectedEvaluations)
        {
            int pendingEvaluations = expectedEvaluations - actualEvaluations;
            TempData["Error"] = $"لا يمكن اختيار الفائز حتى يكمل جميع أعضاء اللجنة تقييم جميع المرشحين. يتبقى {pendingEvaluations} تقييم.";
            return RedirectToAction("Details", new { id = id });
        }

        var ranked = await _rankingService.GetRankedNominationsFastAsync(id);

        var vm = new NominationRankingViewModel
        {
            CycleId = id,
            AwardType = cycle.AwardType,
            RankedNominations = ranked,
            IsSecondStage = false
        };

        return View(vm);
    }

    // POST: AwardCycles/SelectWinner/5 - Stage 1: Preliminary Selection
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectWinner(int id, int[] nominationIds)
    {
        // Double-check validation before allowing winner selection
        var allCommitteeMembers = await _context.CommitteeMembers
            .Where(cm => cm.IsActive)
            .ToListAsync();

        var allNominations = await _context.Nominations
            .Where(n => n.CycleId == id)
            .ToListAsync();

        int expectedEvaluations = allCommitteeMembers.Count * allNominations.Count;
        int actualEvaluations = await _context.Evaluations
            .Where(e => e.Nomination.CycleId == id)
            .CountAsync();

        if (actualEvaluations < expectedEvaluations)
        {
            int pendingEvaluations = expectedEvaluations - actualEvaluations;
            TempData["Error"] = $"لا يمكن اختيار الفائز حتى يكمل جميع أعضاء اللجنة تقييم جميع المرشحين. يتبقى {pendingEvaluations} تقييم.";
            return RedirectToAction("Details", new { id = id });
        }

        // Get the cycle with award type to check winner count
        var cycle = await _context.AwardCycles
            .Include(c => c.AwardType)
            .FirstOrDefaultAsync(c => c.CycleId == id);
        
        if (cycle == null)
        {
            return NotFound();
        }

        var requiredWinnerCount = cycle.AwardType.WinnerCount;
        
        // Validate that at least one winner is selected
        if (nominationIds == null || nominationIds.Length == 0)
        {
            TempData["Error"] = "يجب اختيار فائز واحد على الأقل.";
            return RedirectToAction("SelectWinner", new { id = id });
        }

        // Validate that the correct number of winners is selected
        if (nominationIds.Length != requiredWinnerCount)
        {
            TempData["Error"] = $"يجب اختيار {requiredWinnerCount} فائز بالضبط حسب نوع الجائزة.";
            return RedirectToAction("SelectWinner", new { id = id });
        }

        // Committee member who is selecting winners
        var committeeMemberId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Set all nominations to not winners first
        foreach (var nom in allNominations)
        {
            nom.IsWinner = 0;
        }

        // Set selected nominations as preliminary winners
        foreach (var nominationId in nominationIds)
        {
            var nomination = allNominations.FirstOrDefault(n => n.NominationId == nominationId);
            if (nomination != null)
            {
                nomination.IsWinner = 2; // Preliminary winner
                nomination.WonAt = DateTime.UtcNow;
                nomination.SelectedByCommitteeMemberId = committeeMemberId;
            }
        }

        await _context.SaveChangesAsync();

        var winnerCount = nominationIds.Length;
        TempData["Success"] = $"تم اختيار {winnerCount} فائز بشكل مبدئي. يمكنك الآن مراجعة الاختيار واعتماده نهائياً.";
        return RedirectToAction("SelectWinner", new { id });
    }

    // POST: AwardCycles/ConfirmWinners/5 - Stage 2: Final Confirmation
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmWinners(int id, int[] nominationIds)
    {
        var cycle = await _context.AwardCycles
            .Include(c => c.AwardType)
            .FirstOrDefaultAsync(c => c.CycleId == id);
        
        if (cycle == null)
        {
            return NotFound();
        }

        // Get all preliminary winners
        var preliminaryWinners = await _context.Nominations
            .Where(n => n.CycleId == id && n.IsWinner == 2)
            .ToListAsync();

        if (!preliminaryWinners.Any())
        {
            TempData["Error"] = "لا يوجد فائزون مبدئيون للاعتماد النهائي.";
            return RedirectToAction("SelectWinner", new { id });
        }

        // Validate that nominations selected for final confirmation are among preliminary winners
        if (nominationIds == null || nominationIds.Length == 0)
        {
            TempData["Error"] = "يجب اختيار الفائزين للاعتماد النهائي.";
            return RedirectToAction("SelectWinner", new { id });
        }

        var preliminaryWinnerIds = preliminaryWinners.Select(pw => pw.NominationId).ToHashSet();
        var invalidSelections = nominationIds.Where(nid => !preliminaryWinnerIds.Contains(nid)).ToList();
        
        if (invalidSelections.Any())
        {
            TempData["Error"] = "يمكن اعتماد الفائزين المبدئيين فقط.";
            return RedirectToAction("SelectWinner", new { id });
        }

        // Committee member who is confirming winners
        var committeeMemberId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Reset all preliminary winners to not winners
        foreach (var prelimWinner in preliminaryWinners)
        {
            prelimWinner.IsWinner = 0;
        }

        // Set selected nominations as final winners
        foreach (var nominationId in nominationIds)
        {
            var nomination = preliminaryWinners.FirstOrDefault(n => n.NominationId == nominationId);
            if (nomination != null)
            {
                nomination.IsWinner = 1; // Final winner
                nomination.WonAt = DateTime.UtcNow;
                nomination.SelectedByCommitteeMemberId = committeeMemberId;
            }
        }

        // Mark cycle as published
        cycle.Status = CycleStatus.Published;

        await _context.SaveChangesAsync();

        var winnerCount = nominationIds.Length;
        TempData["Success"] = $"تم اعتماد {winnerCount} فائز نهائياً وإنهاء الدورة بنجاح.";
        return RedirectToAction("Details", new { id });
    }

    private bool AwardCycleExists(int id)
    {
        return _context.AwardCycles.Any(e => e.CycleId == id);
    }

    // GET: AwardCycles/ExportWinnersToExcel/5
    [HttpGet]
    public async Task<IActionResult> ExportWinnersToExcel(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var cycle = await _context.AwardCycles
            .Include(c => c.AwardType)
            .FirstOrDefaultAsync(c => c.CycleId == id);

        if (cycle == null)
        {
            return NotFound();
        }

        // Get ranking information for the cycle
        var rankedNominations = await _rankingService.GetRankedNominationsFastAsync(id.Value);
        
        if (rankedNominations == null || !rankedNominations.Any())
        {
            return NotFound();
        }
        
        // Create view model to match the SelectWinner view
        var viewModel = new NominationRankingViewModel
        {
            CycleId = id.Value,
            AwardType = cycle.AwardType,
            RankedNominations = rankedNominations
        };

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add($"نتائج الدورة {id}");
        
        // Style header row
        var headerRange = worksheet.Cells[1, 1, 1, 11];
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        
        // Headers
        worksheet.Cells[1, 1].Value = "الترتيب";
        worksheet.Cells[1, 2].Value = "رقم الموظف";
        worksheet.Cells[1, 3].Value = "اسم الموظف";
        worksheet.Cells[1, 4].Value = "الوظيفة";
        worksheet.Cells[1, 5].Value = "الدائرة";
        worksheet.Cells[1, 6].Value = "المدير";
        worksheet.Cells[1, 7].Value = "متوسط الدرجة";
        worksheet.Cells[1, 8].Value = "الحالة";
        worksheet.Cells[1, 9].Value = "العنوان";
        worksheet.Cells[1, 10].Value = "تفاصيل المبادرة";
        worksheet.Cells[1, 11].Value = "أعضاء الفريق";

        // Data rows
        for (int i = 0; i < viewModel.RankedNominations.Count; i++)
        {
            var item = viewModel.RankedNominations[i];
            var row = i + 2;

            worksheet.Cells[row, 1].Value = i + 1;
            worksheet.Cells[row, 2].Value = item.Nomination.Employee?.EmployeeId;
            worksheet.Cells[row, 3].Value = $"{item.Nomination.Employee?.FirstName} {item.Nomination.Employee?.LastName}";
            worksheet.Cells[row, 4].Value = item.Nomination.Employee?.JobTitle;
            worksheet.Cells[row, 5].Value = item.Nomination.Employee?.Department?.Description ?? "غير محدد";
            
            // Manager info for Award Type 1, or "ترشيح ذاتي" for self-nominations
            if (viewModel.AwardType?.AwardTypeId == 1)
            {
                worksheet.Cells[row, 6].Value = $"{item.Nomination.Manager?.FirstName ?? ""} {item.Nomination.Manager?.LastName ?? ""}".Trim();
            }
            else
            {
                worksheet.Cells[row, 6].Value = "ترشيح ذاتي";
            }

            worksheet.Cells[row, 7].Value = item.AverageScore;
            worksheet.Cells[row, 7].Style.Numberformat.Format = "0.00";
            
            // Winner status
            string status = "مرشح";
            if (item.Nomination.IsWinner == 1)
            {
                status = "فائز نهائي";
                worksheet.Cells[row, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[row, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            }
            else if (item.Nomination.IsWinner == 2)
            {
                status = "فائز مبدئي";
                worksheet.Cells[row, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[row, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
            }
            worksheet.Cells[row, 8].Value = status;

            // Title and details for self-nomination awards
            worksheet.Cells[row, 9].Value = item.Nomination.Title ?? "";
            worksheet.Cells[row, 10].Value = item.Nomination.InitiativeDetails ?? "";
            
            // Group members if any
            if (item.Nomination.GroupMembers?.Any() == true)
            {
                var groupMemberNames = string.Join("، ", 
                    item.Nomination.GroupMembers.Select(gm => 
                        $"{gm.Employee?.FirstName} {gm.Employee?.LastName}".Trim()));
                worksheet.Cells[row, 11].Value = groupMemberNames;
            }
            else
            {
                worksheet.Cells[row, 11].Value = "";
            }
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        
        // Set minimum column widths for better readability
        for (int col = 1; col <= 11; col++)
        {
            if (worksheet.Column(col).Width < 10)
                worksheet.Column(col).Width = 10;
            if (col == 10 && worksheet.Column(col).Width > 50) // Limit details column width
                worksheet.Column(col).Width = 50;
        }

        // Add summary information at the bottom
        var summaryRow = viewModel.RankedNominations.Count + 4;
        worksheet.Cells[summaryRow, 1].Value = "ملخص الدورة";
        worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
        
        worksheet.Cells[summaryRow + 1, 1].Value = "نوع الجائزة:";
        worksheet.Cells[summaryRow + 1, 2].Value = viewModel.AwardType?.Name;
        
        worksheet.Cells[summaryRow + 2, 1].Value = "عدد المرشحين:";
        worksheet.Cells[summaryRow + 2, 2].Value = viewModel.RankedNominations.Count;
        
        worksheet.Cells[summaryRow + 3, 1].Value = "عدد الفائزين المطلوب:";
        worksheet.Cells[summaryRow + 3, 2].Value = viewModel.AwardType?.WinnerCount ?? 1;
        
        worksheet.Cells[summaryRow + 4, 1].Value = "تاريخ التصدير:";
        worksheet.Cells[summaryRow + 4, 2].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Winners_Cycle_{id}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}