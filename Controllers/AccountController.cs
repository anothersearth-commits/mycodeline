using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.DirectoryServices;
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
    public async Task<IActionResult> Login(string employeeNumber, string password, string authMethod, string? returnUrl = null)
    {
        // Debug logging
        Console.WriteLine($"Login attempt - Employee Number: {employeeNumber}, Auth Method: {authMethod}");
        
        // Parse employee number to integer
        if (!int.TryParse(employeeNumber, out int empId))
        {
            ModelState.AddModelError(string.Empty, "رقم الموظف غير صحيح");
            return View();
        }

        Employee? employee = null;
        bool isAuthenticated = false;

        try
        {
            // First, always get the employee record from database
            employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == empId && e.IsActive == 1);

            if (employee == null)
            {
                ModelState.AddModelError(string.Empty, "رقم الموظف غير موجود أو غير مفعل");
                return View();
            }

            // Authenticate based on selected method
            if (authMethod == "ActiveDirectory")
            {
                isAuthenticated = await AuthenticateWithActiveDirectory(empId.ToString(), password);
                Console.WriteLine($"AD Authentication result: {isAuthenticated}");
            }
            else // Database authentication
            {
                isAuthenticated = employee.Password == password;
                Console.WriteLine($"Database Authentication result: {isAuthenticated}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during authentication: {ex.Message}");
            ModelState.AddModelError(string.Empty, "حدث خطأ أثناء المصادقة");
            return View();
        }

        if (employee != null && isAuthenticated)
        {
            // Create claims for the authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                new Claim(ClaimTypes.Name, $"{employee.FirstName ?? ""} {employee.LastName ?? ""}"),
                new Claim(ClaimTypes.Email, employee.Email ?? "")
            };

            // Check if user is admin using Administrators table
            var administrator = await _context.Administrators
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.EmployeeId && a.IsActive == true);
            if (administrator != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, "EOM-Admin"));
            }

            // Check if user is a manager by looking up in VW_EOM_MANAGERS view
            bool isManager = false;
            try
            {
                Console.WriteLine($"Checking if employee {employee.EmployeeId} is a manager...");
                Console.WriteLine($"_context is null: {_context == null}");
                Console.WriteLine($"_context.Managers is null: {_context.Managers == null}");
                Console.WriteLine($"employee is null: {employee == null}");
                Console.WriteLine($"employee.EmployeeId: {employee?.EmployeeId}");
                
                isManager = await _context.Managers
                    .AnyAsync(m => m.ManagerId == employee.EmployeeId);
                Console.WriteLine($"Manager check result: {isManager}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking manager status: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                isManager = false; // Default to false if there's an error
            }
            
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
                
                // Add Committee Lead role for employee ID = 1
                if (employee.EmployeeId == 9584497)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "EOM-Committee-Lead"));
                }
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

    private async Task<bool> AuthenticateWithActiveDirectory(string employeeId, string password)
    {
        try
        {
            // Configure LDAP connection to bng.local domain
            string ldapPath = "LDAP://10.20.48.4:389/DC=bng,DC=local";
            
            using (var directoryEntry = new DirectoryEntry(ldapPath, $"{employeeId}@bng.local", password))
            {
                // Try to bind to verify credentials
                using (var searcher = new DirectorySearcher(directoryEntry))
                {
                    searcher.Filter = $"(sAMAccountName={employeeId})";
                    searcher.PropertiesToLoad.Add("sAMAccountName");
                    searcher.PropertiesToLoad.Add("displayName");
                    
                    var result = await Task.Run(() => searcher.FindOne());
                    
                    if (result != null)
                    {
                        Console.WriteLine($"AD Authentication successful for user: {employeeId}");
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AD Authentication failed for {employeeId}: {ex.Message}");
        }
        
        return false;
    }
}