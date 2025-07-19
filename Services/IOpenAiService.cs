namespace EOM.Web.Services
{
    /// <summary>
    /// Interface for OpenAI API integration
    /// </summary>
    public interface IOpenAiService
    {
        /// <summary>
        /// Generate AI content using OpenAI Chat API
        /// </summary>
        /// <param name="systemPrompt">System message for AI guidance</param>
        /// <param name="userPrompt">User message with objective data</param>
        /// <param name="temperature">Temperature for randomness (0.0 to 1.0)</param>
        /// <param name="maxTokens">Maximum tokens in response</param>
        /// <returns>Generated AI response</returns>
        Task<OpenAiResponse> GenerateContentAsync(string systemPrompt, string userPrompt, double temperature = 0.7, int maxTokens = 180);

        /// <summary>
        /// Generate AI content with few-shot examples
        /// </summary>
        /// <param name="systemPrompt">System message for AI guidance</param>
        /// <param name="examples">Few-shot examples for consistent output</param>
        /// <param name="userPrompt">User message with objective data</param>
        /// <param name="temperature">Temperature for randomness</param>
        /// <param name="maxTokens">Maximum tokens in response</param>
        /// <returns>Generated AI response</returns>
        Task<OpenAiResponse> GenerateContentWithExamplesAsync(string systemPrompt, List<ChatExample> examples, string userPrompt, double temperature = 0.7, int maxTokens = 180);

        /// <summary>
        /// Test OpenAI API connection and authentication
        /// </summary>
        /// <returns>Connection test result</returns>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Get available OpenAI models
        /// </summary>
        /// <returns>List of available models</returns>
        Task<List<string>> GetAvailableModelsAsync();
    }

    /// <summary>
    /// OpenAI API response
    /// </summary>
    public class OpenAiResponse
    {
        public bool IsSuccess { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int TokensUsed { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime ResponseTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Chat example for few-shot learning
    /// </summary>
    public class ChatExample
    {
        public string UserMessage { get; set; } = string.Empty;
        public string AssistantMessage { get; set; } = string.Empty;
    }
}