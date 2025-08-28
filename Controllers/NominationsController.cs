using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;
using EOM.Web.Services;
using System.Security.Claims;

namespace EOM.Web.Controllers;

[Authorize]
public class NominationsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IEjadahEligibilityService _ejadahService;

    public NominationsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IEjadahEligibilityService ejadahService)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _ejadahService = ejadahService;
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
            .ThenInclude(at => at.Criteria)
            .ThenInclude(c => c.SubCriteria)
            .Include(n => n.ManagerScores)
            .ThenInclude(ms => ms.SubCriteria)
            .Include(n => n.Evaluations)
                .ThenInclude(e => e.EvaluationScores)
                    .ThenInclude(es => es.SubCriteria)
            .Include(n => n.Evaluations)
                .ThenInclude(e => e.CommitteeMember)
                    .ThenInclude(cm => cm.Employee)
            .Include(n => n.Employee)
            .Include(n => n.Manager)
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

        // Get active nomination cycles (exclude self-nomination award types)
        var activeCycles = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Where(ac => ac.Status == CycleStatus.Nomination && !ac.AwardType.IsSelfNomination)
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

        // Fetch quota for the specific award type of the selected cycle (avoid picking quota rows with MaxNominations = 0)
        var selectedCycle = activeCycles.First(c => c.CycleId == selectedCycleId);

        var departmentQuota = await _context.DepartmentQuotas
            .Include(dq => dq.AwardType)
            .FirstOrDefaultAsync(dq => dq.DepartmentId == currentEmployee.DepartmentId && dq.AwardTypeId == selectedCycle.AwardTypeId);

        // Check if department quota exists - if not, don't allow nominations
        if (departmentQuota == null)
        {
            ViewBag.Message = $"لا توجد حصة ترشيح مُعرّفة لدائرتك في نوع الجائزة '{selectedCycle.AwardType.Name}'. يرجى مراجعة اللجنة.";
            ViewData["CycleId"] = new SelectList(activeCycles, "CycleId", "AwardType.Name", cycleId);
            ViewBag.HideCycleSelect = activeCycles.Count == 1 || cycleId.HasValue;
            ViewData["DepartmentEmployees"] = new List<Employee>(); // Empty list
            ViewData["DepartmentQuota"] = null;
            ViewData["ExistingNominations"] = 0;
            ViewData["CanNominate"] = false;
            return View();
        }

        // Get already nominated employees in this cycle
        var nominatedEmployeeIds = await _context.Nominations
            .Where(n => n.CycleId == selectedCycleId)
            .Select(n => n.EmployeeId)
            .ToListAsync();

        // Fetch employees from the same department (including direct reports and department colleagues)
        var allDepartmentEmployees = await _context.Employees
            .Where(e => (e.ManagerId == currentEmployee.EmployeeId || e.DepartmentId == currentEmployee.DepartmentId)
                       && e.IsActive == 1
                       && e.EmployeeId != currentEmployee.EmployeeId // Exclude the manager themselves
                       && !nominatedEmployeeIds.Contains(e.EmployeeId))
            .GroupBy(e => e.EmployeeId)
            .Select(g => g.First())
            .ToListAsync();

        // Get Ejadah eligibility and score information for all employees
        var employeeIds = allDepartmentEmployees.Select(e => e.EmployeeId).ToList();
        var eligibilityResults = await _ejadahService.CheckMultipleEmployeeEligibilityAsync(employeeIds);
        var ineligibilityReasons = await _ejadahService.GetIneligibleEmployeesAsync(employeeIds);
        
        // Get latest Ejadah scores for all employees
        var ejadahScores = new Dictionary<int, dynamic>();
        foreach (var employeeId in employeeIds)
        {
            var latestScore = await _ejadahService.GetLatestEjadahScoreAsync(employeeId);
            if (latestScore != null)
            {
                ejadahScores[employeeId] = new {
                    Score = latestScore.Score,
                    ScoreArabic = latestScore.ScoreArabic,
                    CycleName = $"{latestScore.EjadahCycle?.Year} - النصف {(latestScore.EjadahCycle?.Half == 1 ? "الأول" : "الثاني")}",
                    IsEligible = latestScore.IsEligibleForNomination
                };
            }
        }

        // Show all employees with their eligibility status
        var departmentEmployees = allDepartmentEmployees;
        var ineligibleEmployees = new Dictionary<int, string>();
        var ineligibleEmployeeDetails = new List<dynamic>();

        foreach (var employee in allDepartmentEmployees)
        {
            if (!eligibilityResults.GetValueOrDefault(employee.EmployeeId, true) && ineligibilityReasons.ContainsKey(employee.EmployeeId))
            {
                ineligibleEmployees[employee.EmployeeId] = ineligibilityReasons[employee.EmployeeId];
                ineligibleEmployeeDetails.Add(new {
                    EmployeeId = employee.EmployeeId,
                    Name = $"{employee.FirstName} {employee.LastName}",
                    Reason = ineligibilityReasons[employee.EmployeeId]
                });
            }
        }

        // Count nominations for that specific cycle only
        var existingNominations = await _context.Nominations
            .CountAsync(n => n.ManagerId == currentEmployeeId && n.CycleId == selectedCycleId);

        ViewData["CycleId"] = new SelectList(activeCycles, "CycleId", "AwardType.Name", cycleId);
        ViewBag.SelectedCycleId = selectedCycleId;

        bool hideCycleSelect = activeCycles.Count == 1 || cycleId.HasValue;
        ViewBag.HideCycleSelect = hideCycleSelect;
        ViewData["DepartmentEmployees"] = departmentEmployees;
        ViewData["IneligibleEmployees"] = ineligibleEmployees;
        ViewData["IneligibleEmployeeDetails"] = ineligibleEmployeeDetails;
        ViewData["EjadahScores"] = ejadahScores;
        ViewData["EligibilityResults"] = eligibilityResults;
        ViewData["TotalEmployeesCount"] = allDepartmentEmployees.Count;
        ViewData["EligibleEmployeesCount"] = eligibilityResults.Count(kvp => kvp.Value);
        ViewData["IneligibleEmployeesCount"] = ineligibleEmployees.Count;
        
        if (departmentQuota != null && departmentQuota.MaxNominations <= 0)
        {
            departmentQuota.MaxNominations = 2; // Default fallback
        }

        ViewData["DepartmentQuota"] = departmentQuota;
        ViewData["ExistingNominations"] = existingNominations;
        ViewData["CanNominate"] = departmentQuota == null || existingNominations < departmentQuota.MaxNominations;
        
        return View();
    }

    // POST: Nominations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Manager,EOM-Admin")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB
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
        
        // Check if employee reports to this manager or is in the same department
        var selectedEmployee = await _context.Employees.FindAsync(nomination.EmployeeId);
        if (selectedEmployee == null || 
            (selectedEmployee.ManagerId != currentEmployee.EmployeeId && selectedEmployee.DepartmentId != currentEmployee.DepartmentId))
        {
            ModelState.AddModelError("EmployeeId", "يمكنك فقط ترشيح الموظفين التابعين لك أو من نفس القسم");
        }

        // Check Ejadah eligibility
        if (selectedEmployee != null)
        {
            var isEligible = await _ejadahService.CanEmployeeBeNominatedAsync(selectedEmployee.EmployeeId);
            if (!isEligible)
            {
                var latestScore = await _ejadahService.GetLatestEjadahScoreAsync(selectedEmployee.EmployeeId);
                var scoreText = latestScore?.ScoreArabic ?? "غير محدد";
                var cycleText = latestScore?.EjadahCycle != null 
                    ? $"{latestScore.EjadahCycle.Year} - النصف {(latestScore.EjadahCycle.Half == 1 ? "الأول" : "الثاني")}"
                    : "";
                ModelState.AddModelError("EmployeeId", $"لا يمكن ترشيح هذا الموظف بسبب تقييم أجادة {scoreText} في دورة {cycleText}");
            }
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
            .FirstOrDefaultAsync(dq => dq.DepartmentId == currentEmployee.DepartmentId && dq.AwardTypeId == (_context.AwardCycles.Where(c=>c.CycleId==nomination.CycleId).Select(c=>c.AwardTypeId).FirstOrDefault()));
        
        // Prevent nomination if no department quota exists
        if (departmentQuota == null)
        {
            ModelState.AddModelError("", "لا توجد حصة ترشيح مُعرّفة لقسمك في هذا النوع من الجوائز. يرجى مراجعة الإدارة.");
        }
        else if (departmentQuota.MaxNominations > 0)
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
            // Instead of creating nomination immediately, redirect to scoring page
            TempData["NominationData"] = System.Text.Json.JsonSerializer.Serialize(new {
                CycleId = nomination.CycleId,
                EmployeeId = nomination.EmployeeId,
                ManagerId = nomination.ManagerId
            });
            
            return RedirectToAction("Score", new { 
                cycleId = nomination.CycleId, 
                employeeId = nomination.EmployeeId 
            });
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

        // Reload view data for error case (exclude self-nomination award types)
        var activeCycles = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Where(ac => ac.Status == CycleStatus.Nomination && !ac.AwardType.IsSelfNomination)
            .ToListAsync();

        var departmentEmployees = await _context.Employees
            .Where(e => (e.ManagerId == currentEmployee.EmployeeId || e.DepartmentId == currentEmployee.DepartmentId) 
                       && e.IsActive == 1 
                       && e.EmployeeId != currentEmployee.EmployeeId)
            .ToListAsync();

        ViewData["CycleId"] = new SelectList(activeCycles, "CycleId", "AwardType.Name", nomination.CycleId);
        ViewData["DepartmentEmployees"] = departmentEmployees;
        ViewData["DepartmentQuota"] = departmentQuota;
        
        return View(nomination);
    }


    // GET: Nominations/Score/5 or Score?cycleId=x&employeeId=y
    [Authorize(Roles = "Manager,EOM-Admin")]
    public async Task<IActionResult> Score(int? id, int? cycleId, int? employeeId)
    {
        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Handle new nomination case (cycleId and employeeId provided)
        if (id == null && cycleId.HasValue && employeeId.HasValue)
        {
            return await HandleNewNomination(cycleId.Value, employeeId.Value, currentEmployeeId);
        }
        
        // Handle existing nomination case (id provided)
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

        // Disallow editing if cycle is closed or published
        if (nomination.AwardCycle.Status == CycleStatus.Closed || nomination.AwardCycle.Status == CycleStatus.Published)
        {
            TempData["Error"] = "Cannot edit manager score after the cycle is closed.";
            return RedirectToAction("CycleDetails", new { id = nomination.CycleId });
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

    private async Task<IActionResult> HandleNewNomination(int cycleId, int employeeId, int currentEmployeeId)
    {
        var currentEmployee = await _context.Employees.FindAsync(currentEmployeeId);
        var selectedEmployee = await _context.Employees.FindAsync(employeeId);
        
        if (currentEmployee == null || selectedEmployee == null)
        {
            return NotFound();
        }

        // Verify the employee can be nominated by this manager
        if (selectedEmployee.ManagerId != currentEmployee.EmployeeId && selectedEmployee.DepartmentId != currentEmployee.DepartmentId)
        {
            TempData["ErrorMessage"] = "يمكنك فقط ترشيح الموظفين التابعين لك أو من نفس القسم";
            return RedirectToAction("Create", new { cycleId });
        }

        // Get the award cycle with criteria
        var cycle = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .ThenInclude(at => at.Criteria)
            .ThenInclude(c => c.SubCriteria)
            .FirstOrDefaultAsync(ac => ac.CycleId == cycleId);

        if (cycle == null)
        {
            return NotFound();
        }

        // Check if cycle is still in nomination phase
        if (cycle.Status != CycleStatus.Nomination)
        {
            TempData["ErrorMessage"] = "لا يمكن الترشيح في هذه الدورة حالياً";
            return RedirectToAction("Create", new { cycleId });
        }

        // Create a temporary nomination object for scoring (not saved to database yet)
        var tempNomination = new Nomination
        {
            CycleId = cycleId,
            EmployeeId = employeeId,
            ManagerId = currentEmployeeId,
            Employee = selectedEmployee,
            Manager = currentEmployee,
            AwardCycle = cycle,
            ManagerScores = new List<ManagerScore>()
        };

        // Initialize empty manager scores for all sub-criteria
        foreach (var criterion in cycle.AwardType.Criteria)
        {
            foreach (var subCriteria in criterion.SubCriteria)
            {
                tempNomination.ManagerScores.Add(new ManagerScore
                {
                    SubCriteriaId = subCriteria.SubCriteriaId,
                    SubCriteria = subCriteria,
                    Score = null,
                    Note = string.Empty
                });
            }
        }

        ViewData["IsNewNomination"] = true;
        ViewData["CycleId"] = cycleId;
        ViewData["EmployeeId"] = employeeId;
        return View(tempNomination);
    }

    private async Task<IActionResult> HandleNewNominationSubmission(int cycleId, int employeeId, int currentEmployeeId, List<ManagerScore> managerScores, IFormFile? supportingDoc)
    {
        var currentEmployee = await _context.Employees.FindAsync(currentEmployeeId);
        var selectedEmployee = await _context.Employees.FindAsync(employeeId);
        
        if (currentEmployee == null || selectedEmployee == null)
        {
            return NotFound();
        }

        // Get the award cycle with criteria
        var cycle = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .ThenInclude(at => at.Criteria)
            .ThenInclude(c => c.SubCriteria)
            .FirstOrDefaultAsync(ac => ac.CycleId == cycleId);

        if (cycle == null)
        {
            return NotFound();
        }

        // Re-validate all conditions before creating nomination
        // Check Ejadah eligibility
        var isEligible = await _ejadahService.CanEmployeeBeNominatedAsync(selectedEmployee.EmployeeId);
        if (!isEligible)
        {
            var latestScore = await _ejadahService.GetLatestEjadahScoreAsync(selectedEmployee.EmployeeId);
            var scoreText = latestScore?.ScoreArabic ?? "غير محدد";
            var cycleText = latestScore?.EjadahCycle != null 
                ? $"{latestScore.EjadahCycle.Year} - النصف {(latestScore.EjadahCycle.Half == 1 ? "الأول" : "الثاني")}"
                : "";
            ModelState.AddModelError("", $"لا يمكن ترشيح هذا الموظف بسبب تقييم أجادة {scoreText} في دورة {cycleText}");
        }

        // Check if employee is already nominated in this cycle
        var existingNomination = await _context.Nominations
            .FirstOrDefaultAsync(n => n.EmployeeId == employeeId && n.CycleId == cycleId);
        if (existingNomination != null)
        {
            ModelState.AddModelError("", "هذا الموظف مرشح بالفعل في هذه الدورة");
        }

        // Validate scores
        var subCriterias = cycle.AwardType.Criteria.SelectMany(c => c.SubCriteria).ToDictionary(sc => sc.SubCriteriaId);
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
            // Reload the view with errors - return to GET Score with same parameters
            return await HandleNewNomination(cycleId, employeeId, currentEmployeeId);
        }

        // Handle file upload
        string? supportingDocPath = null;
        if (supportingDoc != null && supportingDoc.Length > 0)
        {
            if (Path.GetExtension(supportingDoc.FileName).ToLower() != ".pdf")
            {
                ModelState.AddModelError("supportingDoc", "الرجاء رفع ملف بصيغة PDF فقط.");
                return await HandleNewNomination(cycleId, employeeId, currentEmployeeId);
            }
            else
            {
                try
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(supportingDoc.FileName)}";
                    var uploadsFolder = @"C:\EOM\uploads";
                    Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await supportingDoc.CopyToAsync(fileStream);
                    }
                    
                    supportingDocPath = fileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("supportingDoc", $"خطأ في رفع الملف: {ex.Message}");
                    return await HandleNewNomination(cycleId, employeeId, currentEmployeeId);
                }
            }
        }

        // Create the nomination record
        var nomination = new Nomination
        {
            CycleId = cycleId,
            EmployeeId = employeeId,
            ManagerId = currentEmployeeId,
            SupportingDocPath = supportingDocPath,
            CreatedAt = DateTime.UtcNow
        };

        _context.Add(nomination);
        await _context.SaveChangesAsync();

        // Add manager scores
        foreach (var score in managerScores)
        {
            if (score.Score.HasValue)
            {
                _context.ManagerScores.Add(new ManagerScore
                {
                    NominationId = nomination.NominationId,
                    SubCriteriaId = score.SubCriteriaId,
                    Score = score.Score,
                    Note = score.Note ?? string.Empty
                });
            }
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"تم ترشيح وتقييم {selectedEmployee.FirstName} {selectedEmployee.LastName} بنجاح";
        return RedirectToAction("CycleDetails", new { id = cycleId });
    }

    // POST: Nominations/Score/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Manager,EOM-Admin")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB
    public async Task<IActionResult> Score(int? id, int? cycleId, int? employeeId, List<ManagerScore> managerScores, IFormFile? supportingDoc)
    {
        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Handle new nomination case (create nomination with scores)
        if (id == null && cycleId.HasValue && employeeId.HasValue)
        {
            return await HandleNewNominationSubmission(cycleId.Value, employeeId.Value, currentEmployeeId, managerScores, supportingDoc);
        }
        
        // Handle existing nomination case
        if (!id.HasValue)
        {
            return NotFound();
        }
        
        var nomination = await _context.Nominations
            .Include(n => n.Employee)
            .Include(n => n.AwardCycle).ThenInclude(ac => ac.AwardType).ThenInclude(at => at.Criteria).ThenInclude(c => c.SubCriteria)
            .Include(n => n.ManagerScores)
            .FirstOrDefaultAsync(n => n.NominationId == id.Value);

        if (nomination == null)
        {
            return NotFound();
        }

        if (nomination.AwardCycle.Status == CycleStatus.Closed || nomination.AwardCycle.Status == CycleStatus.Published)
        {
            TempData["Error"] = "Cannot edit manager score after the cycle is closed.";
            return RedirectToAction("CycleDetails", new { id = nomination.CycleId });
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
                        // Extract filename from the stored path
                        var oldFileName = Path.GetFileName(nomination.SupportingDocPath);
                        var oldFilePath = Path.Combine(@"C:\EOM\uploads", oldFileName);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Generate unique filename and create directory
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(supportingDoc.FileName)}";
                    var uploadsFolder = @"C:\EOM\uploads";
                    Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await supportingDoc.CopyToAsync(fileStream);
                    }
                    
                    // Update nomination with new file path (store just the filename)
                    nomination.SupportingDocPath = fileName;
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
        TempData["SuccessMessage"] = $"تم حفظ تقييم {nomination.Employee?.FirstName} {nomination.Employee?.LastName} بنجاح";
        return RedirectToAction("CycleDetails", new { id = nomination.CycleId });
    }

    // GET: Nominations/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var nomination = await _context.Nominations
            .Include(n => n.Employee)
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
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Nominations/SelectWinner/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "EOM-Committee,EOM-Committee-Lead")]
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
        
        nomination.IsWinner = 1;
        nomination.WonAt = DateTime.UtcNow;
        nomination.SelectedByCommitteeMemberId = currentEmployeeId;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"تم اختيار {nomination.Employee?.FirstName} {nomination.Employee?.LastName} كفائز بنجاح";
        return RedirectToAction("CycleDetails", new { id = nomination.CycleId });
    }

    // POST: Nominations/RemoveWinner/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "EOM-Committee,EOM-Committee-Lead")]
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

        nomination.IsWinner = 0;
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
        
        // Get all cycles with manager's nomination counts (exclude self-nomination award types)
        var cycles = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Include(ac => ac.Nominations.Where(n => n.ManagerId == currentEmployeeId))
            .ThenInclude(n => n.Employee)
            .Include(ac => ac.Nominations.Where(n => n.ManagerId == currentEmployeeId))
            .ThenInclude(n => n.Evaluations)
            .Where(ac => !ac.AwardType.IsSelfNomination)
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
            .ThenInclude(e => e.Department)
            .Include(ac => ac.Nominations.Where(n => n.ManagerId == currentEmployeeId))
            .ThenInclude(n => n.ManagerScores)
            .ThenInclude(ms => ms.SubCriteria)
            .Include(ac => ac.Nominations.Where(n => n.ManagerId == currentEmployeeId))
            .ThenInclude(n => n.Evaluations)
            .Where(ac => !ac.AwardType.IsSelfNomination)
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

    // GET: Nominations/DownloadAttachment/{id}
    public async Task<IActionResult> DownloadAttachment(int id)
    {
        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Get the nomination
        var nomination = await _context.Nominations
            .Include(n => n.Employee)
            .Include(n => n.Manager)
            .FirstOrDefaultAsync(n => n.NominationId == id);

        if (nomination == null || string.IsNullOrEmpty(nomination.AttachmentPath))
        {
            return NotFound();
        }

        // Check access: allow nominee, manager, committee member, or admin
        bool hasAccess = false;
        
        // Check if current user is the nominee
        if (nomination.EmployeeId == currentEmployeeId)
        {
            hasAccess = true;
        }
        // Check if current user is the manager who nominated
        else if (nomination.ManagerId == currentEmployeeId)
        {
            hasAccess = true;
        }
        else if (User.IsInRole("EOM-Admin"))
        {
            // Admin always has access
            hasAccess = true;
        }
        else if (User.IsInRole("Manager"))
        {
            // Any manager can view attachments
            hasAccess = true;
        }
        else
        {
            // Check if current user is a committee member
            var isCommitteeMember = await _context.CommitteeMembers
                .AnyAsync(cm => cm.EmployeeId == currentEmployeeId && cm.IsActive == true);
            
            if (isCommitteeMember)
            {
                hasAccess = true;
            }
        }

        if (!hasAccess)
        {
            return Forbid();
        }

        var filePath = Path.Combine(@"C:\EOM\uploads", nomination.AttachmentPath);
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("الملف غير موجود");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        var fileName = $"{nomination.Title ?? "Nomination"}_attachment.pdf";
        
        return File(fileBytes, "application/pdf", fileName);
    }
}