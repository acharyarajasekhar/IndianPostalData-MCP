using System.Net.Http.Json;
using MCP.Core.Models;

namespace MCP.Server.Services;

public class PostalDataService : IPostalDataService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.postalpincode.in/pincode/";

    public PostalDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ToolResult<PostalApiResponse>> GetPincodeDataAsync(string pincode, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<PostalApiResponse>>($"{BaseUrl}{pincode}", ct);

            if (response == null || response.Count == 0)
            {
                return ToolResult<PostalApiResponse>.Fail("No data found for the provided pincode.", "NOT_FOUND");
            }

            var result = response[0];

            if (result.Status != "Success")
            {
                return ToolResult<PostalApiResponse>.Fail(result.Message, "API_ERROR");
            }

            return ToolResult<PostalApiResponse>.Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return ToolResult<PostalApiResponse>.Fail($"Network error: {ex.Message}", "NETWORK_ERROR");
        }
        catch (Exception ex)
        {
            return ToolResult<PostalApiResponse>.Fail($"An unexpected error occurred: {ex.Message}", "INTERNAL_ERROR");
        }
    }
}
