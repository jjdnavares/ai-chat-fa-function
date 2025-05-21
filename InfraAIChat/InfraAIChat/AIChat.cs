using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenAIService = InfraAIChat.OpenAIService;

namespace InfraAIChat;

public class AIChat
{
    private readonly ILogger<AIChat> _logger;

    public AIChat(ILogger<AIChat> logger)
    {
        _logger = logger;
    }

    [Function("FuncTest")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }

    [Function("ChatCompletion")]
    public async Task<IActionResult> ChatCompletion(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a chat completion request.");

        try
        {
            // Read request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ChatRequest? data = JsonConvert.DeserializeObject<ChatRequest>(requestBody);

            // Validate required parameters
            if (data == null || data.Messages == null || data.Messages.Count == 0)
            {
                return new BadRequestObjectResult("Please provide messages in the request body.");
            }


            OpenAIService openAIService = new OpenAIService();
            OpenAI.Chat.ChatCompletion completionResponse = await OpenAIService.GetChatResponseAsync(data, _logger);

            return new OkObjectResult(completionResponse);

        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [Function("AISearchQuery")]
    public async Task<IActionResult> AISearchQuery(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed an AI search query request.");

        try
        {
            // Read request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string query = requestBody.Trim('"'); // Remove quotes from the query string

            // Validate required parameters
            if (string.IsNullOrEmpty(query))
            {
                return new BadRequestObjectResult("Please provide a query in the request body.");
            }

            AISearchService aiSearchService = new AISearchService();
            SearchResults searchResults = await aiSearchService.QueryAzureAISearchAsync(query, _logger);

            return new OkObjectResult(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    [Function("ChatCompletionWithAISearch")]
    public async Task<IActionResult> ChatCompletionWithAISearch(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a chat completion request.");

        try
        {
            // Read request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ChatRequest? data = JsonConvert.DeserializeObject<ChatRequest>(requestBody);

            // Validate required parameters
            if (data == null || data.Messages == null || data.Messages.Count == 0)
            {
                return new BadRequestObjectResult("Please provide messages in the request body.");
            }


            OpenAIService openAIService = new OpenAIService();
            OpenAI.Chat.ChatCompletion completionResponse = await OpenAIService.GetChatResponseWithAISearchAsync(data, _logger);

            return new OkObjectResult(completionResponse);

        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

}

