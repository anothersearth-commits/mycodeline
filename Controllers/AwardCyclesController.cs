using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;
using System.Linq;

namespace EOM.Web.Controllers;

[Authorize(Roles = "EOM-Admin,EOM-Committee")]
public class AwardCyclesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AwardCyclesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: AwardCycles
    public async Task<IActionResult> Index()
    {
        var cycles = await _context.AwardCycles
            .Include(a => a.AwardType)
            .OrderByDescending(a => a.Year)
            .ThenByDescending(a => a.Month)
            .ToListAsync();
        
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

    private bool AwardCycleExists(int id)
    {
        return _context.AwardCycles.Any(e => e.CycleId == id);
    }
}