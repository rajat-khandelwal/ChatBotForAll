using Azure;
using Azure.AI.OpenAI;
using ChatBotForAll.ApiService.Interfaces;

namespace ChatBotForAll.ApiService.Services
{
    public class AzureOpenAIEmbeddingService : IEmbeddingService
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _embeddingDeploymentName;
        private readonly ILogger<AzureOpenAIEmbeddingService> _logger;

        public AzureOpenAIEmbeddingService(IConfiguration configuration, ILogger<AzureOpenAIEmbeddingService> logger)
        {
            _logger = logger;

            var endpoint = configuration["AzureOpenAI:Endpoint"];
            var apiKey = configuration["AzureOpenAI:ApiKey"];
            _embeddingDeploymentName = configuration["AzureOpenAI:EmbeddingDeploymentName"] ?? "text-embedding-3-small";

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Azure OpenAI configuration is missing. Please set AzureOpenAI:Endpoint and AzureOpenAI:ApiKey in appsettings.json");
            }

            _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning("Empty text provided for embedding");
                    return Array.Empty<float>();
                }

                var embeddingClient = _client.GetEmbeddingClient(_embeddingDeploymentName);
                var response = await embeddingClient.GenerateEmbeddingAsync(text);

                return response.Value.ToFloats().ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating embedding: {ex.Message}");
                throw;
            }
        }
    }
}
