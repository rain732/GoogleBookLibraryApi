using GoogleBookLibraryApi.Models;
using System.Text.Json;

namespace GoogleBookLibraryApi.Services;

public class GoogleBooksService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GoogleBooksService> _logger;

    public GoogleBooksService(HttpClient httpClient, IConfiguration config, ILogger<GoogleBooksService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _apiKey = config["x-api-key"];
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogError("Google API key not found in configuration.");
            throw new InvalidOperationException("Google API key is missing from configuration.");
        }
    }

    public async Task<(bool Success, string? ErrorMessage, List<BookModel>? Books)> SearchBooksAsync(string query)
    {
        try
        {
            var url = $"https://www.googleapis.com/books/v1/volumes?q={query}&key={_apiKey}";
            var result = await _httpClient.GetFromJsonAsync<JsonElement>(url);

            if (!result.TryGetProperty("items", out var items))
                return (false, "No books found for this query.", null);

            var books = new List<BookModel>();
            foreach (var item in items.EnumerateArray())
            {
                var volume = item.GetProperty("volumeInfo");
                books.Add(new BookModel
                {
                    Title = volume.GetProperty("title").GetString() ?? string.Empty,
                    Authors = volume.TryGetProperty("authors", out var authors)
                        ? authors.EnumerateArray().Select(a => a.GetString()).ToArray()
                        : Array.Empty<string>(),
                    Publisher = volume.TryGetProperty("publisher", out var publisher) ? publisher.GetString() ?? string.Empty : string.Empty,
                    PreviewLink = volume.TryGetProperty("previewLink", out var link) ? link.GetString() ?? string.Empty : string.Empty,
                    Description = volume.TryGetProperty("description", out var desc) ? desc.GetString() ?? string.Empty
                        : string.Empty
                });
            }

            return (true, null, books);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Network error while calling Google Books API.");
            return (false, "Network error calling Google Books API.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GoogleBooksService.");
            return (false, "Unexpected server error occurred.", null);
        }
    }
}
