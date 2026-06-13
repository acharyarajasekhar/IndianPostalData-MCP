# Indian Postal Data MCP

A .NET 6 Minimal API that provides Indian postal data via an external API with caching, resilience, and OpenTelemetry.

## 🎯 Overview

- **Server** – ASP.NET Core Minimal API (`MCP.Server`) exposing endpoints to retrieve postal information.
- **Core** – Shared models and interfaces (`MCP.Core`) used by the server and tests.
- **Tests** – XUnit test suite (`MCP.Tests`) covering service logic and integration.

Key features:
- **Caching**: Responses are cached for 24 hours using `IMemoryCache`.
- **Resilience**: Polly retry policies with exponential back‑off for outbound HTTP calls.
- **Telemetry**: OpenTelemetry instrumentation (ready for exporters).
- **Swagger**: Auto‑generated OpenAPI UI.

## 📦 Prerequisites

- [.NET SDK 6+](https://dotnet.microsoft.com/download) installed.
- Optional: `dotnet watch` for live‑reloading during development.

## 🔧 Development Workflow

```bash
# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run the API (development mode)
# The server will start on a random port; check the console output for the URL.
 dotnet watch run --project MCP.Server\MCP.Server.csproj
```

The health endpoint is available at `http://localhost:<port>/health` and a simple root message at `/`.

## ✅ Running Tests

```bash
# Run the full test suite
 dotnet test

# Run a single test (replace with your fully‑qualified test name)
 dotnet test --filter "FullyQualifiedName~Namespace.ClassName.MethodName"
```

Coverage is collected via **coverlet** (already configured).

## 📚 Configuration

Configuration lives in `appsettings.json` / `appsettings.Development.json`. You can add:
- External API base URL
- Cache durations
- Feature flags

## 📄 License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please:
1. Fork the repository.
2. Create a feature branch.
3. Add tests for your changes.
4. Submit a pull request.

Make sure the test suite passes and the server starts successfully.

## 🛠️ ModelContext Protocol Inspector

You can inspect the running server with the ModelContext Protocol inspector.

**Option A:** Run it against your compiled .NET EXE binary

```bash
npx @modelcontextprotocol/inspector path\\to\\your\\bin\\Debug\\net8.0\\MyMcpServer.exe
```

**Option B:** Run it using the dotnet CLI directly

```bash
npx @modelcontextprotocol/inspector dotnet run --project path\\to\\MyMcpServer.csproj
```

## 📞 Support

For issues or questions, open an issue on the repository or contact the maintainer.
