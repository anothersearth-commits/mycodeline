using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;

namespace EOM.Web.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(string? activeRole = null)
    {
        // If user is not authenticated, redirect to login
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get role data from base controller
        bool isManager = User.IsInRole("Manager");
        bool isCommittee = User.IsInRole("EOM-Committee");
        bool isDualRole = ViewBag.IsDualRole as bool? ?? false;
        string currentRole = ViewBag.CurrentRole as string ?? "Committee";
        
        // Get open award cycles for managers (not closed or published)
        var openCycles = await _context.AwardCycles
            .Include(a => a.AwardType)
            .Where(a => a.Status == CycleStatus.Nomination || a.Status == CycleStatus.Evaluating)
            .OrderByDescending(a => a.Year)
            .ThenByDescending(a => a.Month)
            .ToListAsync();
            
        // Get self-nomination cycles specifically
        var selfNominationCycles = await _context.AwardCycles
            .Include(a => a.AwardType)
            .Where(a => a.Status == CycleStatus.Nomination && 
                       a.AwardType.IsSelfNomination == true)
            .OrderByDescending(a => a.Year)
            .ThenByDescending(a => a.Month)
            .ToListAsync();
            
        ViewBag.OpenCycles = openCycles;
        ViewBag.SelfNominationCycles = selfNominationCycles;

        // Get department nominations for all employees to show at the top
        var currentEmployeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var currentEmployee = await _context.Employees.FindAsync(currentEmployeeId);
        
        if (currentEmployee != null)
        {
            // Get the latest award cycle for department nominations
            var latestCycleForDept = await _context.AwardCycles
                .OrderByDescending(ac => ac.Year)
                .ThenByDescending(ac => ac.Month)
                .FirstOrDefaultAsync();
            
            if (latestCycleForDept != null)
            {
                // Get recent nominations from the same department but only from the latest cycle
                var allDepartmentNominations = await _context.Nominations
                    .Include(n => n.Employee)
                    .Include(n => n.Manager)
                    .Include(n => n.AwardCycle)
                    .ThenInclude(ac => ac.AwardType)
                    .Where(n => n.Employee.DepartmentId == currentEmployee.DepartmentId && 
                               n.CycleId == latestCycleForDept.CycleId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
                
                // Remove duplicates by employee ID in memory
                var departmentNominations = allDepartmentNominations
                    .GroupBy(n => n.EmployeeId)
                    .Select(g => g.First())
                    .Take(10)
                    .ToList();
                
                ViewBag.DepartmentNominations = departmentNominations;
            }
            else
            {
                ViewBag.DepartmentNominations = new List<Nomination>();
            }
            
            ViewBag.CurrentEmployeeDepartment = currentEmployee.Department?.Name ?? "غير محدد";
        }

        // Get recent cycles for managers to show as cards
        if (isManager && (currentRole == "Manager" || !isDualRole))
        {
            var employeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Get last 6 cycles with manager's nominations
            var recentCycles = await _context.AwardCycles
                .Include(ac => ac.AwardType)
                .Include(ac => ac.Nominations.Where(n => n.ManagerId == employeeId))
                .ThenInclude(n => n.Employee)
                .Include(ac => ac.Nominations.Where(n => n.ManagerId == employeeId))
                .ThenInclude(n => n.ManagerScores)
                .Include(ac => ac.Nominations.Where(n => n.ManagerId == employeeId))
                .ThenInclude(n => n.Evaluations)
                .OrderByDescending(ac => ac.Year)
                .ThenByDescending(ac => ac.Month)
                .Take(6)
                .ToListAsync();
            ViewBag.RecentCycles = recentCycles;
        }
        
        // Get past cycles (closed or published) for reference - for non-managers
        var pastCycles = await _context.AwardCycles
            .Include(a => a.AwardType)
            .Where(a => a.Status == CycleStatus.Closed || a.Status == CycleStatus.Published)
            .OrderByDescending(a => a.Year)
            .ThenByDescending(a => a.Month)
            .Take(10)
            .ToListAsync();
        ViewBag.PastCycles = pastCycles;
        
        // Get additional manager data if they are a manager and in manager mode
        if (isManager && (currentRole == "Manager" || !isDualRole))
        {
            var employeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var hasNominations = await _context.Nominations
                .AnyAsync(n => n.ManagerId == employeeId && openCycles.Select(c => c.CycleId).Contains(n.CycleId));
            ViewBag.HasNominations = hasNominations;

            // Get current nominations for the current cycles
            var currentNominations = await _context.Nominations
                .Include(n => n.Employee)
                .Include(n => n.AwardCycle)
                .ThenInclude(c => c.AwardType)
                .Where(n => n.ManagerId == employeeId && openCycles.Select(c => c.CycleId).Contains(n.CycleId))
                .ToListAsync();
            ViewBag.CurrentNominations = currentNominations;

            // Manager-specific metrics for floating cards
            // Get total nominations by this manager across all cycles
            var managerTotalNominations = await _context.Nominations
                .Where(n => n.ManagerId == employeeId)
                .CountAsync();
            ViewBag.ManagerTotalNominations = managerTotalNominations;

            // Get manager's nominations in current active cycles
            var managerCurrentNominations = currentNominations.Count;
            ViewBag.ManagerCurrentNominations = managerCurrentNominations;

            // Get pending scoring count (nominations that need manager scoring)
            var pendingScoring = await _context.Nominations
                .Where(n => n.ManagerId == employeeId && 
                           openCycles.Select(c => c.CycleId).Contains(n.CycleId) &&
                           !n.ManagerScores.Any())
                .CountAsync();
            ViewBag.ManagerPendingScoring = pendingScoring;
        }

                var latestCycle = await _context.AwardCycles
                    .Include(ac => ac.AwardType)
                    .Include(ac => ac.Nominations)
                    .OrderByDescending(ac => ac.Year)
                    .ThenByDescending(ac => ac.Month)
                    .FirstOrDefaultAsync();
                
                ViewBag.LatestCycle = latestCycle;
                
        // Get committee member data if they are a committee member and in committee mode
        if (isCommittee && (currentRole == "Committee" || !isDualRole))
        {
            var employeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Check if user is active committee member
            var committeeMember = await _context.CommitteeMembers
                .FirstOrDefaultAsync(cm => cm.EmployeeId == employeeId && cm.IsActive == true);
                
            if (committeeMember != null)
            {
                // Get evaluation cycles (cycles in evaluation status)
                var evaluationCycles = await _context.AwardCycles
                    .Include(ac => ac.AwardType)
                    .Where(ac => ac.Status == CycleStatus.Evaluating)
                    .ToListAsync();
                
                ViewBag.EvaluationCycles = evaluationCycles;

                // Get all nominations that need evaluation from this committee member
                var pendingNominations = await _context.Nominations
                    .AsNoTracking()
                    .Include(n => n.Employee)
                    .Include(n => n.AwardCycle)
                    .ThenInclude(ac => ac.AwardType)
                    .Where(n => evaluationCycles.Select(ec => ec.CycleId).Contains(n.CycleId) &&
                               !n.Evaluations.Any(e => e.CommitteeMemberId == committeeMember.Id))
                    .ToListAsync();
                
                // Remove any duplicates based on NominationId
                pendingNominations = pendingNominations
                    .GroupBy(n => n.NominationId)
                    .Select(g => g.First())
                    .ToList();
                
                ViewBag.PendingEvaluations = pendingNominations;
                ViewBag.PendingCount = pendingNominations.Count();

                // Get completed evaluations by this committee member
                var completedEvaluations = await _context.Evaluations
                    .AsNoTracking()
                    .Include(e => e.Nomination)
                    .ThenInclude(n => n.Employee)
                    .Include(e => e.Nomination)
                    .ThenInclude(n => n.AwardCycle)
                    .ThenInclude(ac => ac.AwardType)
                    .ThenInclude(at => at.Criteria)
                    .ThenInclude(c => c.SubCriteria)
                    .Include(e => e.EvaluationScores)
                    .ThenInclude(es => es.SubCriteria)
                    .Where(e => e.CommitteeMemberId == committeeMember.Id &&
                               evaluationCycles.Select(ec => ec.CycleId).Contains(e.Nomination.CycleId))
                    .ToListAsync();
                
                // Remove any duplicates based on EvaluationId
                completedEvaluations = completedEvaluations
                    .GroupBy(e => e.EvaluationId)
                    .Select(g => g.First())
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(10)
                    .ToList();
                
                // Get the actual count before limiting to 10
                var actualCompletedCount = await _context.Evaluations
                    .Where(e => e.CommitteeMemberId == committeeMember.Id &&
                               evaluationCycles.Select(ec => ec.CycleId).Contains(e.Nomination.CycleId))
                    .Select(e => e.EvaluationId)
                    .Distinct()
                    .CountAsync();
                
                ViewBag.CompletedEvaluations = completedEvaluations;
                ViewBag.CompletedCount = actualCompletedCount;

                // Calculate progress
                var totalEvaluations = pendingNominations.Count() + actualCompletedCount;
                var progressPercentage = totalEvaluations > 0 ? (actualCompletedCount * 100) / totalEvaluations : 0;
                ViewBag.ProgressPercentage = progressPercentage;

                // Calculate days remaining (assuming evaluation period ends at month end)
                var currentDate = DateTime.Now;
                var endOfMonth = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
                var daysRemaining = Math.Max(0, (endOfMonth - currentDate).Days);
                ViewBag.DaysRemaining = daysRemaining;

                // Recent activity - last 5 completed evaluations
                var recentActivity = completedEvaluations.Take(5).ToList();
                ViewBag.RecentActivity = recentActivity;

                // Get the latest cycle (most recent by year/month) for committee review
                // Note: Excluding Employee include due to VW_EOM_EMPLOYEES database link issues

            }
        }
        
        // Get admin statistics if user is admin
        if (User.IsInRole("EOM-Admin"))
        {
            // Get active nominations count
            var activeNominations = await _context.Nominations
                .Where(n => openCycles.Select(c => c.CycleId).Contains(n.CycleId))
                .CountAsync();
            ViewBag.ActiveNominationsCount = activeNominations;

            // Get completed evaluations count  
            var completedEvaluations = await _context.Evaluations
                .Where(e => openCycles.Select(c => c.CycleId).Contains(e.Nomination.CycleId))
                .CountAsync();
            ViewBag.CompletedEvaluationsCount = completedEvaluations;

            // Get participating departments count
            var participatingDepartments = await _context.Nominations
                .Where(n => openCycles.Select(c => c.CycleId).Contains(n.CycleId))
                .Select(n => n.Employee.DepartmentId)
                .Distinct()
                .CountAsync();
            ViewBag.ParticipatingDepartmentsCount = participatingDepartments;

            // Get active cycles count
            ViewBag.ActiveCyclesCount = openCycles.Count;
        }

        // Get committee members progress for last open cycle with AwardType ID 1
        var lastOpenCycleAwardType1 = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Include(ac => ac.Nominations)
            .Where(ac => ac.AwardTypeId == 1 && (ac.Status == CycleStatus.Nomination || ac.Status == CycleStatus.Evaluating))
            .OrderByDescending(ac => ac.Year)
            .ThenByDescending(ac => ac.Month)
            .FirstOrDefaultAsync();

        ViewBag.LastOpenCycleAwardType1 = lastOpenCycleAwardType1;

        // Debug: Log what we found
        _logger.LogInformation($"Found cycle for AwardType 1: {lastOpenCycleAwardType1?.CycleId} - {lastOpenCycleAwardType1?.AwardType?.Name} - {lastOpenCycleAwardType1?.Status}");

        // Always get committee members for debugging, even if no cycle
        var allCommitteeMembers = await _context.CommitteeMembers
            .Include(cm => cm.Employee)
            .Where(cm => cm.IsActive == true)
            .ToListAsync();

        _logger.LogInformation($"Found {allCommitteeMembers.Count} active committee members");

        if (lastOpenCycleAwardType1 != null)
        {
            var cycleId = lastOpenCycleAwardType1.CycleId;
            var totalNominations = lastOpenCycleAwardType1.Nominations.Count();
            
            _logger.LogInformation($"Cycle {cycleId} has {totalNominations} nominations");

            // Get all active committee members first
            var activeCommitteeMembers = await _context.CommitteeMembers
                .Include(cm => cm.Employee)
                .Where(cm => cm.IsActive == true)
                .ToListAsync();

            _logger.LogInformation($"Found {activeCommitteeMembers.Count} active committee members for progress calculation");

            // Calculate progress for each member
            var committeeMembers = new List<dynamic>();
            foreach (var member in activeCommitteeMembers)
            {
                var completedEvaluations = await _context.Evaluations
                    .Where(e => e.CommitteeMemberId == member.Id && 
                               e.Nomination.CycleId == cycleId)
                    .CountAsync();

                var progressPercentage = totalNominations > 0 ? (completedEvaluations * 100) / totalNominations : 0;

                var memberData = new
                {
                    Id = member.Id,
                    FirstName = member.Employee?.FirstName ?? "Unknown",
                    LastName = member.Employee?.LastName ?? "Member",
                    ActiveDirectoryId = member.Employee?.ActiveDirectoryId,
                    TotalEvaluations = totalNominations,
                    CompletedEvaluations = completedEvaluations,
                    ProgressPercentage = progressPercentage
                };

                committeeMembers.Add(memberData);
                _logger.LogInformation($"Member {memberData.FirstName} {memberData.LastName}: {completedEvaluations}/{totalNominations} ({progressPercentage}%)");
            }

            _logger.LogInformation($"Committee members with progress: {committeeMembers.Count}");
            
            ViewBag.CommitteeMembers = committeeMembers;
        }
        else
        {
            // Check if there are any cycles with AwardType 1 at all
            var anyCycleAwardType1 = await _context.AwardCycles
                .Where(ac => ac.AwardTypeId == 1)
                .CountAsync();
            
            _logger.LogInformation($"Total cycles with AwardType 1: {anyCycleAwardType1}");
            
            // Check what award types exist
            var awardTypes = await _context.AwardTypes.ToListAsync();
            _logger.LogInformation($"Available AwardTypes: {string.Join(", ", awardTypes.Select(at => $"{at.AwardTypeId}: {at.Name}"))}");
            
            // Still show committee members even without an active cycle
            var committeeMembers = allCommitteeMembers.Select(member => new
            {
                Id = member.Id,
                FirstName = member.Employee?.FirstName ?? "Unknown",
                LastName = member.Employee?.LastName ?? "Member",
                ActiveDirectoryId = member.Employee?.ActiveDirectoryId,
                TotalEvaluations = 0,
                CompletedEvaluations = 0,
                ProgressPercentage = 0
            }).ToList();
            
            ViewBag.CommitteeMembers = committeeMembers;
            _logger.LogInformation($"Set committee members without cycle: {committeeMembers.Count}");
        }
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // GET: Home/Leaderboard
    public async Task<IActionResult> Leaderboard()
    {
        // Get the latest closed cycle for each award type
        var latestCyclesByType = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Where(ac => ac.Status == CycleStatus.Closed || ac.Status == CycleStatus.Published)
            .GroupBy(ac => ac.AwardTypeId)
            .Select(g => g.OrderByDescending(ac => ac.Year)
                         .ThenByDescending(ac => ac.Month)
                         .FirstOrDefault())
            .ToListAsync();

        if (!latestCyclesByType.Any())
        {
            ViewBag.Message = "لا توجد دورات مكتملة لعرض النتائج";
            return View(new List<Nomination>());
        }

        // Get the cycle IDs
        var cycleIds = latestCyclesByType.Select(c => c.CycleId).ToList();

        // Get all winners from these latest cycles
        var allWinners = await _context.Nominations
            .Include(n => n.Employee)
            .Include(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .Include(n => n.SelectedByCommitteeMember)
            .Include(n => n.GroupMembers)
            .ThenInclude(gm => gm.Employee)
            .Where(n => cycleIds.Contains(n.CycleId) && n.IsWinner == 1)
            .OrderBy(n => n.AwardCycle.AwardTypeId)
            .ThenBy(n => n.Employee.FirstName)
            .ToListAsync();

        // Get the most recent date from all cycles
        var mostRecentCycle = latestCyclesByType
            .OrderByDescending(c => c.Year)
            .ThenByDescending(c => c.Month)
            .FirstOrDefault();

        ViewBag.ClosedCycles = latestCyclesByType;
        ViewBag.LatestMonth = mostRecentCycle?.Month ?? 0;
        ViewBag.LatestYear = mostRecentCycle?.Year ?? 0;
        ViewBag.TotalWinners = allWinners.Count;
        
        return View(allWinners);
    }

    [HttpGet]
    public IActionResult WinnerPhoto(string adUser)
    {
        if (string.IsNullOrEmpty(adUser))
            return File("~/img/default-user.png", "image/png");

        try
        {
            string ldapPath = "LDAP://10.20.48.4:389/DC=bng,DC=local";
            using (var entry = new System.DirectoryServices.DirectoryEntry(ldapPath))
            using (var searcher = new System.DirectoryServices.DirectorySearcher(entry))
            {
                searcher.Filter = $"(sAMAccountName={adUser})";
                searcher.PropertiesToLoad.Add("thumbnailPhoto");
                var result = searcher.FindOne();
                if (result != null && result.Properties.Contains("thumbnailPhoto"))
                {
                    byte[] photoBytes = result.Properties["thumbnailPhoto"][0] as byte[];
                    if (photoBytes != null && photoBytes.Length > 0)
                    {
                        return File(photoBytes, "image/jpeg");
                    }
                }
            }
        }
        catch
        {
            // Optionally log error
        }
        return File("~/img/default-user.png", "image/png");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
