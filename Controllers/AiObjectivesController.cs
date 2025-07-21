using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EOM.Web.Services;
using EOM.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EOM.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AiObjectivesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiMessageService _aiMessageService;
        private readonly ILogger<AiObjectivesController> _logger;

        public AiObjectivesController(
            ApplicationDbContext context,
            IAiMessageService aiMessageService,
            ILogger<AiObjectivesController> logger)
        {
            _context = context;
            _aiMessageService = aiMessageService;
            _logger = logger;
        }

        /// <summary>
        /// Generate AI message for current user's random objective
        /// Called on login via AJAX
        /// </summary>
        [HttpPost("generate-daily-message")]
        public async Task<IActionResult> GenerateDailyMessage()
        {
            try
            {
                var employeeId = GetCurrentEmployeeId();
                if (employeeId == null)
                {
                    return Unauthorized(new { message = "Unable to identify employee" });
                }

                // Get active cycle - handle Oracle casting issue
                var activeCycle = await _context.ObjectiveCycles
                    .Where(oc => oc.IsActive == true)
                    .FirstOrDefaultAsync();

                if (activeCycle == null)
                {
                    return BadRequest(new { message = "No active objective cycle found" });
                }

                // Get random objective for the employee that doesn't have a recent AI message
                var randomObjective = await _context.Objectives
                    .Where(o => o.EmployeeId == employeeId.Value && 
                               o.ObjectiveCycleId == activeCycle.ObjectiveCycleId)
                    .Where(o => !o.AiGeneratedMessages.Any(am => am.IsActive && 
                               am.GeneratedAt.Date == DateTime.Today))
                    .OrderBy(r => Guid.NewGuid())
                    .FirstOrDefaultAsync();

                if (randomObjective == null)
                {
                    // All objectives have messages for today, get the most recent one
                    var recentMessage = await _context.AiGeneratedMessages
                        .Include(am => am.Objective)
                        .Where(am => am.EmployeeId == employeeId.Value && 
                                   am.ObjectiveCycleId == activeCycle.ObjectiveCycleId &&
                                   am.IsActive)
                        .OrderByDescending(am => am.GeneratedAt)
                        .FirstOrDefaultAsync();

                    if (recentMessage != null)
                    {
                        return Ok(new
                        {
                            message = recentMessage.MessageBody,
                            advice = recentMessage.AdviceBody,
                            objectiveTitle = recentMessage.Objective.ObjectiveTitle,
                            mainGoal = recentMessage.Objective.HighLevelGoal,
                            generated = recentMessage.GeneratedAt,
                            isNew = false
                        });
                    }

                    return BadRequest(new { message = "No objectives found for employee" });
                }

                // Generate AI message
                var aiMessage = await _aiMessageService.GenerateMessageAsync(employeeId.Value, randomObjective.ObjectiveId);

                return Ok(new
                {
                    message = aiMessage.MessageBody,
                    advice = aiMessage.AdviceBody,
                    objectiveTitle = randomObjective.ObjectiveTitle,
                    mainGoal = randomObjective.HighLevelGoal,
                    generated = aiMessage.GeneratedAt,
                    isNew = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily AI message for employee");
                return StatusCode(500, new { 
                    message = "خطأ في توليد الرسالة. يرجى المحاولة مرة أخرى.",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Get all AI messages for current user
        /// </summary>
        [HttpGet("my-messages")]
        public async Task<IActionResult> GetMyMessages()
        {
            try
            {
                var employeeId = GetCurrentEmployeeId();
                if (employeeId == null)
                {
                    return Unauthorized(new { message = "Unable to identify employee" });
                }

                var activeCycle = await _context.ObjectiveCycles
                    .Where(oc => oc.IsActive == true)
                    .FirstOrDefaultAsync();

                if (activeCycle == null)
                {
                    return BadRequest(new { message = "No active objective cycle found" });
                }

                var messages = await _aiMessageService.GetActiveMessagesAsync(employeeId.Value, activeCycle.ObjectiveCycleId);

                var result = messages.Select(m => new
                {
                    id = m.AiMessageId,
                    message = m.MessageBody,
                    advice = m.AdviceBody,
                    objectiveTitle = m.Objective.ObjectiveTitle,
                    mainGoal = m.Objective.HighLevelGoal,
                    generated = m.GeneratedAt,
                    style = m.StyleTag
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI messages for employee");
                return StatusCode(500, new { message = "خطأ في جلب الرسائل" });
            }
        }

        /// <summary>
        /// Regenerate AI message for specific objective
        /// </summary>
        [HttpPost("regenerate/{objectiveId}")]
        public async Task<IActionResult> RegenerateMessage(long objectiveId)
        {
            try
            {
                var employeeId = GetCurrentEmployeeId();
                if (employeeId == null)
                {
                    return Unauthorized(new { message = "Unable to identify employee" });
                }

                // Verify objective belongs to current employee
                var objective = await _context.Objectives
                    .FirstOrDefaultAsync(o => o.ObjectiveId == objectiveId && o.EmployeeId == employeeId.Value);

                if (objective == null)
                {
                    return NotFound(new { message = "Objective not found or access denied" });
                }

                var aiMessage = await _aiMessageService.RegenerateMessageAsync(objectiveId);

                return Ok(new
                {
                    message = aiMessage.MessageBody,
                    advice = aiMessage.AdviceBody,
                    objectiveTitle = objective.ObjectiveTitle,
                    mainGoal = objective.HighLevelGoal,
                    generated = aiMessage.GeneratedAt,
                    isNew = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating AI message for objective {ObjectiveId}", objectiveId);
                return StatusCode(500, new { message = "خطأ في إعادة توليد الرسالة" });
            }
        }

        /// <summary>
        /// Get statistics about AI messages
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var employeeId = GetCurrentEmployeeId();
                if (employeeId == null)
                {
                    return Unauthorized(new { message = "Unable to identify employee" });
                }

                var activeCycle = await _context.ObjectiveCycles
                    .Where(oc => oc.IsActive == true)
                    .FirstOrDefaultAsync();

                if (activeCycle == null)
                {
                    return BadRequest(new { message = "No active objective cycle found" });
                }

                var stats = await _aiMessageService.GetMessageStatisticsAsync(activeCycle.ObjectiveCycleId);

                return Ok(new
                {
                    totalObjectives = stats.TotalObjectives,
                    generatedMessages = stats.GeneratedMessages,
                    validMessages = stats.ValidMessages,
                    lastGenerated = stats.LastGenerated,
                    messagesByStyle = stats.MessagesByStyle
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI message statistics");
                return StatusCode(500, new { message = "خطأ في جلب الإحصائيات" });
            }
        }

        private long? GetCurrentEmployeeId()
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (long.TryParse(employeeIdClaim, out long employeeId))
            {
                return employeeId;
            }

            // Fallback: try to get from user identity if stored differently
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out long userId))
            {
                return userId;
            }

            return null;
        }
    }
}