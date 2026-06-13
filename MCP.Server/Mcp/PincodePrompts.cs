using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCP.Server;

/// <summary>
/// Pre‑defined prompt templates that can be fetched via the MCP protocol.
/// Each method is marked with <c>[McpPrompt]</c> and returns a ready‑to‑use prompt
/// string. Clients can request the prompt by name and then feed the resulting
/// prompt to an LLM or another tool.
/// </summary>
[McpServerPromptType]
public class PincodePrompts
{
    private readonly ILogger<PincodePrompts> _logger;

    public PincodePrompts(ILogger<PincodePrompts> logger)
    {
        _logger = logger;
    }

    // --------------------------------------------------------------------
    // Prompt: Analyze delivery network coverage for a district
    // --------------------------------------------------------------------
    [McpServerPrompt]
    [Description("Analyze the delivery network coverage in a specific district using pincode data")]
    public string AnalyzeDeliveryNetwork(
        [Description("District name (e.g., 'Central Delhi', 'Bangalore Urban')")] string district,
        [Description("State name to filter (optional)")] string? state = null)
    {
        var stateFilter = string.IsNullOrEmpty(state) ? "" : $" in {state}";
        return $"""
        You are a logistics analyst. Analyze the postal delivery network for {district}{stateFilter}.

        Please follow this analysis structure:

        1. **Network Overview**
           - List all pincodes in {district}
           - Identify the primary Head Post Office (delivery office)
           - Count total delivery vs non-delivery offices

        2. **Coverage Assessment**
           - Identify any coverage gaps or areas served by neighboring districts
           - Note any special branch types (E.g., Sub Post Office, Head Post Office)

        3. **Efficiency Recommendations**
           - Suggest optimization opportunities
           - Identify high-density areas that may need additional resources

        Use the `get_post_offices_by_pincode` tool to fetch pincode data. Start with pincodes in {district}
        and analyze systematically.
        """;
    }

    // --------------------------------------------------------------------
    // Prompt: Validate an address's pincode
    // --------------------------------------------------------------------
    [McpServerPrompt]
    [Description("Template for validating whether an address has a correct pincode")]
    public string ValidateAddressPrompt(
        [Description("Complete address including locality, city, and pincode")] string address)
    {
        return $"""
        You are an address validation expert for India Post.

        Validate this address:
        \"{address}\"

        Follow this validation checklist:

        1. **Pincode Format Check**
           - Is it exactly 6 digits?
           - Does the first digit match the state?

        2. **Location-Pincode Match**
           - Use `get_post_offices_by_pincode` to verify the pincode
           - Check if the locality/city belongs to that pincode
           - Verify district and state match

        3. **Output Format**
           - VALID: If everything matches
           - PARTIAL: If pincode exists but location doesn't match
           - INVALID: If pincode doesn't exist

           Provide specific correction suggestions for invalid cases.
        """;
    }

    // --------------------------------------------------------------------
    // Prompt: Generate pincode statistics for a state
    // --------------------------------------------------------------------
    [McpServerPrompt]
    [Description("Generate statistics about pincode distribution in a state")]
    public string GeneratePincodeStatistics(
        [Description("State name (e.g., 'Maharashtra', 'Tamil Nadu')")] string state)
    {
        return $"""
        You are a data analyst. Generate comprehensive statistics about pincode distribution in {state}.

        Required analysis:

        1. **Coverage Metrics**
           - Total number of pincodes in {state}
           - Distribution across districts
           - Urban vs rural pincode ratio (based on branch type)

        2. **Service Analysis**
           - Head Post Office density
           - Sub Post Office distribution
           - Delivery vs Non-Delivery ratio

        3. **Comparison**
           - Compare with national averages
           - Identify underserved districts

        Use multiple `get_post_offices_by_pincode` calls across different pincodes
        to build a representative sample for {state}.
        """;
    }

    // --------------------------------------------------------------------
    // Prompt: Find the correct pincode for an area
    // --------------------------------------------------------------------
    [McpServerPrompt]
    [Description("Template to help find the correct pincode for an area")]
    public string FindPincodeTemplate(
        [Description("Area/locality name (e.g., 'Connaught Place', 'Bandra')")] string area,
        [Description("City name")] string city,
        [Description("State name")] string state)
    {
        return $"""
        Find the correct pincode for: {area}, {city}, {state}

        Search strategy:
        1. If you know a nearby pincode, use `get_post_offices_by_pincode` to see area coverage
        2. Look for post offices with names matching the area
        3. Check for Sub Post Offices (non-delivery) - these often serve specific localities

        Report back with:
        - The most likely pincode
        - Alternative nearby pincodes
        - The primary delivery office for that area
        - Any notes about delivery coverage (e.g., "Non-delivery offices serve this area")
        """;
    }
}
