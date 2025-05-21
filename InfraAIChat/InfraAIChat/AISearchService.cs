using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;

namespace InfraAIChat
{
    public class AISearchService
    {
        public async Task<SearchResults> QueryAzureAISearchAsync(string query, ILogger _logger)
        {
            try
            {
                // Retrieve the Azure Cognitive Search endpoint and key from environment variables
                var endpoint = Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT");
                var key = Environment.GetEnvironmentVariable("AZURE_SEARCH_KEY");
                var indexName = Environment.GetEnvironmentVariable("AZURE_SEARCH_INDEX");

                if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
                {
                    throw new Exception("Azure Cognitive Search endpoint or key is not set.");
                }

                // Create a new instance of the SearchClient
                var searchClient = new SearchClient(new Uri(endpoint), indexName, new AzureKeyCredential(key));


                // Create a new search options instance
                var searchOptions = new SearchOptions
                {
                    IncludeTotalCount = true,
                    Size = 3,
                    QueryType = SearchQueryType.Simple  // Use simple query type for text queries
                };

                // Query the search index
                var response = await searchClient.SearchAsync<SearchDocument>(query, searchOptions);

                var searchResults = new SearchResults
                {
                    Value = response.Value.GetResults().Select(r => new SearchResult
                    {
                        Title = r.Document["title"].ToString(),
                        Text = r.Document["chunk"].ToString()
                    }).ToArray()
                };

                return searchResults;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while querying Azure Cognitive Search: {ex.Message}");
                throw;
            }
        }
    }

    public class SearchResults
    {
        public SearchResult[]? Value { get; set; }
    }

    public class SearchResult
    {
        public string? Title { get; set; }
        public string? Text { get; set; }
    }

}
