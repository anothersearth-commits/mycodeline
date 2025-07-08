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

        // Get past cycles (closed or published) for reference
        var pastCycles = await _context.AwardCycles
            .Include(a => a.AwardType)
            .Where(a => a.Status == CycleStatus.Closed || a.Status == CycleStatus.Published)
            .OrderByDescending(a => a.Year)
            .ThenByDescending(a => a.Month)
            .Take(10)
            .ToListAsync();
        ViewBag.PastCycles = pastCycles;
        
        // Get user's employee ID if they are a manager
        if (User.IsInRole("Manager"))
        {
            var employeeId = int.Parse(User.FindFirst("EmployeeId")?.Value ?? "0");
            var hasNominations = await _context.Nominations
                .AnyAsync(n => n.ManagerId == employeeId && openCycles.Select(c => c.CycleId).Contains(n.CycleId));
            ViewBag.HasNominations = hasNominations;
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
