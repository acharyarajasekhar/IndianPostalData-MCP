# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Common Development Commands

| Task | Command | Notes |
|------|---------|-------|
| **Restore packages** | `dotnet restore` | Restores NuGet dependencies for the entire solution.
| **Build solution** | `dotnet build` | Compiles all projects (Server, Core, Tests) in Debug configuration.
| **Run the API server** | `dotnet run --project MCP.Server\MCP.Server.csproj` | Starts the ASP.NET Core web server. The health endpoint is available at `http://localhost:<port>/health` and a simple root message at `/`.
| **Run all tests** | `dotnet test` | Executes the XUnit test suite in `MCP.Tests`. Coverage is collected via `coverlet` (already configured).
| **Run a single test** | `dotnet test --filter "FullyQualifiedName~Namespace.ClassName.MethodName"` | Replace the filter with the test you want to target.
| **Watch & rebuild** | `dotnet watch run --project MCP.Server\MCP.Server.csproj` | Auto‑restarts the server when source files change (useful during local development).
| **Apply migrations** | *(none – this project does not use a database)* |
| **Format code** | `dotnet format` | Runs the built‑in formatter on the solution.
| **Lint / static analysis** | `dotnet format analyzers` | Invokes Roslyn analyzers.

## High‑Level Architecture

- **Solution (`IndianPostalDataMCP.sln`)** contains three primary projects:
  - **`MCP.Server`** – ASP.NET Core Web API hosting the service.
    - Entry point: `Program.cs` configures a minimal API, registers `IPostalDataService` via `AddHttpClient`, and adds health checks.
    - Uses **Swashbuckle** for OpenAPI/Swagger UI, **Polly** for resilience, **MemoryCache** for caching, and **OpenTelemetry** for telemetry.
    - Service implementations live under `MCP.Server/Services/` (e.g., `PostalDataService`).
  - **`MCP.Core`** – Class library containing shared models and interfaces.
    - Currently defines `MCP.Core.Models.PostalApiResponse` and the `IPostalDataService` interface used by the server.
    - Keeps the core domain logic independent of any ASP.NET specifics, enabling reuse in tests or other hosts.
  - **`MCP.Tests`** – XUnit test project.
    - References both `MCP.Core` and `MCP.Server` to allow integration‑style tests.
    - Uses `coverlet.collector` for code‑coverage reporting.

- **Dependency Flow**

```
MCP.Server → MCP.Core
MCP.Tests  → MCP.Server, MCP.Core
```

  The Server depends on Core for contracts and data models; Tests depend on both to validate behavior.

- **Key Runtime Concerns**
  - **Caching** – `PostalDataService` caches successful API responses for 24 hours using `IMemoryCache`.
  - **Resilience** – A Polly retry policy with exponential back‑off (2 s, 4 s, 8 s) is applied to all outbound HTTP calls.
  - **Validation** – Input pincode is validated (non‑empty, six digits). Errors are wrapped in a `ToolResult<T>` discriminated‑union style object.
  - **Telemetry** – OpenTelemetry is wired via NuGet packages; no custom instrumentation is present yet.

## Project Structure (collapsed for brevity)

- `MCP.Server/`
  - `Program.cs` – minimal API bootstrap.
  - `Services/` – concrete service implementations.
  - `Properties/launchSettings.json` – local debug launch profiles.
  - `appsettings*.json` – configuration (currently empty, can be extended).
- `MCP.Core/`
  - `Models/` – POCOs matching the external postal API response.
  - `Interfaces/` – service contracts (e.g., `IPostalDataService`).
- `MCP.Tests/`
  - Test classes using XUnit.

## Configuration Files

- **`appsettings.json` / `appsettings.Development.json`** – standard ASP.NET Core configuration files. They are currently minimal but can hold settings such as external API base URLs, cache durations, or feature flags.
- **`launchSettings.json`** – defines the launch profile used by `dotnet watch` and IDE debugging.

## Tooling & Extensions

- No **`.cursor/rules/**`** or **`.github/copilot‑instructions.md`** files are present, so no special Copilot or Cursor rules need to be enforced.
- Standard .NET tooling (`dotnet`, `dotnet format`, `dotnet test`) is sufficient.

## Suggested Development Workflow
1. **Restore & Build** – `dotnet restore && dotnet build`.
2. **Run Locally** – `dotnet watch run --project MCP.Server\MCP.Server.csproj`.
3. **Exercise the API** – Curl or a browser to `http://localhost:<port>/` and the health endpoint.
4. **Write Tests** – Add XUnit tests under `MCP.Tests`. Run them frequently with `dotnet test`.
5. **Iterate** – After changes, run the full test suite and verify the server still starts.

## Notes for Future Claude Instances
- When adding new features, consider extending the **Core** layer first (models, interfaces) before touching the **Server** layer.
- Follow the existing pattern in `PostalDataService`: validate inputs early, attempt cache retrieval, then execute a resilient HTTP call, map the response, cache it, and return a `ToolResult<T>`.
- Keep configuration (e.g., external API URLs) in `appsettings.json` to avoid hard‑coding values.
- Use the built‑in **Polly** policies for any new outbound HTTP interactions.
- Remember to add or update unit tests in `MCP.Tests` to cover new logic paths.
