using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EOM.Web.Controllers;

public class BaseController : Controller
{
    // Helper methods for committee role checking
    protected bool IsCommitteeMember => User.IsInRole("EOM-Committee");
    protected bool IsCommitteeLead => User.IsInRole("EOM-Committee-Lead");
    protected bool IsCommitteeOrLead => IsCommitteeMember || IsCommitteeLead;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            // Role switcher logic for dual-role users
            bool isManager = User.IsInRole("Manager");
            bool isCommittee = User.IsInRole("EOM-Committee");
            bool isCommitteeLead = User.IsInRole("EOM-Committee-Lead");
            bool isDualRole = isManager && (isCommittee || isCommitteeLead);
            
            string? activeRoleParam = HttpContext.Request.Query["activeRole"].FirstOrDefault();
            string? cookieRole = HttpContext.Request.Cookies["UserActiveRole"];
            
            // Default to committee role for dual-role users, unless explicitly switching
            string currentRole = "Committee"; // Default
            if (isDualRole)
            {
                // Priority: URL parameter > Cookie > Default
                currentRole = activeRoleParam ?? cookieRole ?? "Committee";
                
                // If activeRole was provided in URL, update the cookie
                if (!string.IsNullOrEmpty(activeRoleParam))
                {
                    HttpContext.Response.Cookies.Append("UserActiveRole", activeRoleParam, new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(30),
                        HttpOnly = true,
                        SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax
                    });
                }
            }
            else if (isManager)
            {
                currentRole = "Manager";
            }
            else if (isCommittee || isCommitteeLead)
            {
                currentRole = "Committee";
            }
            
            ViewBag.IsDualRole = isDualRole;
            ViewBag.CurrentRole = currentRole;
        }
        
        base.OnActionExecuting(context);
    }
}