using MCP.Server.Services;
using MCP.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IPostalDataService, PostalDataService>();
builder.Services.AddHealthChecks();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

builder.Services.AddCors();

var app = builder.Build();

// 2. Enable CORS if needed by your AI client
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

// 3. THIS IS THE CORRECT METHOD: Mounts the entire MCP server protocol route
app.MapMcp("/mcp");

// Configure the HTTP request pipeline.
app.MapHealthChecks("/health");

app.MapGet("/", () => "Indian Postal Data MCP Server is running!");

// Fetch postal data for a given pincode
app.MapGet("/postal/{pincode}", async (string pincode, IPostalDataService service, CancellationToken ct) =>
{
    var result = await service.GetPincodeDataAsync(pincode, ct);
    return result.Success
        ? Results.Ok(result.Data)
        : Results.Problem(result.ErrorMessage ?? "Unknown error", statusCode: 400);
});

// Fetch post office data by city name
app.MapGet("/city/{city}", async (string city, IPostalDataService service, CancellationToken ct) =>
{
    var result = await service.GetPostOfficeByCityAsync(city, ct);
    return result.Success
        ? Results.Ok(result.Data)
        : Results.Problem(result.ErrorMessage ?? "Unknown error", statusCode: 400);
});

app.Run();

