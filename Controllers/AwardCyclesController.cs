using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;
using System.Linq;

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
        var allCommitteeMembers = await _context.CommitteeMembers
            .Where(cm => cm.IsActive)
            .ToListAsync();

        foreach (var cycle in cycles)
        {
            if (cycle.Status == CycleStatus.Evaluating)
            {
                var nominationsCount = await _context.Nominations
                    .Where(n => n.CycleId == cycle.CycleId)
                    .CountAsync();

                var expectedEvaluations = allCommitteeMembers.Count * nominationsCount;
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
        // Check if there's already an active cycle
        var activeCycle = await _context.AwardCycles
            .Where(c => c.Status == CycleStatus.Nomination || c.Status == CycleStatus.Evaluating)
            .FirstOrDefaultAsync();

        if (activeCycle != null)
        {
            TempData["Error"] = "لا يمكن إنشاء دورة جديدة. يوجد دورة نشطة بالفعل يجب إغلاقها أولاً.";
            return RedirectToAction(nameof(Index));
        }

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
        // Check if there's already an active cycle
        var activeCycle = await _context.AwardCycles
            .Where(c => c.Status == CycleStatus.Nomination || c.Status == CycleStatus.Evaluating)
            .FirstOrDefaultAsync();

        if (activeCycle != null)
        {
            TempData["Error"] = "لا يمكن إنشاء دورة جديدة. يوجد دورة نشطة بالفعل يجب إغلاقها أولاً.";
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

        // Check if there's already an active cycle
        var activeCycle = await _context.AwardCycles
            .Where(c => c.Status == CycleStatus.Nomination || c.Status == CycleStatus.Evaluating)
            .FirstOrDefaultAsync();

        if (activeCycle != null && activeCycle.CycleId != id)
        {
            TempData["Error"] = "لا يمكن فتح دورة جديدة. يوجد دورة نشطة بالفعل يجب إغلاقها أولاً.";
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
                .AnyAsync(n => n.CycleId == id && n.IsWinner);

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

        // Check if all committee members have evaluated all nominations
        var allCommitteeMembers = await _context.CommitteeMembers
            .Where(cm => cm.IsActive)
            .ToListAsync();

        var allNominations = await _context.Nominations
            .Where(n => n.CycleId == id)
            .ToListAsync();

        // Calculate total expected evaluations
        int expectedEvaluations = allCommitteeMembers.Count * allNominations.Count;

        // Calculate actual evaluations
        int actualEvaluations = await _context.Evaluations
            .Where(e => e.Nomination.CycleId == id)
            .CountAsync();

        // If not all evaluations are complete, redirect with error message
        if (actualEvaluations < expectedEvaluations)
        {
            int pendingEvaluations = expectedEvaluations - actualEvaluations;
            TempData["Error"] = $"لا يمكن اختيار الفائز حتى يكمل جميع أعضاء اللجنة تقييم جميع المرشحين. يتبقى {pendingEvaluations} تقييم.";
            return RedirectToAction("Details", new { id = id });
        }

        var ranked = await _rankingService.GetRankedNominationsAsync(id);

        var vm = new NominationRankingViewModel
        {
            CycleId = id,
            AwardType = cycle.AwardType,
            RankedNominations = ranked
        };

        return View(vm); // Views/AwardCycles/SelectWinner.cshtml (to be created)
    }

    // POST: AwardCycles/SelectWinner/5
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

        // Reuse allNominations already loaded above

        // Committee member who is selecting winners
        var committeeMemberId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Set all nominations to not winners first
        foreach (var nom in allNominations)
        {
            nom.IsWinner = false;
        }

        // Set selected nominations as winners
        foreach (var nominationId in nominationIds)
        {
            var nomination = allNominations.FirstOrDefault(n => n.NominationId == nominationId);
            if (nomination != null)
            {
                nomination.IsWinner = true;
                nomination.WonAt = DateTime.UtcNow;
                nomination.SelectedByCommitteeMemberId = committeeMemberId;
            }
        }

        // Mark cycle completed
        cycle.Status = CycleStatus.Published;

        await _context.SaveChangesAsync();

        var winnerCount = nominationIds.Length;
        TempData["Success"] = $"تم اختيار {winnerCount} فائز وإنهاء الدورة بنجاح.";
        return RedirectToAction("Details", new { id });
    }

    private bool AwardCycleExists(int id)
    {
        return _context.AwardCycles.Any(e => e.CycleId == id);
    }
}