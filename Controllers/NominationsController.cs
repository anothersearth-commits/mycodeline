using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;
using System.Security.Claims;

namespace EOM.Web.Controllers;

[Authorize]
public class NominationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public NominationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Nominations
    public async Task<IActionResult> Index()
    {
        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var currentEmployee = await _context.Employees.FindAsync(currentEmployeeId);
        
        IQueryable<Nomination> nominationsQuery = _context.Nominations
            .Include(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .Include(n => n.ManagerScores)
            .ThenInclude(ms => ms.SubCriteria);

        // If user is a manager, show only their nominations
        if (User.IsInRole("Manager"))
        {
            nominationsQuery = nominationsQuery.Where(n => n.ManagerId == currentEmployeeId);
        }
        // If admin, show all nominations
        
        var nominations = await nominationsQuery.ToListAsync();
        
        // Get manager's department quota if they're a manager
        if (User.IsInRole("Manager") && currentEmployee != null)
        {
            var departmentQuota = await _context.DepartmentQuotas
                .Include(dq => dq.AwardType)
                .FirstOrDefaultAsync(dq => dq.DepartmentId == currentEmployee.DepartmentId);
            
            ViewData["DepartmentQuota"] = departmentQuota;
        }
        
        return View(nominations);
    }

    // GET: Nominations/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var nomination = await _context.Nominations
            .Include(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .Include(n => n.ManagerScores)
            .ThenInclude(ms => ms.SubCriteria)
            .Include(n => n.Evaluations)
            .ThenInclude(e => e.EvaluationScores)
            .ThenInclude(es => es.SubCriteria)
            .FirstOrDefaultAsync(m => m.NominationId == id);
        
        if (nomination == null)
        {
            return NotFound();
        }

        return View(nomination);
    }

    // GET: Nominations/Create
    [Authorize(Roles = "Manager,EOM-Admin")]
    public async Task<IActionResult> Create()
    {
        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var currentEmployee = await _context.Employees.FindAsync(currentEmployeeId);
        
        if (currentEmployee == null)
        {
            return NotFound();
        }

        // Get active nomination cycles
        var activeCycles = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Where(ac => ac.Status == CycleStatus.Nomination)
            .ToListAsync();

        if (!activeCycles.Any())
        {
            ViewBag.Message = "لا توجد دورات ترشيح نشطة حالياً";
            return View();
        }

        // Get department quota for the manager
        var departmentQuota = await _context.DepartmentQuotas
            .Include(dq => dq.AwardType)
            .FirstOrDefaultAsync(dq => dq.DepartmentId == currentEmployee.DepartmentId);

        // Get employees in the same department (managed by this manager)
        var departmentEmployees = await _context.Employees
            .Where(e => e.DepartmentId == currentEmployee.DepartmentId 
                       && e.EmployeeId != currentEmployeeId 
                       && e.IsActive)
            .ToListAsync();

        // Check how many nominations this manager has made for current cycles
        var existingNominations = await _context.Nominations
            .Where(n => n.ManagerId == currentEmployeeId && activeCycles.Select(ac => ac.CycleId).Contains(n.CycleId))
            .CountAsync();

        ViewData["CycleId"] = new SelectList(activeCycles, "CycleId", "AwardType.Name");
        ViewData["DepartmentEmployees"] = departmentEmployees;
        ViewData["DepartmentQuota"] = departmentQuota;
        ViewData["ExistingNominations"] = existingNominations;
        ViewData["CanNominate"] = departmentQuota == null || existingNominations < departmentQuota.MaxNominations;
        
        return View();
    }

    // POST: Nominations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Manager,EOM-Admin")]
    public async Task<IActionResult> Create([Bind("CycleId,EmployeeId,SupportingDocPath")] Nomination nomination)
    {
        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var currentEmployee = await _context.Employees.FindAsync(currentEmployeeId);
        
        if (currentEmployee == null)
        {
            return NotFound();
        }

        // Set the manager ID to current user
        nomination.ManagerId = currentEmployeeId;
        
        // Check if employee is in same department
        var selectedEmployee = await _context.Employees.FindAsync(nomination.EmployeeId);
        if (selectedEmployee == null || selectedEmployee.DepartmentId != currentEmployee.DepartmentId)
        {
            ModelState.AddModelError("EmployeeId", "يمكنك فقط ترشيح الموظفين في قسمك");
        }

        // Check department quota
        var departmentQuota = await _context.DepartmentQuotas
            .FirstOrDefaultAsync(dq => dq.DepartmentId == currentEmployee.DepartmentId);
        
        if (departmentQuota != null)
        {
            var existingNominations = await _context.Nominations
                .CountAsync(n => n.ManagerId == currentEmployeeId && n.CycleId == nomination.CycleId);
            
            if (existingNominations >= departmentQuota.MaxNominations)
            {
                ModelState.AddModelError("", $"لقد تجاوزت الحد الأقصى للترشيحات ({departmentQuota.MaxNominations})");
            }
        }

        if (ModelState.IsValid)
        {
            nomination.CreatedAt = DateTime.UtcNow;
            _context.Add(nomination);
            await _context.SaveChangesAsync();
            
            // Redirect to scoring page
            return RedirectToAction("Score", new { id = nomination.NominationId });
        }
        
        // Reload view data for error case
        var activeCycles = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Where(ac => ac.Status == CycleStatus.Nomination)
            .ToListAsync();

        var departmentEmployees = await _context.Employees
            .Where(e => e.DepartmentId == currentEmployee.DepartmentId 
                       && e.EmployeeId != currentEmployeeId 
                       && e.IsActive)
            .ToListAsync();

        ViewData["CycleId"] = new SelectList(activeCycles, "CycleId", "AwardType.Name", nomination.CycleId);
        ViewData["DepartmentEmployees"] = departmentEmployees;
        ViewData["DepartmentQuota"] = departmentQuota;
        
        return View(nomination);
    }

    // GET: Nominations/Score/5
    public async Task<IActionResult> Score(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var nomination = await _context.Nominations
            .Include(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .ThenInclude(at => at.Criteria)
            .ThenInclude(c => c.SubCriteria)
            .Include(n => n.ManagerScores)
            .FirstOrDefaultAsync(n => n.NominationId == id);

        if (nomination == null)
        {
            return NotFound();
        }

        // Initialize manager scores if they don't exist
        foreach (var criterion in nomination.AwardCycle.AwardType.Criteria)
        {
            foreach (var subCriteria in criterion.SubCriteria)
            {
                if (!nomination.ManagerScores.Any(ms => ms.SubCriteriaId == subCriteria.SubCriteriaId))
                {
                    nomination.ManagerScores.Add(new ManagerScore
                    {
                        NominationId = nomination.NominationId,
                        SubCriteriaId = subCriteria.SubCriteriaId,
                        Score = 0,
                        Note = string.Empty
                    });
                }
            }
        }

        return View(nomination);
    }

    // POST: Nominations/Score/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Score(int id, List<ManagerScore> managerScores)
    {
        var nomination = await _context.Nominations
            .Include(n => n.ManagerScores)
            .FirstOrDefaultAsync(n => n.NominationId == id);

        if (nomination == null)
        {
            return NotFound();
        }

        // Update or add manager scores
        foreach (var score in managerScores)
        {
            var existingScore = nomination.ManagerScores
                .FirstOrDefault(ms => ms.SubCriteriaId == score.SubCriteriaId);

            if (existingScore != null)
            {
                existingScore.Score = score.Score;
                existingScore.Note = score.Note;
            }
            else
            {
                score.NominationId = nomination.NominationId;
                _context.ManagerScores.Add(score);
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Nominations/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var nomination = await _context.Nominations
            .Include(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .FirstOrDefaultAsync(m => m.NominationId == id);
        
        if (nomination == null)
        {
            return NotFound();
        }

        return View(nomination);
    }

    // POST: Nominations/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var nomination = await _context.Nominations.FindAsync(id);
        if (nomination != null)
        {
            _context.Nominations.Remove(nomination);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool NominationExists(int id)
    {
        return _context.Nominations.Any(e => e.NominationId == id);
    }
}