using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Retry;
using MCP.Core.Models;

namespace MCP.Server.Services;

public class PostalDataService : IPostalDataService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private const string BaseUrl = "https://api.postalpincode.in/pincode/";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public PostalDataService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;

        // Define a retry policy: retry 3 times with exponential backoff (2s, 4s, 8s)
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    // Existing method: fetch by pincode
    public async Task<ToolResult<PostalApiResponse>> GetPincodeDataAsync(string pincode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(pincode))
        {
            return ToolResult<PostalApiResponse>.Fail("Pincode cannot be empty.", "INVALID_INPUT");
        }

        // Basic validation: Indian pincodes are 6 digits
        if (pincode.Length != 6 || !pincode.All(char.IsDigit))
        {
            return ToolResult<PostalApiResponse>.Fail("Invalid pincode format. Please provide a 6-digit numeric pincode.", "INVALID_INPUT");
        }

        string cacheKey = $"pincode_{pincode}";

        // Try to get data from cache first
        if (_cache.TryGetValue(cacheKey, out PostalApiResponse? cachedResult) && cachedResult != null)
        {
            return ToolResult<PostalApiResponse>.Ok(cachedResult);
        }

        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"{BaseUrl}{pincode}", ct));

            if (!response.IsSuccessStatusCode)
            {
                return ToolResult<PostalApiResponse>.Fail($"API returned an error: {response.StatusCode}", "API_UNAVAILABLE");
            }

            var data = await response.Content.ReadFromJsonAsync<List<PostalApiResponse>>(cancellationToken: ct);
            if (data == null || data.Count == 0)
            {
                return ToolResult<PostalApiResponse>.Fail("No data found for the provided pincode.", "NOT_FOUND");
            }
            var result = data[0];

            if (result.Status != "Success")
            {
                return ToolResult<PostalApiResponse>.Fail(result.Message ?? "API error occurred", "API_ERROR");
            }

            // Cache the successful result
            _cache.Set(cacheKey, result, CacheDuration);

            return ToolResult<PostalApiResponse>.Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return ToolResult<PostalApiResponse>.Fail($"Network error after retries: {ex.Message}", "NETWORK_ERROR");
        }
        catch (Exception ex)
        {
            return ToolResult<PostalApiResponse>.Fail($"An unexpected error occurred: {ex.Message}", "INTERNAL_ERROR");
        }
    }

    // New method: fetch post office data by city name
    public async Task<ToolResult<PostalApiResponse>> GetPostOfficeByCityAsync(string city, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return ToolResult<PostalApiResponse>.Fail("City name cannot be empty.", "INVALID_INPUT");
        }

        var encodedCity = Uri.EscapeDataString(city);
        string cacheKey = $"city_{encodedCity}";

        if (_cache.TryGetValue(cacheKey, out PostalApiResponse? cachedResult) && cachedResult != null)
        {
            return ToolResult<PostalApiResponse>.Ok(cachedResult);
        }

        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"https://api.postalpincode.in/postoffice/{encodedCity}", ct));

            if (!response.IsSuccessStatusCode)
            {
                return ToolResult<PostalApiResponse>.Fail($"API returned an error: {response.StatusCode}", "API_UNAVAILABLE");
            }

            var data = await response.Content.ReadFromJsonAsync<List<PostalApiResponse>>(cancellationToken: ct);
            if (data == null || data.Count == 0)
            {
                return ToolResult<PostalApiResponse>.Fail("No data found for the provided city.", "NOT_FOUND");
            }
            var result = data[0];

            if (result.Status != "Success")
            {
                return ToolResult<PostalApiResponse>.Fail(result.Message ?? "API error occurred", "API_ERROR");
            }

            _cache.Set(cacheKey, result, CacheDuration);
            return ToolResult<PostalApiResponse>.Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return ToolResult<PostalApiResponse>.Fail($"Network error after retries: {ex.Message}", "NETWORK_ERROR");
        }
        catch (Exception ex)
        {
            return ToolResult<PostalApiResponse>.Fail($"An unexpected error occurred: {ex.Message}", "INTERNAL_ERROR");
        }
    }
}
