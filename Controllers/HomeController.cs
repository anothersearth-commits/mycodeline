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
            
        ViewBag.OpenCycles = openCycles;

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
                    .Include(n => n.Employee)
                    .Include(n => n.AwardCycle)
                    .ThenInclude(ac => ac.AwardType)
                    .Where(n => evaluationCycles.Select(ec => ec.CycleId).Contains(n.CycleId) &&
                               !n.Evaluations.Any(e => e.CommitteeMemberId == committeeMember.Id))
                    .ToListAsync();
                
                ViewBag.PendingEvaluations = pendingNominations;
                ViewBag.PendingCount = pendingNominations.Count();

                // Get completed evaluations by this committee member
                var completedEvaluations = await _context.Evaluations
                    .Include(e => e.Nomination)
                    .ThenInclude(n => n.Employee)
                    .Include(e => e.Nomination)
                    .ThenInclude(n => n.AwardCycle)
                    .ThenInclude(ac => ac.AwardType)
                    .Include(e => e.EvaluationScores)
                    .Where(e => e.CommitteeMemberId == committeeMember.Id &&
                               evaluationCycles.Select(ec => ec.CycleId).Contains(e.Nomination.CycleId))
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(10)
                    .ToListAsync();
                
                ViewBag.CompletedEvaluations = completedEvaluations;
                ViewBag.CompletedCount = completedEvaluations.Count();

                // Calculate progress
                var totalEvaluations = pendingNominations.Count() + completedEvaluations.Count();
                var progressPercentage = totalEvaluations > 0 ? (completedEvaluations.Count() * 100) / totalEvaluations : 0;
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
                var latestCycle = await _context.AwardCycles
                    .Include(ac => ac.AwardType)
                    .Include(ac => ac.Nominations)
                    .OrderByDescending(ac => ac.Year)
                    .ThenByDescending(ac => ac.Month)
                    .FirstOrDefaultAsync();
                
                ViewBag.LatestCycle = latestCycle;
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
        // Get the last closed cycle
        var lastClosedCycle = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Where(ac => ac.Status == CycleStatus.Closed || ac.Status == CycleStatus.Published)
            .OrderByDescending(ac => ac.Year)
            .ThenByDescending(ac => ac.Month)
            .FirstOrDefaultAsync();

        if (lastClosedCycle == null)
        {
            ViewBag.Message = "لا توجد دورات مكتملة لعرض النتائج";
            return View(new List<Nomination>());
        }

        // Get all winners from the last closed cycle with their employee data
        var winners = await _context.Nominations
            .Include(n => n.Employee)
            .Include(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .Include(n => n.SelectedByCommitteeMember)
            .Where(n => n.CycleId == lastClosedCycle.CycleId && n.IsWinner)
            .OrderBy(n => n.Employee.FirstName)
            .ToListAsync();

        ViewBag.CycleInfo = lastClosedCycle;
        ViewBag.TotalWinners = winners.Count;
        
        return View(winners);
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
