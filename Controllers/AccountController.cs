using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EOM.Web.Data;
using EOM.Web.Models;

namespace EOM.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string employeeNumber, string password, string? returnUrl = null)
    {
        // Debug logging
        Console.WriteLine($"Login attempt - Employee Number: {employeeNumber}, Password: {password}");
        
        // Parse employee number to integer
        if (!int.TryParse(employeeNumber, out int empId))
        {
            ModelState.AddModelError(string.Empty, "رقم الموظف غير صحيح");
            return View();
        }

        // Find employee by ID and verify password using EF Core
        Employee? employee = null;
        try
        {
            employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == empId && e.IsActive == 1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing employee lookup: {ex.Message}");
            return View();
        }

        Console.WriteLine($"Employee found: {employee?.FirstName}, Password in DB: {employee?.Password}");

        if (employee != null && employee.Password == password)
        {
            // Create claims for the authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                new Claim(ClaimTypes.Name, $"{employee.FirstName ?? ""} {employee.LastName ?? ""}"),
                new Claim(ClaimTypes.Email, employee.Email ?? "")
            };

            // Check if user is admin (employee ID 1)
            if (employee.EmployeeId == 1)
            {
                claims.Add(new Claim(ClaimTypes.Role, "EOM-Admin"));
            }

            // Check if user is a manager using Employee.IsManager field
            var isManager = employee.IsManager == 1;
            if (isManager)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Manager"));
            }

            // Check if user is a committee member using EF Core
            CommitteeMember? committeeMember = null;
            try
            {
                committeeMember = await _context.CommitteeMembers
                    .FirstOrDefaultAsync(cm => cm.EmployeeId == employee.EmployeeId && cm.IsActive == true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing committee member lookup: {ex.Message}");
            }
            if (committeeMember != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, "EOM-Committee"));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var authProperties = new AuthenticationProperties();

            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "رقم الموظف أو كلمة المرور غير صحيحة");
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Index", "Home");
    }
}