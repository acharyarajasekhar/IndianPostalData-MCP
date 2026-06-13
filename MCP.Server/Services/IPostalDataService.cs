using MCP.Core.Models;

namespace MCP.Server.Services;

public interface IPostalDataService
{
    Task<ToolResult<PostalApiResponse>> GetPincodeDataAsync(string pincode, CancellationToken ct = default);
    Task<ToolResult<PostalApiResponse>> GetPostOfficeByCityAsync(string city, CancellationToken ct = default);
}