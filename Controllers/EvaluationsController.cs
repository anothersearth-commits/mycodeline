using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;

namespace EOM.Web.Controllers;

[Authorize]
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
            .Where(e => e.CommitteeMemberId == committeeMember.Id && e.Nomination.AwardCycle.Status == CycleStatus.Evaluating)
            .Include(e => e.Nomination)
                .ThenInclude(n => n.Employee)
            .Include(e => e.Nomination)
                .ThenInclude(n => n.AwardCycle)
                .ThenInclude(ac => ac.AwardType)
                .ThenInclude(at => at.Criteria)
                .ThenInclude(c => c.SubCriteria)
            .Include(e => e.EvaluationScores)
            .ToListAsync();

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
        var pendingNominations = await _context.Nominations
            .Include(n => n.AwardCycle)
                .ThenInclude(ac => ac.AwardType)
            .Include(n => n.Employee)
                .ThenInclude(e => e.Department)
            .Where(n => n.AwardCycle.Status == CycleStatus.Evaluating)
            .Where(n => !n.Evaluations.Any(e => e.CommitteeMemberId == committeeMember.Id))
            .OrderBy(n => n.Employee.DepartmentId)
            .ThenBy(n => n.Employee.LastName)
            .ToListAsync();

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
            return RedirectToAction("Edit", new { id = existingEvaluation.EvaluationId });
        }

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
    public async Task<IActionResult> Create(Evaluation evaluation)
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
    [Authorize(Roles = "EOM-Committee")]
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
}