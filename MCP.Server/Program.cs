using MCP.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<IPostalDataService, PostalDataService>();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapHealthChecks("/health");

app.MapGet("/", () => "Indian Postal Data MCP Server is running!");

app.Run();

