using System.Collections.Generic;

namespace MCP.Core.Models;

public record PostalApiResponse
{
    public string Message { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public List<PostOffice> PostOffice { get; init; } = new();
}