namespace MCP.Core.Models;

public record PostOffice
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string BranchType { get; init; } = string.Empty;
    public string DeliveryStatus { get; init; } = string.Empty;
    public string Circle { get; init; } = string.Empty;
    public string District { get; init; } = string.Empty;
    public string Division { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public string Block { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string Pincode { get; init; } = string.Empty;
}