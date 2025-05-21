using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.AI.OpenAI.Chat;

namespace InfraAIChat
{
    public class OpenAIService
    {
        public static async Task<OpenAI.Chat.ChatCompletion> GetChatResponseAsync(ChatRequest data, ILogger _logger)
        {
            try
            {
                // Retrieve the OpenAI endpoint from environment variables
                var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
                if (string.IsNullOrEmpty(endpoint))
                {
                    _logger.LogError("AZURE_OPENAI_ENDPOINT environment variable is not set.");
                    throw new Exception("AZURE_OPENAI_ENDPOINT environment variable is not set.");
                }

                var key = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
                if (string.IsNullOrEmpty(key))
                {
                    _logger.LogError("AZURE_OPENAI_KEY environment variable is not set.");
                    throw new Exception("AZURE_OPENAI_KEY environment variable is not set.");
                }

                var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME");
                if (string.IsNullOrEmpty(deploymentName))
                {
                    _logger.LogError("Deployment name is not provided and AZURE_OPENAI_DEPLOYMENT environment variable is not set.");
                    throw new Exception("Deployment name is not provided and AZURE_OPENAI_DEPLOYMENT environment variable is not set.");
                }

                AzureKeyCredential credential = new AzureKeyCredential(key);

                // Initialize the AzureOpenAIClient
                AzureOpenAIClient azureClient = new(new Uri(endpoint), credential);

                // Initialize the ChatClient with the specified deployment name
                ChatClient chatClient = azureClient.GetChatClient(deploymentName);

                // Convert request messages to ChatMessage objects
                var chatMessages = new List<ChatMessage>();

                foreach (var message in data.Messages)
                {
                    switch (message.Role.ToLower())
                    {
                        case "system":
                            chatMessages.Add(new SystemChatMessage(message.Content));
                            break;
                        case "user":
                            chatMessages.Add(new UserChatMessage(message.Content));
                            break;
                        case "assistant":
                            chatMessages.Add(new AssistantChatMessage(message.Content));
                            break;
                        default:
                            chatMessages.Add(new UserChatMessage(message.Content));
                            break;
                    }
                }

                // Create chat completion options
                var options = new ChatCompletionOptions
                {
                    Temperature = data.Temperature ?? 0.7f,
                    MaxOutputTokenCount = data.MaxTokens ?? 800,
                    TopP = data.TopP ?? 0.95f,
                    FrequencyPenalty = data.FrequencyPenalty ?? 0f,
                    PresencePenalty = data.PresencePenalty ?? 0f
                };

                // Create the chat completion request
                var response = await chatClient.CompleteChatAsync(chatMessages, options);
                OpenAI.Chat.ChatCompletion completion = response.Value; // Extract the value from ClientResult

                // Return the response
                if (completion != null)
                {
                    return completion;
                }
                else
                {
                    _logger.LogWarning("No response received from OpenAI.");
                    throw new Exception("No response received from OpenAI.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                throw new Exception($"An error occurred: {ex.Message}");
            }
        }

        public static async Task<OpenAI.Chat.ChatCompletion> GetChatResponseWithAISearchAsync(ChatRequest data, ILogger _logger)
        {
            #pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            try
            {
                // Retrieve the OpenAI endpoint from environment variables
                var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
                if (string.IsNullOrEmpty(endpoint))
                {
                    _logger.LogError("AZURE_OPENAI_ENDPOINT environment variable is not set.");
                    throw new Exception("AZURE_OPENAI_ENDPOINT environment variable is not set.");
                }

                var key = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
                if (string.IsNullOrEmpty(key))
                {
                    _logger.LogError("AZURE_OPENAI_KEY environment variable is not set.");
                    throw new Exception("AZURE_OPENAI_KEY environment variable is not set.");
                }

                var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME");
                if (string.IsNullOrEmpty(deploymentName))
                {
                    _logger.LogError("Deployment name is not provided and AZURE_OPENAI_DEPLOYMENT environment variable is not set.");
                    throw new Exception("Deployment name is not provided and AZURE_OPENAI_DEPLOYMENT environment variable is not set.");
                }

                var searchEndpoint = Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT");
                if (string.IsNullOrEmpty(searchEndpoint))
                {
                    _logger.LogError("Search endpoint is not provided and AZURE_SEARCH_ENDPOINT environment variable is not set.");
                    throw new Exception("Search endpoint is not provided and AZURE_SEARCH_ENDPOINT environment variable is not set.");
                }

                var searchKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_KEY");
                if (string.IsNullOrEmpty(searchKey))
                {
                    _logger.LogError("Search key is not provided and AZURE_SEARCH_KEY environment variable is not set.");
                    throw new Exception("Search key is not provided and AZURE_SEARCH_KEY environment variable is not set.");
                }

                var searchIndex = Environment.GetEnvironmentVariable("AZURE_SEARCH_INDEX");
                if (string.IsNullOrEmpty(searchIndex))
                {
                    _logger.LogError("Search index is not provided and AZURE_SEARCH_INDEX environment variable is not set.");
                    throw new Exception("Search index is not provided and AZURE_SEARCH_INDEX environment variable is not set.");
                }

                AzureKeyCredential credential = new AzureKeyCredential(key);

                // Initialize the AzureOpenAIClient
                AzureOpenAIClient azureClient = new(new Uri(endpoint), credential);

                // Initialize the ChatClient with the specified deployment name
                ChatClient chatClient = azureClient.GetChatClient(deploymentName);


                // Convert request messages to ChatMessage objects
                var chatMessages = new List<ChatMessage>();

                foreach (var message in data.Messages)
                {
                    switch (message.Role.ToLower())
                    {
                        case "system":
                            chatMessages.Add(new SystemChatMessage(message.Content));
                            break;
                        case "user":
                            chatMessages.Add(new UserChatMessage(message.Content));
                            break;
                        case "assistant":
                            chatMessages.Add(new AssistantChatMessage(message.Content));
                            break;
                        default:
                            chatMessages.Add(new UserChatMessage(message.Content));
                            break;
                    }
                }

                // Create chat completion options
                var options = new ChatCompletionOptions
                {
                    Temperature = (float)0.7,
                    MaxOutputTokenCount = 800,

                    TopP = (float)0.95,
                    FrequencyPenalty = (float)0,
                    PresencePenalty = (float)0

                };

                
                options.AddDataSource(new AzureSearchChatDataSource()
                {
                    Endpoint = new Uri(searchEndpoint),
                    IndexName = searchIndex,
                    Authentication = DataSourceAuthentication.FromApiKey(searchKey), // Add your Azure AI Search admin key here
                });
                

                // Create the chat completion request
                var response = await chatClient.CompleteChatAsync(chatMessages, options);
                OpenAI.Chat.ChatCompletion completion = response.Value; // Extract the value from ClientResult

                //ChatCompletion test = chatClient.CompleteChat(chatMessages, new ChatCompletionOptions
                //{
                //    Temperature = (float)0.7,
                //    MaxOutputTokenCount = 800,

                //    TopP = (float)0.95,
                //    FrequencyPenalty = (float)0,
                //    PresencePenalty = (float)0
                //});

                //object value = test.GetMessageContext

                // Return the response
                if (completion != null)
                {
                    return completion;
                }
                else
                {
                    _logger.LogWarning("No response received from OpenAI.");
                    throw new Exception("No response received from OpenAI.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                throw new Exception($"An error occurred: {ex.Message}");
            }
            #pragma warning restore AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        }
    }

    public class ChatRequest
    {
        [JsonProperty("deployment_name")]
        public required string DeploymentName { get; set; }

        [JsonProperty("messages")]
        public required List<MessageRequest> Messages { get; set; }

        [JsonProperty("temperature")]
        public float? Temperature { get; set; }

        [JsonProperty("max_tokens")]
        public int? MaxTokens { get; set; }

        [JsonProperty("top_p")]
        public float? TopP { get; set; }

        [JsonProperty("frequency_penalty")]
        public float? FrequencyPenalty { get; set; }

        [JsonProperty("presence_penalty")]
        public float? PresencePenalty { get; set; }
    }

    public class MessageRequest
    {
        [JsonProperty("role")]
        public required string Role { get; set; }

        [JsonProperty("content")]
        public required string Content { get; set; }
    }
}


