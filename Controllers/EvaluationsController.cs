using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;

namespace EOM.Web.Controllers;

[Authorize(Roles = "EOM-Committee,EOM-Committee-Lead")]
public class EvaluationsController : BaseController
{
    private readonly ApplicationDbContext _context;

    public EvaluationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Evaluations
    public async Task<IActionResult> Index()
    {
        // TODO: Get current employee from authentication
        var currentEmployeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Get current employee's committee member record
        var committeeMember = await _context.CommitteeMembers
            .Where(cm => cm.EmployeeId == currentEmployeeId && cm.IsActive)
            .FirstOrDefaultAsync();
            
        if (committeeMember == null)
        {
            return Forbid("غير مخول للتقييم");
        }

        // Get evaluations from the current active cycle (status = Evaluating)
        var evaluations = await _context.Evaluations
            .AsNoTracking()
            .Where(e => e.CommitteeMemberId == committeeMember.Id && e.Nomination.AwardCycle.Status == CycleStatus.Evaluating)
            .Include(e => e.Nomination)
                .ThenInclude(n => n.Employee)
                    .ThenInclude(e => e.Department)
            .Include(e => e.Nomination)
                .ThenInclude(n => n.AwardCycle)
                    .ThenInclude(ac => ac.AwardType)
                        .ThenInclude(at => at.Criteria)
                            .ThenInclude(c => c.SubCriteria)
            .Include(e => e.EvaluationScores)
                .ThenInclude(es => es.SubCriteria)
            .ToListAsync();
        
        // Remove any duplicates based on EvaluationId (in case EF is creating duplicates)
        evaluations = evaluations
            .GroupBy(e => e.EvaluationId)
            .Select(g => g.First())
            .OrderBy(e => e.Nomination?.Employee?.DepartmentId)
            .ThenBy(e => e.Nomination?.Employee?.LastName)
            .ToList();

        return View(evaluations);
    }

    // GET: Evaluations/Pending
    public async Task<IActionResult> Pending()
    {
        // TODO: Get current employee from authentication
        var currentEmployeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Get current employee's committee member record
        var committeeMember = await _context.CommitteeMembers
            .Where(cm => cm.EmployeeId == currentEmployeeId && cm.IsActive)
            .FirstOrDefaultAsync();
            
        if (committeeMember == null)
        {
            return Forbid("غير مخول للتقييم");
        }

        // Get nominations that need evaluation by this committee member
        // First get the data without tracking to avoid EF issues
        var pendingNominations = await _context.Nominations
            .AsNoTracking()
            .Include(n => n.AwardCycle)
                .ThenInclude(ac => ac.AwardType)
            .Include(n => n.Employee)
                .ThenInclude(e => e.Department)
            .Include(n => n.Evaluations)
            .Where(n => n.AwardCycle.Status == CycleStatus.Evaluating)
            .Where(n => !n.Evaluations.Any(e => e.CommitteeMemberId == committeeMember.Id))
            .ToListAsync();
        
        // Remove any duplicates based on NominationId (in case EF is creating duplicates)
        pendingNominations = pendingNominations
            .GroupBy(n => n.NominationId)
            .Select(g => g.First())
            .OrderBy(n => n.Employee?.DepartmentId)
            .ThenBy(n => n.Employee?.LastName)
            .ToList();

        return View(pendingNominations);
    }

    // GET: Evaluations/Create/5 (nomination id)
    public async Task<IActionResult> Create(int nominationId)
    {
        // TODO: Get current employee from authentication
        var currentEmployeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Get current employee's committee member record
        var committeeMember = await _context.CommitteeMembers
            .Where(cm => cm.EmployeeId == currentEmployeeId && cm.IsActive)
            .FirstOrDefaultAsync();
            
        if (committeeMember == null)
        {
            return Forbid("غير مخول للتقييم");
        }

        var nomination = await _context.Nominations
            .Include(n => n.Employee)
            .Include(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .ThenInclude(at => at.Criteria)
            .ThenInclude(c => c.SubCriteria)
            .Include(n => n.ManagerScores)
            .ThenInclude(ms => ms.SubCriteria)
            .Include(n => n.GroupMembers)
            .ThenInclude(gm => gm.Employee)
            .FirstOrDefaultAsync(n => n.NominationId == nominationId);

        if (nomination == null)
        {
            return NotFound();
        }

        // Check if user already evaluated this nomination
        var existingEvaluation = await _context.Evaluations
            .FirstOrDefaultAsync(e => e.NominationId == nominationId && e.CommitteeMemberId == committeeMember.Id);

        if (existingEvaluation != null)
        {
            TempData["Info"] = "لقد قمت بتقييم هذا الترشيح مسبقاً. يمكنك تعديل التقييم الموجود.";
            return RedirectToAction("Edit", new { id = existingEvaluation.EvaluationId });
        }
        
        // Generate a unique submission token to prevent double-submit
        var submissionToken = Guid.NewGuid().ToString();
        HttpContext.Session.SetString($"EvalSubmitToken_{nominationId}", submissionToken);
        ViewData["SubmissionToken"] = submissionToken;

        var evaluation = new Evaluation
        {
            NominationId = nominationId,
            CommitteeMemberId = committeeMember.Id,
            Nomination = nomination
        };

        // Initialize evaluation scores
        foreach (var criterion in nomination.AwardCycle.AwardType.Criteria)
        {
            foreach (var subCriteria in criterion.SubCriteria)
            {
                evaluation.EvaluationScores.Add(new EvaluationScore
                {
                    SubCriteriaId = subCriteria.SubCriteriaId,
                    Note = string.Empty
                });
            }
        }

        return View(evaluation);
    }

    // POST: Evaluations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Evaluation evaluation, string submissionToken)
    {
        // TODO: Get current employee from authentication
        var currentEmployeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Get current employee's committee member record
        var committeeMember = await _context.CommitteeMembers
            .Where(cm => cm.EmployeeId == currentEmployeeId && cm.IsActive)
            .FirstOrDefaultAsync();
            
        if (committeeMember == null)
        {
            return Forbid("غير مخول للتقييم");
        }

        // Check and validate submission token to prevent double-submit
        var sessionTokenKey = $"EvalSubmitToken_{evaluation.NominationId}";
        var sessionToken = HttpContext.Session.GetString(sessionTokenKey);
        
        if (string.IsNullOrEmpty(sessionToken) || sessionToken != submissionToken)
        {
            // Token already used or invalid - likely a double-submit
            TempData["Warning"] = "تم معالجة هذا الطلب بالفعل. يتم توجيهك إلى صفحة التقييمات.";
            return RedirectToAction(nameof(Index));
        }
        
        // Remove the token immediately to prevent reuse
        HttpContext.Session.Remove(sessionTokenKey);

        // Check if this committee member already evaluated this nomination
        var existingEvaluation = await _context.Evaluations
            .FirstOrDefaultAsync(e => e.NominationId == evaluation.NominationId && e.CommitteeMemberId == committeeMember.Id);

        if (existingEvaluation != null)
        {
            // Redirect to edit the existing evaluation instead
            TempData["Warning"] = "لقد قمت بتقييم هذا الترشيح مسبقاً. يمكنك تعديل التقييم الموجود.";
            return RedirectToAction("Edit", new { id = existingEvaluation.EvaluationId });
        }

        evaluation.CommitteeMemberId = committeeMember.Id;
        evaluation.CreatedAt = DateTime.UtcNow;

        if (ModelState.IsValid)
        {
            // Server-side validation for score ranges
            var subCriteriaDict = await _context.SubCriteria
                .Where(sc => evaluation.EvaluationScores.Select(es => es.SubCriteriaId).Contains(sc.SubCriteriaId))
                .ToDictionaryAsync(sc => sc.SubCriteriaId, sc => sc.MaxScore);

            foreach (var score in evaluation.EvaluationScores)
            {
                if (score.Score.HasValue)
                {
                    if (subCriteriaDict.TryGetValue(score.SubCriteriaId, out var maxScore))
                    {
                        if (score.Score.Value < 0 || score.Score.Value > maxScore)
                        {
                            ModelState.AddModelError("", $"Score for SubCriteria {score.SubCriteriaId} must be between 0 and {maxScore}.");
                        }
                    }
                }
            }
            
            if (!ModelState.IsValid)
            {
                // Reload data if validation fails
                evaluation.Nomination = await _context.Nominations
                    .Include(n => n.Employee)
                    .Include(n => n.AwardCycle)
                    .ThenInclude(ac => ac.AwardType)
                    .ThenInclude(at => at.Criteria)
                    .ThenInclude(c => c.SubCriteria)
                    .Include(n => n.ManagerScores)
                    .ThenInclude(ms => ms.SubCriteria)
                    .FirstOrDefaultAsync(n => n.NominationId == evaluation.NominationId) ?? evaluation.Nomination;
                return View(evaluation);
            }

            _context.Add(evaluation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Reload nomination data for view
        evaluation.Nomination = await _context.Nominations
            .Include(n => n.Employee)
            .Include(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .ThenInclude(at => at.Criteria)
            .ThenInclude(c => c.SubCriteria)
            .Include(n => n.ManagerScores)
            .ThenInclude(ms => ms.SubCriteria)
            .FirstOrDefaultAsync(n => n.NominationId == evaluation.NominationId) ?? evaluation.Nomination;

        return View(evaluation);
    }

    // GET: Evaluations/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // TODO: Get current employee from authentication
        var currentEmployeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Get current employee's committee member record
        var committeeMember = await _context.CommitteeMembers
            .Where(cm => cm.EmployeeId == currentEmployeeId && cm.IsActive)
            .FirstOrDefaultAsync();
            
        if (committeeMember == null)
        {
            return Forbid("غير مخول للتقييم");
        }

        var evaluation = await _context.Evaluations
            .Include(e => e.Nomination)
                .ThenInclude(n => n.Employee)
            .Include(e => e.Nomination)
                .ThenInclude(n => n.AwardCycle)
                .ThenInclude(ac => ac.AwardType)
            .ThenInclude(at => at.Criteria)
            .ThenInclude(c => c.SubCriteria)
            .Include(e => e.Nomination.ManagerScores)
            .ThenInclude(ms => ms.SubCriteria)
            .Include(e => e.EvaluationScores)
            .ThenInclude(es => es.SubCriteria)
            .FirstOrDefaultAsync(e => e.EvaluationId == id && e.CommitteeMemberId == committeeMember.Id);

        if (evaluation == null)
        {
            return NotFound();
        }

        return View(evaluation);
    }

    // POST: Evaluations/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Evaluation evaluation)
    {
        if (id != evaluation.EvaluationId)
        {
            return NotFound();
        }

        // TODO: Get current employee from authentication
        var currentEmployeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Get current employee's committee member record
        var committeeMember = await _context.CommitteeMembers
            .Where(cm => cm.EmployeeId == currentEmployeeId && cm.IsActive)
            .FirstOrDefaultAsync();
            
        if (committeeMember == null)
        {
            return Forbid("غير مخول للتقييم");
        }

        evaluation.CommitteeMemberId = committeeMember.Id;

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(evaluation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EvaluationExists(evaluation.EvaluationId))
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

        // Reload data for view
        evaluation = await _context.Evaluations
            .Include(e => e.Nomination)
                .ThenInclude(n => n.Employee)
            .Include(e => e.Nomination)
                .ThenInclude(n => n.AwardCycle)
                .ThenInclude(ac => ac.AwardType)
            .ThenInclude(at => at.Criteria)
            .ThenInclude(c => c.SubCriteria)
            .Include(e => e.EvaluationScores)
            .FirstOrDefaultAsync(e => e.EvaluationId == id) ?? evaluation;

        return View(evaluation);
    }

    // GET: Evaluations/LatestCycle
    [Authorize(Roles = "EOM-Committee,EOM-Committee-Lead")]
    public async Task<IActionResult> LatestCycle()
    {
        // Get the latest cycle (most recent by year/month)
        var latestCycle = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Include(ac => ac.Nominations)
            .ThenInclude(n => n.Employee)
            .ThenInclude(e => e.Department)
            .Include(ac => ac.Nominations)
            .ThenInclude(n => n.Manager)
            .Include(ac => ac.Nominations)
            .ThenInclude(n => n.Evaluations)
            .ThenInclude(e => e.CommitteeMember)
            .ThenInclude(cm => cm.Employee)
            .OrderByDescending(ac => ac.Year)
            .ThenByDescending(ac => ac.Month)
            .FirstOrDefaultAsync();

        if (latestCycle == null)
        {
            TempData["Message"] = "لا توجد دورات للمراجعة";
            return RedirectToAction("Index", "Home");
        }

        return View(latestCycle);
    }

    private bool EvaluationExists(int id)
    {
        return _context.Evaluations.Any(e => e.EvaluationId == id);
    }
    
    // GET: Evaluations/GetAttendance
    [HttpGet]
    [Authorize(Roles = "EOM-Committee,EOM-Committee-Lead,EOM-Admin")]
    public async Task<IActionResult> GetAttendance(int employeeId, int month, int year)
    {
        // Additional security check - verify the user is accessing attendance for evaluation purposes
        var currentEmployeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // For committee members, verify they have access to evaluate this employee's nomination
        if (!User.IsInRole("EOM-Admin"))
        {
            var isCommitteeMember = await _context.CommitteeMembers
                .AnyAsync(cm => cm.EmployeeId == currentEmployeeId && cm.IsActive);
                
            if (!isCommitteeMember)
            {
                return Forbid("غير مخول للوصول لسجلات الحضور");
            }
            
            // Verify there's an active evaluation cycle
            var hasActiveEvaluation = await _context.Nominations
                .AnyAsync(n => n.EmployeeId == employeeId 
                    && n.AwardCycle.Status == CycleStatus.Evaluating
                    && n.AwardCycle.Month == month 
                    && n.AwardCycle.Year == year);
                    
            if (!hasActiveEvaluation)
            {
                return BadRequest("لا توجد دورة تقييم نشطة لهذا الموظف");
            }
        }
        
        // Get employee details
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
            
        if (employee == null)
        {
            return NotFound("الموظف غير موجود");
        }
        
        // Calculate date range for the given month/year
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        // Get the actual employee number from the employee record
        // In this system, EmployeeId and EmployeeNumber appear to be the same
        long employeeNumber = (long)employee.EmployeeId;
        
        // Query attendance records for the employee in the specified month
        // Use raw SQL for better Oracle date handling
        var startDateStr = $"01/{month:D2}/{year}";
        var endDateStr = $"{DateTime.DaysInMonth(year, month):D2}/{month:D2}/{year}";
        
        var attendanceData = await _context.AttendanceRecords
            .FromSqlRaw(@"SELECT * FROM VW_EOM_ATTENDANCE 
                         WHERE EMP_NO = {0} 
                         AND ATT_DATE >= TO_DATE({1}, 'DD/MM/YYYY')
                         AND ATT_DATE <= TO_DATE({2}, 'DD/MM/YYYY')
                         ORDER BY ATT_DATE", 
                         employeeNumber, 
                         startDateStr, 
                         endDateStr)
            .Select(a => new
            {
                a.AttendanceDate,
                a.AttendanceIn,
                a.AttendanceOut,
                a.Difference
            })
            .ToListAsync();
            
        // Format the data after retrieving from database
        var attendanceRecords = attendanceData.Select(a => new
        {
            Date = a.AttendanceDate.ToString("yyyy-MM-dd"),
            DayName = a.AttendanceDate.ToString("dddd", new System.Globalization.CultureInfo("en-US")),
            CheckIn = a.AttendanceIn ?? "-",
            CheckOut = a.AttendanceOut ?? "-",
            Duration = a.Difference ?? "-"
        }).ToList();
        
        return Json(new
        {
            success = true,
            employeeName = $"{employee.FirstName} {employee.LastName}",
            employeeNumber = employeeNumber,
            month = month,
            year = year,
            monthName = new DateTime(year, month, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("en-US")),
            totalDays = attendanceRecords.Count,
            records = attendanceRecords
        });
    }
}