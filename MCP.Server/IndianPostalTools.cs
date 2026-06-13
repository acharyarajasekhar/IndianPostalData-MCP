using MCP.Server.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace MCP.Server;

[McpServerToolType]
public class PincodeTools
{
    private readonly IPostalDataService _postalDataService;
    private readonly ILogger<PincodeTools> _logger;

    public PincodeTools(IPostalDataService postalDataService, ILogger<PincodeTools> logger)
    {
        _postalDataService = postalDataService;
        _logger = logger;
    }

    [McpServerTool]  // Tools are now methods with this attribute
    [Description("Get all post offices in a given Indian pincode (6-digit postal code)")]
    public async Task<string> GetPostOfficesByPincode(
        [Description("6-digit Indian postal code (e.g., 110001, 560001)")]
        string pincode,
        CancellationToken cancellationToken = default)
    {
        var result = await _postalDataService.GetPincodeDataAsync(pincode, cancellationToken);
        if (!result.Success)
        {
            // Return an error object as JSON for consistency
            var err = new { success = false, error = result.ErrorMessage ?? "Unknown error", pincode };
            return JsonSerializer.Serialize(err);
        }

        // Successful response – serialize the data payload
        var response = new
        {
            success = true,
            pincode = pincode,
            message = result.Data?.Message,
            status = result.Data?.Status,
            postOffices = result.Data?.PostOffice
        };
        return JsonSerializer.Serialize(response);
    }
    
    
    [McpServerTool]
    [Description("Get post offices matching a given post office name (case-insensitive)")]
    public async Task<string> GetPostOfficesByName(
        [Description("Name of the post office (e.g., 'Connaught Place')")]
        string name,
        CancellationToken cancellationToken = default)
    {
        var result = await _postalDataService.GetPostOfficeByCityAsync(name, cancellationToken);
        if (!result.Success)
        {
            var err = new { success = false, error = result.ErrorMessage ?? "Unknown error", name };
            return JsonSerializer.Serialize(err);
        }

        var response = new
        {
            success = true,
            name = name,
            message = result.Data?.Message,
            status = result.Data?.Status,
            postOffices = result.Data?.PostOffice
        };
        return JsonSerializer.Serialize(response);
    }
}