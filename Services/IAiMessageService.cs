using EOM.Web.Models;

namespace EOM.Web.Services
{
    /// <summary>
    /// Interface for AI-powered message generation service
    /// </summary>
    public interface IAiMessageService
    {
        /// <summary>
        /// Generate AI message and advice for a specific employee objective
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="objectiveId">Objective ID</param>
        /// <returns>Generated message and advice</returns>
        Task<AiGeneratedMessage> GenerateMessageAsync(int employeeId, long objectiveId);

        /// <summary>
        /// Generate AI messages for all objectives in a cycle for a specific employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="cycleId">Objective cycle ID</param>
        /// <returns>List of generated messages</returns>
        Task<List<AiGeneratedMessage>> GenerateMessagesForCycleAsync(int employeeId, int cycleId);

        /// <summary>
        /// Get active AI messages for an employee in a specific cycle
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="cycleId">Objective cycle ID</param>
        /// <returns>List of active messages</returns>
        Task<List<AiGeneratedMessage>> GetActiveMessagesAsync(int employeeId, int cycleId);

        /// <summary>
        /// Regenerate AI message for a specific objective
        /// </summary>
        /// <param name="objectiveId">Objective ID</param>
        /// <returns>New generated message</returns>
        Task<AiGeneratedMessage> RegenerateMessageAsync(long objectiveId);

        /// <summary>
        /// Validate AI message content according to specifications
        /// </summary>
        /// <param name="messageBody">Message content</param>
        /// <param name="adviceBody">Advice content</param>
        /// <returns>Validation result</returns>
        Task<AiMessageValidationResult> ValidateMessageAsync(string messageBody, string adviceBody);

        /// <summary>
        /// Get AI message statistics for a cycle
        /// </summary>
        /// <param name="cycleId">Objective cycle ID</param>
        /// <returns>Statistics about AI messages</returns>
        Task<AiMessageStatistics> GetMessageStatisticsAsync(int cycleId);
    }

    /// <summary>
    /// Result of AI message validation
    /// </summary>
    public class AiMessageValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public int MessageWordCount { get; set; }
        public int EmojiCount { get; set; }
        public bool ContainsDateReferences { get; set; }
        public bool ContainsInstitutionName { get; set; }
    }

    /// <summary>
    /// Statistics about AI messages in a cycle
    /// </summary>
    public class AiMessageStatistics
    {
        public int TotalObjectives { get; set; }
        public int GeneratedMessages { get; set; }
        public int ValidMessages { get; set; }
        public int RegeneratedMessages { get; set; }
        public DateTime LastGenerated { get; set; }
        public Dictionary<string, int> MessagesByStyle { get; set; } = new();
        public Dictionary<string, int> MessagesByModel { get; set; } = new();
    }
}