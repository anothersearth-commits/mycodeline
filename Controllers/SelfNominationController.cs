using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;
using System.Security.Claims;

namespace EOM.Web.Controllers;

[Authorize]
public class SelfNominationController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SelfNominationController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: SelfNomination
    public async Task<IActionResult> Index()
    {
        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var currentEmployee = await _context.Employees.FindAsync(currentEmployeeId);
        
        if (currentEmployee == null)
        {
            return NotFound();
        }

        // Check if employee has won Employee of the Month (AwardTypeId = 1)
        var hasWonEmployeeOfMonth = await _context.Nominations
            .Include(n => n.AwardCycle)
            .AnyAsync(n => n.EmployeeId == currentEmployeeId && 
                          n.IsWinner == 1 && 
                          n.AwardCycle.AwardTypeId == 1);

        // Get active self-nomination cycles only
        var activeSelfNominationCycles = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Where(ac => ac.Status == CycleStatus.Nomination && ac.AwardType.IsSelfNomination)
            .ToListAsync();

        // Filter out award types 2 and 3 if employee has won type 1
        if (hasWonEmployeeOfMonth)
        {
            var restrictedCycles = activeSelfNominationCycles.Where(c => c.AwardTypeId == 2 || c.AwardTypeId == 3).ToList();
            if (restrictedCycles.Any())
            {
                ViewBag.RestrictedAwards = restrictedCycles.Select(c => c.AwardType?.Name).ToList();
            }
            activeSelfNominationCycles = activeSelfNominationCycles.Where(c => c.AwardTypeId != 2 && c.AwardTypeId != 3).ToList();
            ViewBag.HasWonEmployeeOfMonth = true;
        }
        
        // Check if employee has already applied for award type 2 or 3 (as main nominee)
        var existingNominationsInActiveCycles = await _context.Nominations
            .Include(n => n.AwardCycle)
            .Where(n => n.EmployeeId == currentEmployeeId && 
                       n.IsSelfNomination &&
                       n.AwardCycle.Status == CycleStatus.Nomination &&
                       (n.AwardCycle.AwardTypeId == 2 || n.AwardCycle.AwardTypeId == 3))
            .Select(n => n.AwardCycle.AwardTypeId)
            .ToListAsync();
        
        // Also check if employee is a team member in award type 2 or 3
        var teamMemberInAwards = await _context.GroupNominationMembers
            .Include(gm => gm.Nomination)
            .ThenInclude(n => n.AwardCycle)
            .Where(gm => gm.EmployeeId == currentEmployeeId &&
                        gm.Nomination.IsSelfNomination &&
                        gm.Nomination.AwardCycle.Status == CycleStatus.Nomination &&
                        (gm.Nomination.AwardCycle.AwardTypeId == 2 || gm.Nomination.AwardCycle.AwardTypeId == 3))
            .Select(gm => gm.Nomination.AwardCycle.AwardTypeId)
            .ToListAsync();
        
        // Combine both lists to get all award types the employee is involved in
        var allInvolvedAwardTypes = existingNominationsInActiveCycles.Union(teamMemberInAwards).ToList();
        
        // Track which award they are already involved in (either as main nominee or team member)
        if (allInvolvedAwardTypes.Contains(2))
        {
            ViewBag.AlreadyAppliedForAward2 = true;
            var restrictedCycle = activeSelfNominationCycles.FirstOrDefault(c => c.AwardTypeId == 3);
            if (restrictedCycle != null)
            {
                ViewBag.RestrictedDueToAward2 = restrictedCycle.AwardType?.Name;
                activeSelfNominationCycles = activeSelfNominationCycles.Where(c => c.AwardTypeId != 3).ToList();
            }
            
            // Check if they are a team member
            if (teamMemberInAwards.Contains(2) && !existingNominationsInActiveCycles.Contains(2))
            {
                ViewBag.IsTeamMemberInAward2 = true;
            }
        }
        
        if (allInvolvedAwardTypes.Contains(3))
        {
            ViewBag.AlreadyAppliedForAward3 = true;
            var restrictedCycle = activeSelfNominationCycles.FirstOrDefault(c => c.AwardTypeId == 2);
            if (restrictedCycle != null)
            {
                ViewBag.RestrictedDueToAward3 = restrictedCycle.AwardType?.Name;
                activeSelfNominationCycles = activeSelfNominationCycles.Where(c => c.AwardTypeId != 2).ToList();
            }
            
            // Check if they are a team member
            if (teamMemberInAwards.Contains(3) && !existingNominationsInActiveCycles.Contains(3))
            {
                ViewBag.IsTeamMemberInAward3 = true;
            }
        }

        if (!activeSelfNominationCycles.Any())
        {
            if (hasWonEmployeeOfMonth)
            {
                ViewBag.Message = "عذراً، لا يمكنك الترشح للجوائز المتاحة حالياً لأنك فزت بجائزة موظف الشهر سابقاً";
            }
            else if (ViewBag.AlreadyAppliedForAward2 == true || ViewBag.AlreadyAppliedForAward3 == true)
            {
                ViewBag.Message = "عذراً، لقد قمت بالترشح لإحدى الجوائز ولا يمكنك الترشح لأكثر من جائزة واحدة في نفس الفترة";
            }
            else
            {
                ViewBag.Message = "لا توجد دورات ترشيح ذاتي نشطة حالياً";
            }
        }

        // Check if employee already has nominations in these cycles (as main nominee)
        var existingNominations = await _context.Nominations
            .Where(n => n.EmployeeId == currentEmployeeId && n.IsSelfNomination)
            .Select(n => n.CycleId)
            .ToListAsync();

        // Get nominations where the employee is part of a team
        var teamNominations = await _context.GroupNominationMembers
            .Include(gm => gm.Nomination)
            .ThenInclude(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .Where(gm => gm.EmployeeId == currentEmployeeId)
            .Select(gm => gm.Nomination)
            .ToListAsync();

        // Combine own nominations and team nominations
        var userNominations = await _context.Nominations
            .Include(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .Where(n => n.EmployeeId == currentEmployeeId && n.IsSelfNomination)
            .ToDictionaryAsync(n => n.CycleId, n => n);

        // Add team nominations to the dictionary
        foreach (var teamNom in teamNominations)
        {
            if (!userNominations.ContainsKey(teamNom.CycleId))
            {
                userNominations[teamNom.CycleId] = teamNom;
                existingNominations.Add(teamNom.CycleId);
            }
        }

        ViewData["ExistingNominations"] = existingNominations;
        ViewData["UserNominations"] = userNominations;
        ViewData["TeamNominations"] = teamNominations.Select(n => n.CycleId).ToList();
        return View(activeSelfNominationCycles);
    }

    // GET: SelfNomination/Create
    public async Task<IActionResult> Create(int? cycleId)
    {
        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var currentEmployee = await _context.Employees.FindAsync(currentEmployeeId);
        
        if (currentEmployee == null)
        {
            return NotFound();
        }

        // Check if employee has won Employee of the Month (AwardTypeId = 1)
        var hasWonEmployeeOfMonth = await _context.Nominations
            .Include(n => n.AwardCycle)
            .AnyAsync(n => n.EmployeeId == currentEmployeeId && 
                          n.IsWinner == 1 && 
                          n.AwardCycle.AwardTypeId == 1);

        // Get active self-nomination cycles
        var activeCycles = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .Where(ac => ac.Status == CycleStatus.Nomination && ac.AwardType.IsSelfNomination)
            .ToListAsync();
        
        // Filter out award types 2 and 3 if employee has won type 1
        if (hasWonEmployeeOfMonth)
        {
            activeCycles = activeCycles.Where(c => c.AwardTypeId != 2 && c.AwardTypeId != 3).ToList();
            ViewBag.HasWonEmployeeOfMonth = true;
        }
        
        // Check if employee has already applied for award type 2 or 3 in active cycles (as main nominee)
        var existingNominationsInActiveCycles = await _context.Nominations
            .Include(n => n.AwardCycle)
            .Where(n => n.EmployeeId == currentEmployeeId && 
                       n.IsSelfNomination &&
                       n.AwardCycle.Status == CycleStatus.Nomination &&
                       (n.AwardCycle.AwardTypeId == 2 || n.AwardCycle.AwardTypeId == 3))
            .Select(n => n.AwardCycle.AwardTypeId)
            .ToListAsync();
        
        // Also check if employee is a team member in award type 2 or 3
        var teamMemberInAwards = await _context.GroupNominationMembers
            .Include(gm => gm.Nomination)
            .ThenInclude(n => n.AwardCycle)
            .Where(gm => gm.EmployeeId == currentEmployeeId &&
                        gm.Nomination.IsSelfNomination &&
                        gm.Nomination.AwardCycle.Status == CycleStatus.Nomination &&
                        (gm.Nomination.AwardCycle.AwardTypeId == 2 || gm.Nomination.AwardCycle.AwardTypeId == 3))
            .Select(gm => gm.Nomination.AwardCycle.AwardTypeId)
            .ToListAsync();
        
        // Combine both lists
        var allInvolvedAwardTypes = existingNominationsInActiveCycles.Union(teamMemberInAwards).ToList();
        
        // If employee already involved in award 2, remove award 3 from available cycles
        if (allInvolvedAwardTypes.Contains(2))
        {
            activeCycles = activeCycles.Where(c => c.AwardTypeId != 3).ToList();
            ViewBag.AlreadyAppliedForAward2 = true;
            if (teamMemberInAwards.Contains(2) && !existingNominationsInActiveCycles.Contains(2))
            {
                ViewBag.IsTeamMemberInAward2 = true;
            }
        }
        
        // If employee already involved in award 3, remove award 2 from available cycles
        if (allInvolvedAwardTypes.Contains(3))
        {
            activeCycles = activeCycles.Where(c => c.AwardTypeId != 2).ToList();
            ViewBag.AlreadyAppliedForAward3 = true;
            if (teamMemberInAwards.Contains(3) && !existingNominationsInActiveCycles.Contains(3))
            {
                ViewBag.IsTeamMemberInAward3 = true;
            }
        }

        if (!activeCycles.Any())
        {
            if (hasWonEmployeeOfMonth)
            {
                ViewBag.Message = "عذراً، لا يمكنك الترشح للجوائز المتاحة حالياً لأنك فزت بجائزة موظف الشهر سابقاً";
            }
            else if (ViewBag.AlreadyAppliedForAward2 == true || ViewBag.AlreadyAppliedForAward3 == true)
            {
                ViewBag.Message = "عذراً، لقد قمت بالترشح لإحدى الجوائز ولا يمكنك الترشح لأكثر من جائزة واحدة في نفس الفترة";
            }
            else
            {
                ViewBag.Message = "لا توجد دورات ترشيح ذاتي نشطة حالياً";
            }
            return View();
        }

        // If more than one active cycle, let employee choose; otherwise preselect
        if (!cycleId.HasValue && activeCycles.Count == 1)
        {
            cycleId = activeCycles.First().CycleId;
        }

        // Validate provided cycleId
        if (cycleId.HasValue && !activeCycles.Any(c => c.CycleId == cycleId.Value))
        {
            return RedirectToAction(nameof(Index));
        }

        if (cycleId.HasValue)
        {
            // Check if employee already nominated themselves in this cycle
            var existingNomination = await _context.Nominations
                .FirstOrDefaultAsync(n => n.EmployeeId == currentEmployeeId && n.CycleId == cycleId.Value && n.IsSelfNomination);
            
            if (existingNomination != null)
            {
                TempData["ErrorMessage"] = "لقد قمت بالترشح في هذه الدورة بالفعل";
                return RedirectToAction(nameof(Index));
            }
            
            // Load criteria for the selected award type
            var selectedCycle = activeCycles.FirstOrDefault(c => c.CycleId == cycleId.Value);
            if (selectedCycle != null)
            {
                var criteria = await _context.Criteria
                    .Include(c => c.SubCriteria)
                    .Where(c => c.AwardTypeId == selectedCycle.AwardTypeId)
                    .OrderBy(c => c.CriterionId)
                    .ToListAsync();
                    
                ViewData["Criteria"] = criteria;
            }
        }

        ViewData["CycleId"] = new SelectList(activeCycles, "CycleId", "AwardType.Name", cycleId);
        ViewBag.SelectedCycleId = cycleId;
        ViewBag.HideCycleSelect = activeCycles.Count == 1 || cycleId.HasValue;
        ViewData["CurrentEmployee"] = currentEmployee;
        
        return View();
    }

    // POST: SelfNomination/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB
    public async Task<IActionResult> Create([Bind("CycleId,Title,InitiativeDetails")] Nomination model, 
        IFormFile? attachmentFile, List<int> groupMemberIds)
    {
        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var currentEmployee = await _context.Employees.FindAsync(currentEmployeeId);
        
        if (currentEmployee == null)
        {
            return NotFound();
        }

        // Validate cycle is self-nomination type
        var cycle = await _context.AwardCycles
            .Include(ac => ac.AwardType)
            .FirstOrDefaultAsync(ac => ac.CycleId == model.CycleId && ac.AwardType.IsSelfNomination);
            
        if (cycle == null)
        {
            ModelState.AddModelError("CycleId", "دورة الترشيح غير صحيحة");
        }
        else
        {
            // Check if employee has won Employee of the Month and is trying to nominate for types 2 or 3
            if (cycle.AwardTypeId == 2 || cycle.AwardTypeId == 3)
            {
                var hasWonEmployeeOfMonth = await _context.Nominations
                    .Include(n => n.AwardCycle)
                    .AnyAsync(n => n.EmployeeId == currentEmployeeId && 
                                  n.IsWinner == 1 && 
                                  n.AwardCycle.AwardTypeId == 1);
                
                if (hasWonEmployeeOfMonth)
                {
                    ModelState.AddModelError("", "عذراً، لا يمكنك الترشح لهذه الجائزة لأنك فزت بجائزة موظف الشهر سابقاً");
                }
                
                // Check if employee already applied for the other award (2 or 3)
                var otherAwardTypeId = cycle.AwardTypeId == 2 ? 3 : 2;
                var otherAwardNomination = await _context.Nominations
                    .Include(n => n.AwardCycle)
                    .ThenInclude(ac => ac.AwardType)
                    .FirstOrDefaultAsync(n => n.EmployeeId == currentEmployeeId && 
                                  n.IsSelfNomination &&
                                  n.AwardCycle.Status == CycleStatus.Nomination &&
                                  n.AwardCycle.AwardTypeId == otherAwardTypeId);
                
                if (otherAwardNomination != null)
                {
                    var otherAwardName = otherAwardNomination.AwardCycle?.AwardType?.Name ?? "الجائزة الأخرى";
                    ModelState.AddModelError("", $"عذراً، لقد قمت بالترشح لجائزة {otherAwardName} ولا يمكنك الترشح لكلا الجائزتين في نفس الفترة");
                }
            }
        }

        // Check if already nominated
        var existingNomination = await _context.Nominations
            .FirstOrDefaultAsync(n => n.EmployeeId == currentEmployeeId && n.CycleId == model.CycleId && n.IsSelfNomination);
        
        if (existingNomination != null)
        {
            ModelState.AddModelError("", "لقد قمت بالترشح في هذه الدورة بالفعل");
        }

        // Validate group members (max 5 total including self)
        if (groupMemberIds != null && groupMemberIds.Count > 4)
        {
            ModelState.AddModelError("", "لا يمكن إضافة أكثر من 4 موظفين آخرين (5 موظفين إجمالي)");
        }

        // Validate group member employee IDs
        var validGroupMembers = new List<Employee>();
        if (groupMemberIds != null && groupMemberIds.Any())
        {
            validGroupMembers = await _context.Employees
                .Where(e => groupMemberIds.Contains(e.EmployeeId) && e.IsActive == 1)
                .ToListAsync();
                
            if (validGroupMembers.Count != groupMemberIds.Count)
            {
                ModelState.AddModelError("", "بعض أرقام الموظفين غير صحيحة أو غير نشطة");
            }
            
            // Check if any member is the current employee (trying to add themselves)
            if (groupMemberIds.Contains(currentEmployeeId))
            {
                ModelState.AddModelError("", "لا يمكنك إضافة نفسك كعضو في الفريق. أنت المرشح الرئيسي بالفعل");
            }

            // Check if any group member is already nominated in this cycle
            var alreadyNominatedIds = await _context.Nominations
                .Where(n => n.CycleId == model.CycleId && groupMemberIds.Contains(n.EmployeeId))
                .Select(n => n.EmployeeId)
                .ToListAsync();
                
            if (alreadyNominatedIds.Any())
            {
                var nominatedEmployees = await _context.Employees
                    .Where(e => alreadyNominatedIds.Contains(e.EmployeeId))
                    .ToListAsync();
                var names = string.Join(", ", nominatedEmployees.Select(e => $"{e.FirstName} {e.LastName}"));
                ModelState.AddModelError("", $"الموظفون التاليون مرشحون بالفعل في هذه الدورة: {names}");
            }
            
            // Check if any group member has won Employee of the Month for award types 2 or 3
            if (cycle != null && (cycle.AwardTypeId == 2 || cycle.AwardTypeId == 3))
            {
                var groupMemberWinners = await _context.Nominations
                    .Include(n => n.AwardCycle)
                    .Where(n => groupMemberIds.Contains(n.EmployeeId) && 
                               n.IsWinner == 1 && 
                               n.AwardCycle.AwardTypeId == 1)
                    .Select(n => n.EmployeeId)
                    .Distinct()
                    .ToListAsync();
                
                if (groupMemberWinners.Any())
                {
                    var winnerNames = validGroupMembers
                        .Where(e => groupMemberWinners.Contains(e.EmployeeId))
                        .Select(e => $"{e.FirstName} {e.LastName}")
                        .ToList();
                    
                    ModelState.AddModelError("", $"لا يمكن إضافة الموظفين التالين لأنهم فازوا بجائزة موظف الشهر سابقاً: {string.Join(", ", winnerNames)}");
                }
                
                // Check if group members are already involved in the other award (2 or 3)
                var otherAwardTypeId = cycle.AwardTypeId == 2 ? 3 : 2;
                
                // Check as main nominees
                var alreadyNominated = await _context.Nominations
                    .Include(n => n.AwardCycle)
                    .Where(n => groupMemberIds.Contains(n.EmployeeId) && 
                               n.IsSelfNomination &&
                               n.AwardCycle.Status == CycleStatus.Nomination &&
                               n.AwardCycle.AwardTypeId == otherAwardTypeId)
                    .Select(n => n.EmployeeId)
                    .ToListAsync();
                
                // Check as team members
                var alreadyTeamMembers = await _context.GroupNominationMembers
                    .Include(gm => gm.Nomination)
                    .ThenInclude(n => n.AwardCycle)
                    .Where(gm => groupMemberIds.Contains(gm.EmployeeId) &&
                                gm.Nomination.IsSelfNomination &&
                                gm.Nomination.AwardCycle.Status == CycleStatus.Nomination &&
                                gm.Nomination.AwardCycle.AwardTypeId == otherAwardTypeId)
                    .Select(gm => gm.EmployeeId)
                    .ToListAsync();
                
                var allInvolvedInOtherAward = alreadyNominated.Union(alreadyTeamMembers).Distinct().ToList();
                
                if (allInvolvedInOtherAward.Any())
                {
                    var involvedNames = validGroupMembers
                        .Where(e => allInvolvedInOtherAward.Contains(e.EmployeeId))
                        .Select(e => $"{e.FirstName} {e.LastName}")
                        .ToList();
                    
                    var otherAwardName = await _context.AwardTypes
                        .Where(at => at.AwardTypeId == otherAwardTypeId)
                        .Select(at => at.Name)
                        .FirstOrDefaultAsync() ?? "الجائزة الأخرى";
                    
                    ModelState.AddModelError("", $"لا يمكن إضافة الموظفين التالين لأنهم مشاركون بالفعل في جائزة {otherAwardName}: {string.Join(", ", involvedNames)}");
                }
            }
        }

        // Validate PDF file is required
        if (attachmentFile == null || attachmentFile.Length == 0)
        {
            ModelState.AddModelError("attachmentFile", "يجب إرفاق ملف PDF يحتوي على تفاصيل المبادرة أو الابتكار.");
        }

        // Handle file upload
        string? attachmentPath = null;
        if (attachmentFile != null && attachmentFile.Length > 0)
        {
            if (Path.GetExtension(attachmentFile.FileName).ToLower() != ".pdf")
            {
                ModelState.AddModelError("attachmentFile", "الرجاء رفع ملف بصيغة PDF فقط.");
            }
            else
            {
                try
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(attachmentFile.FileName)}";
                    var uploadsFolder = @"C:\EOM\uploads";
                    Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await attachmentFile.CopyToAsync(fileStream);
                    }
                    
                    attachmentPath = fileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("attachmentFile", $"خطأ في رفع الملف: {ex.Message}");
                }
            }
        }

        if (!ModelState.IsValid)
        {
            // Reload view data for error case
            var activeCycles = await _context.AwardCycles
                .Include(ac => ac.AwardType)
                .Where(ac => ac.Status == CycleStatus.Nomination && ac.AwardType.IsSelfNomination)
                .ToListAsync();

            ViewData["CycleId"] = new SelectList(activeCycles, "CycleId", "AwardType.Name", model.CycleId);
            ViewData["CurrentEmployee"] = currentEmployee;
            ViewData["GroupMembers"] = validGroupMembers;
            return View(model);
        }

        // Create the self-nomination
        var nomination = new Nomination
        {
            CycleId = model.CycleId,
            EmployeeId = currentEmployeeId,
            ManagerId = null, // No manager for self-nominations
            IsSelfNomination = true,
            Title = model.Title,
            InitiativeDetails = model.InitiativeDetails,
            AttachmentPath = attachmentPath,
            CreatedAt = DateTime.UtcNow
        };

        _context.Add(nomination);
        await _context.SaveChangesAsync();

        // Add group members if any
        if (validGroupMembers.Any())
        {
            foreach (var member in validGroupMembers)
            {
                _context.GroupNominationMembers.Add(new GroupNominationMember
                {
                    NominationId = nomination.NominationId,
                    EmployeeId = member.EmployeeId
                });
            }
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = $"تم إرسال ترشيحك بنجاح لجائزة {cycle?.AwardType.Name}";
        return RedirectToAction(nameof(Index));
    }

    // GET: SelfNomination/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var nomination = await _context.Nominations
            .Include(n => n.AwardCycle)
            .ThenInclude(ac => ac.AwardType)
            .Include(n => n.Employee)
            .Include(n => n.GroupMembers)
            .ThenInclude(gm => gm.Employee)
            .Include(n => n.Evaluations)
            .ThenInclude(e => e.CommitteeMember)
            .ThenInclude(cm => cm.Employee)
            .FirstOrDefaultAsync(n => n.NominationId == id && n.IsSelfNomination);
        
        if (nomination == null)
        {
            return NotFound();
        }

        // Check if current user has access to view this nomination
        // Either they are the main nominee or a team member
        bool hasAccess = nomination.EmployeeId == currentEmployeeId;
        
        if (!hasAccess)
        {
            // Check if they are a team member
            var isTeamMember = await _context.GroupNominationMembers
                .AnyAsync(gm => gm.NominationId == id && gm.EmployeeId == currentEmployeeId);
            
            hasAccess = isTeamMember;
        }
        
        if (!hasAccess)
        {
            return Forbid();
        }

        // Add flag to indicate if viewer is team member
        ViewBag.IsTeamMember = nomination.EmployeeId != currentEmployeeId;
        ViewBag.CurrentEmployeeId = currentEmployeeId;

        return View(nomination);
    }

    // POST: SelfNomination/SearchEmployee
    [HttpPost]
    [Route("SelfNomination/SearchEmployee")]
    public async Task<IActionResult> SearchEmployee([FromForm] int employeeNumber, [FromForm] int? cycleId)
    {
        try
        {
            // Get current user's employee ID
            var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Check if employee is trying to add themselves
            if (employeeNumber == currentEmployeeId)
            {
                return Json(new { success = false, message = "لا يمكنك إضافة نفسك كعضو في الفريق. أنت المرشح الرئيسي بالفعل" });
            }
            
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeNumber && e.IsActive == 1);
                
            if (employee == null)
            {
                return Json(new { success = false, message = "رقم الموظف غير صحيح أو غير نشط" });
            }

            // Check if cycleId is provided and validate against award type restrictions
            if (cycleId.HasValue)
            {
                var cycle = await _context.AwardCycles
                    .FirstOrDefaultAsync(c => c.CycleId == cycleId.Value);
                    
                // If this is award type 2 or 3, check if the employee has won award type 1
                if (cycle != null && (cycle.AwardTypeId == 2 || cycle.AwardTypeId == 3))
                {
                    var hasWonEmployeeOfMonth = await _context.Nominations
                        .Include(n => n.AwardCycle)
                        .Where(n => n.AwardCycle != null)
                        .AnyAsync(n => n.EmployeeId == employeeNumber && 
                                      n.IsWinner == 1 && 
                                      n.AwardCycle.AwardTypeId == 1);
                    
                    if (hasWonEmployeeOfMonth)
                    {
                        return Json(new { 
                            success = false, 
                            message = "لا يمكن إضافة هذا الموظف لأنه فاز بجائزة موظف الشهر سابقاً ولا يحق له الترشح لهذه الجائزة" 
                        });
                    }
                }
            }

            return Json(new { 
                success = true, 
                employee = new { 
                    EmployeeId = employee.EmployeeId,
                    EmployeeNumber = employee.EmployeeId, // Same value, for clarity
                    Name = $"{employee.FirstName?.Trim()} {employee.LastName?.Trim()}".Trim(),
                    Department = employee.Department?.Name ?? "غير محدد"
                }
            });
        }
        catch (Exception ex)
        {
            // Log the exception if you have logging configured
            return Json(new { success = false, message = "حدث خطأ أثناء البحث عن الموظف" });
        }
    }

    // GET: SelfNomination/DownloadAttachment/{id}
    public async Task<IActionResult> DownloadAttachment(int id)
    {
        var currentEmployeeId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Get the nomination
        var nomination = await _context.Nominations
            .Include(n => n.Employee)
            .FirstOrDefaultAsync(n => n.NominationId == id && n.IsSelfNomination);

        if (nomination == null || string.IsNullOrEmpty(nomination.AttachmentPath))
        {
            return NotFound();
        }

        // Check access: allow nominee, committee member, or admin
        bool hasAccess = false;
        
        // Check if current user is the nominee
        if (nomination.EmployeeId == currentEmployeeId)
        {
            hasAccess = true;
        }
        else if (User.IsInRole("EOM-Admin"))
        {
            // Admin always has access
            hasAccess = true;
        }
        else
        {
            // Check if current user is a committee member
            var isCommitteeMember = await _context.CommitteeMembers
                .AnyAsync(cm => cm.EmployeeId == currentEmployeeId && cm.IsActive == true);
            
            if (isCommitteeMember)
            {
                hasAccess = true;
            }
        }

        if (!hasAccess)
        {
            return Forbid();
        }

        var filePath = Path.Combine(@"C:\EOM\uploads", nomination.AttachmentPath);
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("الملف غير موجود");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        var fileName = $"{nomination.Title}_attachment.pdf";
        
        return File(fileBytes, "application/pdf", fileName);
    }
}