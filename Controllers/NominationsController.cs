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
    private readonly IWebHostEnvironment _webHostEnvironment;

    public NominationsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Nominations - Redirect to Home page since it has better cycle-based view
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Home");
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
    public async Task<IActionResult> Create(int? cycleId)
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

        // If more than one active cycle, let manager choose; otherwise preselect
        if (!cycleId.HasValue && activeCycles.Count == 1)
        {
            cycleId = activeCycles.First().CycleId;
        }

        // Validate provided cycleId
        if (cycleId.HasValue && !activeCycles.Any(c => c.CycleId == cycleId.Value))
        {
            return RedirectToAction(nameof(Index));
        }

        int selectedCycleId = cycleId ?? activeCycles.First().CycleId;

        // Get department quota for the manager
        var departmentQuota = await _context.DepartmentQuotas
            .Include(dq => dq.AwardType)
            .FirstOrDefaultAsync(dq => dq.DepartmentId == currentEmployee.DepartmentId);

        // Get already nominated employees in this cycle
        var nominatedEmployeeIds = await _context.Nominations
            .Where(n => n.CycleId == selectedCycleId)
            .Select(n => n.EmployeeId)
            .ToListAsync();

        // Get employees in the same department (managed by this manager) excluding already nominated ones
        var departmentEmployees = await _context.Employees
            .Where(e => e.DepartmentId == currentEmployee.DepartmentId 
                       && e.EmployeeId != currentEmployeeId 
                       && e.IsActive
                       && !nominatedEmployeeIds.Contains(e.EmployeeId))
            .ToListAsync();

        // Count nominations for that specific cycle only
        var existingNominations = await _context.Nominations
            .CountAsync(n => n.ManagerId == currentEmployeeId && n.CycleId == selectedCycleId);

        ViewData["CycleId"] = new SelectList(activeCycles, "CycleId", "AwardType.Name", cycleId);

        bool hideCycleSelect = activeCycles.Count == 1 || cycleId.HasValue;
        ViewBag.HideCycleSelect = hideCycleSelect;
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
            ModelState.AddModelError("EmployeeId", "يمكنك فقط ترشيح الموظفين في دائرتك");
        }

        // Check if employee is already nominated in this cycle
        var existingNomination = await _context.Nominations
            .FirstOrDefaultAsync(n => n.EmployeeId == nomination.EmployeeId && n.CycleId == nomination.CycleId);
        if (existingNomination != null)
        {
            ModelState.AddModelError("EmployeeId", "هذا الموظف مرشح بالفعل في هذه الدورة");
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
            
            TempData["SuccessMessage"] = $"تم ترشيح {selectedEmployee?.FirstName} {selectedEmployee?.LastName} بنجاح";
            return RedirectToAction("Score", new { id = nomination.NominationId });
        }
        
        // If we reach here, ModelState is invalid. Log the errors.
        var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { x.Key, x.Value.Errors });

        System.Diagnostics.Debug.WriteLine("===== ModelState Errors on POST Nominations/Create =====");
        foreach (var error in errors)
        {
            foreach (var subError in error.Errors)
            {
                System.Diagnostics.Debug.WriteLine($"Key: {error.Key}, Error: {subError.ErrorMessage}");
            }
        }
        System.Diagnostics.Debug.WriteLine("======================================================");

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
            .Include(n => n.Employee)
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
                        Score = null,
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
    public async Task<IActionResult> Score(int id, List<ManagerScore> managerScores, IFormFile? supportingDoc)
    {
        var nomination = await _context.Nominations
            .Include(n => n.Employee)
            .Include(n => n.AwardCycle).ThenInclude(ac => ac.AwardType).ThenInclude(at => at.Criteria).ThenInclude(c => c.SubCriteria)
            .Include(n => n.ManagerScores)
            .FirstOrDefaultAsync(n => n.NominationId == id);

        if (nomination == null)
        {
            return NotFound();
        }

        // Handle file upload (optional)
        if (supportingDoc != null && supportingDoc.Length > 0)
        {
            // Validate file type
            if (Path.GetExtension(supportingDoc.FileName).ToLower() != ".pdf")
            {
                ModelState.AddModelError("supportingDoc", "الرجاء رفع ملف بصيغة PDF فقط.");
            }
            else
            {
                try
                {
                    // Clean up old file if it exists
                    if (!string.IsNullOrEmpty(nomination.SupportingDocPath))
                    {
                        var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, nomination.SupportingDocPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Generate unique filename and create directory
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(supportingDoc.FileName)}";
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "nominations");
                    Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await supportingDoc.CopyToAsync(fileStream);
                    }
                    
                    // Update nomination with new file path
                    nomination.SupportingDocPath = $"/uploads/nominations/{fileName}";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("supportingDoc", $"خطأ في رفع الملف: {ex.Message}");
                }
            }
        }
        // Note: If no file is uploaded, SupportingDocPath remains unchanged

        var subCriterias = nomination.AwardCycle.AwardType.Criteria.SelectMany(c => c.SubCriteria).ToDictionary(sc => sc.SubCriteriaId);

        // Manual validation of scores
        for (int i = 0; i < managerScores.Count; i++)
        {
            var score = managerScores[i];
            if (!score.Score.HasValue)
            {
                ModelState.AddModelError($"managerScores[{i}].Score", "يجب إدخال درجة لكل المعايير.");
                continue;
            }

            if (subCriterias.TryGetValue(score.SubCriteriaId, out var subCriteria))
            {
                if (score.Score.Value < 0 || score.Score.Value > subCriteria.MaxScore)
                {
                    ModelState.AddModelError($"managerScores[{i}].Score", $"الدرجة لمعيار '{subCriteria.Name}' يجب أن تكون بين 0 و {subCriteria.MaxScore}.");
                }
            }
        }

        if (!ModelState.IsValid)
        {
            // If model is invalid, re-populate the scores from the form submission to show the invalid values back to the user.
            foreach (var submittedScore in managerScores)
            {
                var modelScore = nomination.ManagerScores.FirstOrDefault(ms => ms.SubCriteriaId == submittedScore.SubCriteriaId);
                if (modelScore != null)
                {
                    modelScore.Score = submittedScore.Score;
                    modelScore.Note = submittedScore.Note;
                }
            }
            return View(nomination);
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
        
        // Redirect to cycle details page
        var cycleId = nomination.CycleId;
        TempData["SuccessMessage"] = $"تم حفظ تقييم {nomination.Employee?.FirstName} {nomination.Employee?.LastName} بنجاح";
        return RedirectToAction("CycleDetails", new { id = cycleId });
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
        var nomination = await _context.Nominations
            .Include(n => n.AwardCycle)
            .Include(n => n.Employee)
            .FirstOrDefaultAsync(n => n.NominationId == id);
            
        if (nomination != null)
        {
            var cycleId = nomination.CycleId;
            var employeeName = $"{nomination.Employee?.FirstName} {nomination.Employee?.LastName}";
            
            // Clean up supporting document file if it exists
            if (!string.IsNullOrEmpty(nomination.SupportingDocPath))
            {
                try
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, nomination.SupportingDocPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue with deletion
                    System.Diagnostics.Debug.WriteLine($"Error deleting supporting document: {ex.Message}");
                }
            }
            
            _context.Nominations.Remove(nomination);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"تم حذف ترشيح {employeeName} بنجاح";
            return RedirectToAction("CycleDetails", new { id = cycleId });
        }

        TempData["ErrorMessage"] = "حدث خطأ أثناء حذف الترشيح";
        return RedirectToAction("Index", "Home");
    }

    // POST: Nominations/SelectWinner/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "EOM-Committee")]
    public async Task<IActionResult> SelectWinner(int id)
    {
        var nomination = await _context.Nominations
            .Include(n => n.Employee)
            .Include(n => n.AwardCycle)
            .FirstOrDefaultAsync(n => n.NominationId == id);

        if (nomination == null)
        {
            TempData["ErrorMessage"] = "الترشيح غير موجود";
            return RedirectToAction("Index", "Home");
        }

        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        nomination.IsWinner = true;
        nomination.WonAt = DateTime.UtcNow;
        nomination.SelectedByCommitteeMemberId = currentEmployeeId;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"تم اختيار {nomination.Employee?.FirstName} {nomination.Employee?.LastName} كفائز بنجاح";
        return RedirectToAction("CycleDetails", new { id = nomination.CycleId });
    }

    // POST: Nominations/RemoveWinner/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "EOM-Committee")]
    public async Task<IActionResult> RemoveWinner(int id)
    {
        var nomination = await _context.Nominations
            .Include(n => n.Employee)
            .Include(n => n.AwardCycle)
            .FirstOrDefaultAsync(n => n.NominationId == id);

        if (nomination == null)
        {
            TempData["ErrorMessage"] = "الترشيح غير موجود";
            return RedirectToAction("Index", "Home");
        }

        nomination.IsWinner = false;
        nomination.WonAt = null;
        nomination.SelectedByCommitteeMemberId = null;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"تم إلغاء فوز {nomination.Employee?.FirstName} {nomination.Employee?.LastName} بنجاح";
        return RedirectToAction("CycleDetails", new { id = nomination.CycleId });
    }

    // GET: Nominations/Cycles
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Cycles()
    {
        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Get all cycles with manager's nomination counts
        var cycles = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Include(ac => ac.Nominations.Where(n => n.ManagerId == currentEmployeeId))
            .ThenInclude(n => n.Employee)
            .Include(ac => ac.Nominations.Where(n => n.ManagerId == currentEmployeeId))
            .ThenInclude(n => n.Evaluations)
            .OrderByDescending(ac => ac.Year)
            .ThenByDescending(ac => ac.Month)
            .ToListAsync();
        
        return View(cycles);
    }
    
    // GET: Nominations/CycleDetails/5
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> CycleDetails(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var cycle = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Include(ac => ac.Nominations.Where(n => n.ManagerId == currentEmployeeId))
            .ThenInclude(n => n.Employee)
            .Include(ac => ac.Nominations.Where(n => n.ManagerId == currentEmployeeId))
            .ThenInclude(n => n.ManagerScores)
            .ThenInclude(ms => ms.SubCriteria)
            .Include(ac => ac.Nominations.Where(n => n.ManagerId == currentEmployeeId))
            .ThenInclude(n => n.Evaluations)
            .FirstOrDefaultAsync(ac => ac.CycleId == id);
        
        if (cycle == null)
        {
            return NotFound();
        }
        
        return View(cycle);
    }

    private bool NominationExists(int id)
    {
        return _context.Nominations.Any(e => e.NominationId == id);
    }
}