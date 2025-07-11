using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;

namespace EOM.Web.Controllers;

[Authorize(Roles = "EOM-Committee,EOM-Admin")]
public class DepartmentQuotasController : Controller
{
    private readonly ApplicationDbContext _context;

    public DepartmentQuotasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: DepartmentQuotas
    public async Task<IActionResult> Index()
    {
        var departments = await _context.Departments
            .Where(d => d.IsActive)
            .Include(d => d.DepartmentQuotas)
            .ThenInclude(dq => dq.AwardType)
            .OrderBy(d => d.Name)
            .ToListAsync();

        var awardTypes = await _context.AwardTypes
            .Where(at => at.IsActive)
            .OrderBy(at => at.Name)
            .ToListAsync();

        ViewBag.AwardTypes = awardTypes;
        return View(departments);
    }

    // POST: DepartmentQuotas/UpdateQuota
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuota(int departmentId, int awardTypeId, int maxNominations)
    {
        try
        {
            var existingQuota = await _context.DepartmentQuotas
                .FirstOrDefaultAsync(dq => dq.DepartmentId == departmentId && dq.AwardTypeId == awardTypeId);

            if (existingQuota != null)
            {
                existingQuota.MaxNominations = maxNominations;
                _context.Update(existingQuota);
            }
            else
            {
                var newQuota = new DepartmentQuota
                {
                    DepartmentId = departmentId,
                    AwardTypeId = awardTypeId,
                    MaxNominations = maxNominations
                };
                _context.Add(newQuota);
            }

            await _context.SaveChangesAsync();
            
            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                Request.Headers["Content-Type"].ToString().Contains("application/json"))
            {
                return Json(new { success = true, message = "تم تحديث حصة الدائرة بنجاح." });
            }
            
            TempData["Success"] = "تم تحديث حصة الدائرة بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                Request.Headers["Content-Type"].ToString().Contains("application/json"))
            {
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث الحصة." });
            }
            
            TempData["Error"] = "حدث خطأ أثناء تحديث الحصة.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: DepartmentQuotas/DeleteQuota
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuota(int departmentId, int awardTypeId)
    {
        var quota = await _context.DepartmentQuotas
            .FirstOrDefaultAsync(dq => dq.DepartmentId == departmentId && dq.AwardTypeId == awardTypeId);

        if (quota != null)
        {
            _context.DepartmentQuotas.Remove(quota);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف حصة الدائرة بنجاح.";
        }
        else
        {
            TempData["Error"] = "لم يتم العثور على الحصة المطلوبة.";
        }

        return RedirectToAction(nameof(Index));
    }
}