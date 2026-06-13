using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace MCP.Server;

/// <summary>
/// Collection of static resources that can be accessed through the MCP protocol.
/// These resources are served via the built‑in resource‑template mechanism (`WithResourceTemplate`).
/// </summary>
[McpServerResourceType]
public class PincodeResources
{
    private readonly ILogger<PincodeResources> _logger;

    public PincodeResources(ILogger<PincodeResources> logger)
    {
        _logger = logger;
    }

    // --------------------------------------------------------------------
    // Resource 1 – static reference data (pincode format rules)
    // --------------------------------------------------------------------
    [McpServerResource]
    [Description("Indian pincode format specifications – 6‑digit structure rules")]
    public string GetPincodeFormatRules()
    {
        return """
        # Indian Pincode Format Rules

        ## Structure
        - Total digits: 6
        - First digit: Zone (1‑8 for geographic zones, 9 for Army)
        - First 2 digits: Postal circle/state
        - First 3 digits: Sorting district
        - Last 3 digits: Delivery post office

        ## Zone Map
        - 1: Delhi, Haryana, Punjab, Himachal, J&K, Chandigarh
        - 2: UP, Uttarakhand
        - 3: Rajasthan, Gujarat, MP, Chhattisgarh, Dadra, Daman, Diu
        - 4: Maharashtra, Goa, Karnataka
        - 5: Telangana, AP, Odisha
        - 6: Tamil Nadu, Kerala, Puducherry, Lakshadweep
        - 7: West Bengal, Assam, Sikkim, North East states
        - 8: Bihar, Jharkhand
        - 9: Army Postal Service (APS)
        """;
    }

    // --------------------------------------------------------------------
    // Resource 2 – state‑to‑zone mapping
    // --------------------------------------------------------------------
    [McpServerResource]
    [Description("Mapping of Indian states to pincode zones (first‑digit mapping)")]
    public string GetStateZoneMapping()
    {
        var mapping = new Dictionary<string, int>
        {
            ["Delhi"] = 1, ["Haryana"] = 1, ["Punjab"] = 1,
            ["Uttar Pradesh"] = 2, ["Uttarakhand"] = 2,
            ["Rajasthan"] = 3, ["Gujarat"] = 3, ["Madhya Pradesh"] = 3,
            ["Maharashtra"] = 4, ["Goa"] = 4, ["Karnataka"] = 4,
            ["Telangana"] = 5, ["Andhra Pradesh"] = 5, ["Odisha"] = 5,
            ["Tamil Nadu"] = 6, ["Kerala"] = 6,
            ["West Bengal"] = 7, ["Assam"] = 7,
            ["Bihar"] = 8, ["Jharkhand"] = 8,
            ["Army"] = 9
        };

        return JsonSerializer.Serialize(mapping, new JsonSerializerOptions { WriteIndented = true });
    }

    // --------------------------------------------------------------------
    // Resource 3 – example response schema (helps LLMs understand shape)
    // --------------------------------------------------------------------
    [McpServerResource]
    [Description("JSON schema for the India Post Pincode API response")]
    public string GetResponseSchema()
    {
        return """
        {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "Message": { "type": "string" },
            "Status": { "type": "string", "enum": ["Success", "Error"] },
            "PostOffice": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "Name": { "type": "string" },
                  "BranchType": { "type": "string" },
                  "DeliveryStatus": { "type": "string" },
                  "District": { "type": "string" },
                  "State": { "type": "string" },
                  "Pincode": { "type": "string" }
                }
              }
            }
          }
        }
        """;
    }
}
