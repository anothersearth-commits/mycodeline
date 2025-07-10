using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;

namespace EOM.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // If user is not authenticated, redirect to login
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction("Login", "Account");
        }
        
        // Get open award cycles for managers
        var openCycles = await _context.AwardCycles
            .Include(a => a.AwardType)
            .Where(a => a.Status == CycleStatus.Nomination)
            .OrderByDescending(a => a.Year)
            .ThenByDescending(a => a.Month)
            .ToListAsync();
            
        ViewBag.OpenCycles = openCycles;

        // Get recent cycles for managers to show as cards
        if (User.IsInRole("Manager"))
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
        
        // Get additional manager data if they are a manager
        if (User.IsInRole("Manager"))
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
        }

        // Get committee member data if they are a committee member
        if (User.IsInRole("EOM-Committee"))
        {
            var employeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Check if user is active committee member
            var committeeMember = await _context.CommitteeMembers
                .FirstOrDefaultAsync(cm => cm.EmployeeId == employeeId && cm.IsActive);
                
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
                var latestCycle = await _context.AwardCycles
                    .Include(ac => ac.AwardType)
                    .Include(ac => ac.Nominations)
                    .ThenInclude(n => n.Employee)
                    .OrderByDescending(ac => ac.Year)
                    .ThenByDescending(ac => ac.Month)
                    .FirstOrDefaultAsync();
                
                ViewBag.LatestCycle = latestCycle;
            }
        }
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
