using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;

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
    public IActionResult Create()
    {
        ViewData["AwardTypeId"] = new SelectList(_context.AwardTypes.Where(at => at.IsActive), "AwardTypeId", "Name");
        return View();
    }

    // POST: AwardCycles/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("AwardTypeId,Month,Year,NominationStart,NominationEnd,Status")] AwardCycle awardCycle)
    {
        if (ModelState.IsValid)
        {
            _context.Add(awardCycle);
            await _context.SaveChangesAsync();
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