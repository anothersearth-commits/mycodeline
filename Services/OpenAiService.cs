using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI;
using OpenAI.Chat;

namespace EOM.Web.Services
{
    /// <summary>
    /// Service for OpenAI API integration
    /// </summary>
    public class OpenAiService : IOpenAiService
    {
        private readonly OpenAIClient _openAiClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAiService> _logger;
        private readonly string _model;

        public OpenAiService(IConfiguration configuration, ILogger<OpenAiService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            var apiKey = _configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
            _model = _configuration["OpenAI:Model"] ?? "gpt-4o";
            
            _openAiClient = new OpenAIClient(apiKey);
            _logger.LogInformation("OpenAI client initialized with model: {Model}", _model);
        }


        public async Task<OpenAiResponse> GenerateContentAsync(string systemPrompt, string userPrompt, double temperature = 0.7, int maxTokens = 180)
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                };

                var chatCompletionOptions = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = maxTokens,
                    Temperature = (float)temperature
                };

                _logger.LogInformation("Sending request to OpenAI API with model: {Model}", _model);

                var response = await _openAiClient.GetChatClient(_model).CompleteChatAsync(messages, chatCompletionOptions);

                var content = response.Value.Content[0].Text;
                var usage = response.Value.Usage;

                _logger.LogInformation("OpenAI API response received successfully. Tokens used: {TokensUsed}", usage.TotalTokenCount);

                return new OpenAiResponse
                {
                    IsSuccess = true,
                    Content = content,
                    Model = _model,
                    TokensUsed = usage.TotalTokenCount,
                    ResponseTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OpenAI content");
                return new OpenAiResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<OpenAiResponse> GenerateContentWithExamplesAsync(string systemPrompt, List<ChatExample> examples, string userPrompt, double temperature = 0.7, int maxTokens = 180)
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt)
                };

                // Add few-shot examples
                foreach (var example in examples)
                {
                    messages.Add(new UserChatMessage(example.UserMessage));
                    messages.Add(new AssistantChatMessage(example.AssistantMessage));
                }

                // Add the actual user prompt
                messages.Add(new UserChatMessage(userPrompt));

                var chatCompletionOptions = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = maxTokens,
                    Temperature = (float)temperature
                };

                _logger.LogInformation("Sending request to OpenAI API with model: {Model} and {ExampleCount} examples", _model, examples.Count);

                var response = await _openAiClient.GetChatClient(_model).CompleteChatAsync(messages, chatCompletionOptions);

                var content = response.Value.Content[0].Text;
                var usage = response.Value.Usage;

                _logger.LogInformation("OpenAI API response received successfully. Tokens used: {TokensUsed}", usage.TotalTokenCount);

                return new OpenAiResponse
                {
                    IsSuccess = true,
                    Content = content,
                    Model = _model,
                    TokensUsed = usage.TotalTokenCount,
                    ResponseTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OpenAI content with examples");
                return new OpenAiResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var testResponse = await GenerateContentAsync(
                    "You are a test assistant. Respond with 'OK' only.",
                    "Test connection",
                    0.1,
                    5
                );

                return testResponse.IsSuccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing OpenAI connection");
                return false;
            }
        }

        public async Task<List<string>> GetAvailableModelsAsync()
        {
            try
            {
                // For now, return a static list of common models
                // The OpenAI library doesn't have a direct models endpoint method
                return new List<string> 
                { 
                    "gpt-4o",
                    "gpt-4o-mini",
                    "gpt-4",
                    "gpt-3.5-turbo"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available OpenAI models");
                return new List<string>();
            }
        }

    }
}