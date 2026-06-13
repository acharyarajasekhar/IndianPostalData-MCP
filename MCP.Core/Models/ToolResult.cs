namespace MCP.Core.Models;

public record ToolResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }

    public static ToolResult<T> Ok(T data, Dictionary<string, object>? metadata = null) => new()
    {
        Success = true,
        Data = data,
        Metadata = metadata
    };

    public static ToolResult<T> Fail(string errorMessage, string? errorCode = null) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        ErrorCode = errorCode
    };
}