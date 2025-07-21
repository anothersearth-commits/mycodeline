using EOM.Web.Data;
using EOM.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EOM.Web.Services
{
    /// <summary>
    /// Service for AI-powered message generation and management
    /// </summary>
    public class AiMessageService : IAiMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOpenAiService _openAiService;
        private readonly ILogger<AiMessageService> _logger;

        // System prompt template for AI generation
        private const string SystemPrompt = @"أنت مساعد رسمي لمؤسسة حكومية. اكتب فقرتين: الأولى ""رسالة"" ترحيبية تحفيزية رسمية ≤ 30 كلمة، تذكر اسم الموظف وهدفه (وقد تذكر التصنيف إن أضاف قيمة). الثانية ""نصيحة"" عملية قصيرة مرتبطة بالهدف أو الوصف. لا تذكر تواريخ أو مدد أو أرقام أيام. لا تكرر اسم المؤسسة إلا لو أضاف قيمة. استعمل فصحى واضحة، ولا تستخدم أكثر من إيموجي واحد في الرسالة فقط. افصل بين الفقرتين بسطر فارغ.";

        // Few-shot examples for consistent output
        private readonly List<ChatExample> _fewShotExamples = new()
        {
            new ChatExample
            {
                UserMessage = @"الاسم: أحمد بن محمد بن حمد العجمي
الهدف: إدارة وصيانة Active Directory
التصنيف: هدف يساهم في تحقيق المهام والاختصاصات الوظيفية
الوصف: إدارة حسابات المستخدمين، الصلاحيات، متابعة الـ GPO وReplication",
                AssistantMessage = @"💬 الرسالة:
مرحبًا أحمد العجمي، التحكم في بيئة Active Directory لا يظهر على السطح، لكنه يشكّل العمود الفقري لكل صلاحية وانسيابية داخل المؤسسة. دورك محوري… وثقتنا بك راسخة. 🛡️

💡 النصيحة:
احرص على مراجعة الصلاحيات القديمة دوريًا، فالحسابات المهملة أو المتروكة بصلاحيات واسعة هي الثغرات التي لا تُرى إلا عند حدوث الخلل."
            },
            new ChatExample
            {
                UserMessage = @"الاسم: عبد الله بن جمعة بن سيف الهنداسي
الهدف: تفعيل التحول الرقمي
التصنيف: هدف يساهم في تحقيق الخطة السنوية
الوصف: تطوير وتحسين برنامج إدارة الوثائق والمراسلات لزيادة كفاءة التحول الرقمي",
                AssistantMessage = @"💬 الرسالة:
مرحبًا عبد الله الهنداسي، تحسين إدارة الوثائق والمراسلات خطوة جوهرية لتسريع التحول الرقمي وتمتين الحوكمة.

💡 النصيحة:
حدّد أكثر نموذج يُستخدم تكرارًا، وابدأ بأتمتة حقوله القابلة للتعبئة المسبقة لتقليل الأخطاء اليدوية."
            }
        };

        public AiMessageService(ApplicationDbContext context, IOpenAiService openAiService, ILogger<AiMessageService> logger)
        {
            _context = context;
            _openAiService = openAiService;
            _logger = logger;
        }

        public async Task<AiGeneratedMessage> GenerateMessageAsync(long employeeId, long objectiveId)
        {
            try
            {
                var objective = await _context.Objectives
                    .Include(o => o.ObjectiveCycle)
                    .FirstOrDefaultAsync(o => o.ObjectiveId == objectiveId && o.EmployeeId == employeeId);

                if (objective == null)
                {
                    throw new ArgumentException($"Objective {objectiveId} not found for employee {employeeId}");
                }

                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
                if (employee == null)
                {
                    throw new ArgumentException($"Employee {employeeId} not found");
                }

                // Deactivate existing messages for this objective
                await DeactivateExistingMessagesAsync(objectiveId);

                // Generate user prompt
                var userPrompt = BuildUserPrompt(employee, objective);

                // Generate AI content
                var aiResponse = await _openAiService.GenerateContentWithExamplesAsync(
                    SystemPrompt,
                    _fewShotExamples,
                    userPrompt,
                    temperature: 0.7,
                    maxTokens: 180
                );

                if (!aiResponse.IsSuccess)
                {
                    throw new Exception($"AI generation failed: {aiResponse.ErrorMessage}");
                }

                // Parse and validate response
                var (messageBody, adviceBody) = ParseAiResponse(aiResponse.Content);
                var validationResult = await ValidateMessageAsync(messageBody, adviceBody);

                // If validation fails, try regenerating once
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Initial AI generation failed validation, attempting regeneration for objective {ObjectiveId}", objectiveId);
                    
                    var regeneratedResponse = await _openAiService.GenerateContentWithExamplesAsync(
                        SystemPrompt + "\n\nتنبيه: تأكد من عدم تجاوز 30 كلمة في الرسالة وعدم ذكر التواريخ.",
                        _fewShotExamples,
                        userPrompt,
                        temperature: 0.5,
                        maxTokens: 180
                    );

                    if (regeneratedResponse.IsSuccess)
                    {
                        (messageBody, adviceBody) = ParseAiResponse(regeneratedResponse.Content);
                        validationResult = await ValidateMessageAsync(messageBody, adviceBody);
                        aiResponse = regeneratedResponse;
                    }
                }

                // Create and save the message
                var aiMessage = new AiGeneratedMessage
                {
                    ObjectiveId = objectiveId,
                    EmployeeId = employeeId,
                    ObjectiveCycleId = objective.ObjectiveCycleId,
                    MessageBody = messageBody,
                    AdviceBody = adviceBody,
                    StyleTag = "Formal",
                    ModelName = aiResponse.Model,
                    IsActive = true
                };

                _context.AiGeneratedMessages.Add(aiMessage);
                await _context.SaveChangesAsync();

                _logger.LogInformation("AI message generated successfully for objective {ObjectiveId}, employee {EmployeeId}", objectiveId, employeeId);

                return aiMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI message for objective {ObjectiveId}, employee {EmployeeId}", objectiveId, employeeId);
                throw;
            }
        }

        public async Task<List<AiGeneratedMessage>> GenerateMessagesForCycleAsync(long employeeId, int cycleId)
        {
            try
            {
                var objectives = await _context.Objectives
                    .Include(o => o.ObjectiveCycle)
                    .Where(o => o.EmployeeId == employeeId && o.ObjectiveCycleId == cycleId)
                    .ToListAsync();

                var messages = new List<AiGeneratedMessage>();

                foreach (var objective in objectives)
                {
                    try
                    {
                        var message = await GenerateMessageAsync(employeeId, objective.ObjectiveId);
                        messages.Add(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to generate message for objective {ObjectiveId}", objective.ObjectiveId);
                        // Continue with other objectives even if one fails
                    }
                }

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating messages for cycle {CycleId}, employee {EmployeeId}", cycleId, employeeId);
                throw;
            }
        }

        public async Task<List<AiGeneratedMessage>> GetActiveMessagesAsync(long employeeId, int cycleId)
        {
            return await _context.AiGeneratedMessages
                .Include(am => am.Objective)
                .Where(am => am.EmployeeId == employeeId && am.ObjectiveCycleId == cycleId && am.IsActive)
                .OrderBy(am => am.Objective.ObjectiveTitle)
                .ToListAsync();
        }

        public async Task<AiGeneratedMessage> RegenerateMessageAsync(long objectiveId)
        {
            var objective = await _context.Objectives
                .Include(o => o.ObjectiveCycle)
                .FirstOrDefaultAsync(o => o.ObjectiveId == objectiveId);

            if (objective == null)
            {
                throw new ArgumentException($"Objective {objectiveId} not found");
            }

            return await GenerateMessageAsync(objective.EmployeeId, objectiveId);
        }

        public async Task<AiMessageValidationResult> ValidateMessageAsync(string messageBody, string adviceBody)
        {
            var result = new AiMessageValidationResult();

            // Count words in message
            result.MessageWordCount = messageBody.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            // Count emojis in message
            result.EmojiCount = messageBody.Count(c => char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherSymbol);

            // Check for date references
            var content = $"{messageBody} {adviceBody}".ToLower();
            var datePatterns = new[] { "يوم", "أسبوع", "شهر", "سنة", "متبق", "باق", "2024", "2025", "2026" };
            result.ContainsDateReferences = datePatterns.Any(pattern => content.Contains(pattern));

            // Check for institution name (configurable)
            var institutionNames = new[] { "المؤسسة", "الوزارة", "الهيئة", "المحافظة" };
            result.ContainsInstitutionName = institutionNames.Any(name => content.Contains(name));

            // Validate message length
            if (result.MessageWordCount > 35)
            {
                result.ValidationErrors.Add($"الرسالة طويلة جداً ({result.MessageWordCount} كلمة). يجب أن تكون أقل من 30 كلمة.");
            }

            // Validate emoji count
            if (result.EmojiCount > 1)
            {
                result.ValidationErrors.Add($"يوجد أكثر من إيموجي واحد في الرسالة ({result.EmojiCount}). يُسمح بإيموجي واحد فقط.");
            }

            // Validate date references
            if (result.ContainsDateReferences)
            {
                result.ValidationErrors.Add("الرسالة تحتوي على مراجع تاريخية. يجب تجنب ذكر التواريخ والمدد.");
            }

            // Check for empty content
            if (string.IsNullOrWhiteSpace(messageBody))
            {
                result.ValidationErrors.Add("الرسالة فارغة.");
            }

            if (string.IsNullOrWhiteSpace(adviceBody))
            {
                result.ValidationErrors.Add("النصيحة فارغة.");
            }

            result.IsValid = result.ValidationErrors.Count == 0;

            return result;
        }

        public async Task<AiMessageStatistics> GetMessageStatisticsAsync(int cycleId)
        {
            var stats = new AiMessageStatistics();

            var objectives = await _context.Objectives
                .Where(o => o.ObjectiveCycleId == cycleId)
                .CountAsync();

            var messages = await _context.AiGeneratedMessages
                .Where(am => am.ObjectiveCycleId == cycleId)
                .ToListAsync();

            stats.TotalObjectives = objectives;
            stats.GeneratedMessages = messages.Count(m => m.IsActive);
            stats.ValidMessages = messages.Count(m => m.IsActive && m.IsValidFormat());
            stats.RegeneratedMessages = messages.Count() - messages.Count(m => m.IsActive);

            if (messages.Any())
            {
                stats.LastGenerated = messages.Max(m => m.GeneratedAt);
            }

            stats.MessagesByStyle = messages
                .Where(m => m.IsActive && !string.IsNullOrEmpty(m.StyleTag))
                .GroupBy(m => m.StyleTag)
                .ToDictionary(g => g.Key, g => g.Count());

            stats.MessagesByModel = messages
                .Where(m => m.IsActive && !string.IsNullOrEmpty(m.ModelName))
                .GroupBy(m => m.ModelName)
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }

        private string BuildUserPrompt(Employee employee, Objective objective)
        {
            var fullName = $"{employee.FirstName} {employee.LastName}";
            var promptBuilder = new StringBuilder();

            promptBuilder.AppendLine($"الاسم: {fullName}");
            
            // Include the high-level goal for context
            if (!string.IsNullOrEmpty(objective.HighLevelGoal))
            {
                promptBuilder.AppendLine($"الهدف الرئيسي: {objective.HighLevelGoal}");
            }
            
            promptBuilder.AppendLine($"الهدف الفرعي: {objective.ObjectiveTitle}");

            if (!string.IsNullOrEmpty(objective.Classification))
            {
                promptBuilder.AppendLine($"التصنيف: {objective.Classification}");
            }

            if (!string.IsNullOrEmpty(objective.ResultDescription))
            {
                var description = objective.ResultDescription.Length > 200
                    ? objective.ResultDescription[..200] + "..."
                    : objective.ResultDescription;
                promptBuilder.AppendLine($"الوصف: {description}");
            }

            return promptBuilder.ToString();
        }

        private (string messageBody, string adviceBody) ParseAiResponse(string aiResponse)
        {
            var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            var messageLines = new List<string>();
            var adviceLines = new List<string>();
            var isAdviceSection = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (trimmedLine.StartsWith("💡") || trimmedLine.Contains("النصيحة"))
                {
                    isAdviceSection = true;
                    continue;
                }
                
                if (trimmedLine.StartsWith("💬") || trimmedLine.Contains("الرسالة"))
                {
                    isAdviceSection = false;
                    continue;
                }

                if (!string.IsNullOrEmpty(trimmedLine))
                {
                    if (isAdviceSection)
                    {
                        adviceLines.Add(trimmedLine);
                    }
                    else
                    {
                        messageLines.Add(trimmedLine);
                    }
                }
            }

            var messageBody = string.Join(" ", messageLines);
            var adviceBody = string.Join(" ", adviceLines);

            // Fallback: split on first empty line if parsing fails
            if (string.IsNullOrEmpty(messageBody) || string.IsNullOrEmpty(adviceBody))
            {
                var parts = aiResponse.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    messageBody = parts[0].Trim();
                    adviceBody = parts[1].Trim();
                }
            }

            return (messageBody, adviceBody);
        }

        private async Task DeactivateExistingMessagesAsync(long objectiveId)
        {
            var existingMessages = await _context.AiGeneratedMessages
                .Where(am => am.ObjectiveId == objectiveId && am.IsActive)
                .ToListAsync();

            foreach (var message in existingMessages)
            {
                message.IsActive = false;
            }

            if (existingMessages.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}