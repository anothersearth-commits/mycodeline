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
