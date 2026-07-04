# .NET GraphQL Engine

A production-grade, code-first GraphQL server for .NET applications.

![Build](https://github.com/sarmkadan/dotnet-graphql-engine/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/dotnet-graphql-engine)

## Installation

### Prerequisites

- .NET 9.0 SDK or later
- Git (for source installation)

### Option 1: Install as NuGet Package (Recommended)

```bash
dotnet add package dotnet-graphql-engine
```

### Option 2: Clone and Build from Source

```bash
git clone https://github.com/sarmkadan/dotnet-graphql-engine.git
cd dotnet-graphql-engine
dotnet build
```

### Option 3: Use in Your Project

Add the package to your `.csproj` file:

```xml
<PackageReference Include="dotnet-graphql-engine" Version="1.0.0" />
```

## Quick Start

Here's a complete example to get started with .NET GraphQL Engine:

```csharp
using Microsoft.Extensions.DependencyInjection;
using GraphQLEngine.Configuration;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Services.Schema;

// Setup services
var services = new ServiceCollection();
services.AddGraphQLEngine(options =>
{
    options.MaxQueryComplexity = 5000;
    options.EnableCaching = true;
});

var serviceProvider = services.BuildServiceProvider();

// Create schema
var schemaService = serviceProvider.GetRequiredService<SchemaService>();
var schema = schemaService.CreateSchema("MyAPI");

// Define a User type
var userType = new GraphQLType
{
    Name = "User",
    Description = "A user in the system",
    Kind = TypeKind.Object
};

userType.AddField(new GraphQLField
{
    Name = "id",
    Type = "ID!",
    Description = "The user's unique identifier"
});

userType.AddField(new GraphQLField
{
    Name = "name",
    Type = "String!",
    Description = "The user's name"
});

userType.AddField(new GraphQLField
{
    Name = "email",
    Type = "String!",
    Description = "The user's email address"
});

schemaService.AddType("MyAPI", userType);

// Register a resolver for the user field
var executionService = serviceProvider.GetRequiredService<GraphQLExecutionService>();
executionService.RegisterResolver("user", async (context) =>
{
    // Your resolver logic here
    return new { id = "1", name = "John Doe", email = "john@example.com" };
});

// Execute a query
var query = new GraphQLQuery("{ user { id name email } }");
var result = await executionService.ExecuteAsync(query);

Console.WriteLine(result.Data);
```

## Configuration

The .NET GraphQL Engine supports flexible configuration through:

### 1. Code-based Configuration

Configure the engine via `GraphQLEngineOptions`:

```csharp
services.AddGraphQLEngine(options =>
{
    // Service identification
    options.ServiceName = "MyGraphQLService";
    options.Version = "2.0.0";

    // Query execution limits
    options.MaxQueryComplexity = 5000;      // Prevent expensive queries
    options.MaxQueryDepth = 15;             // Prevent deeply nested queries  
    options.MaxQueryLength = 10000;         // Maximum query length in characters
    options.MaxQueryFields = 200;           // Maximum fields per query
    options.QueryTimeoutMs = 30000;         // 30 seconds timeout
    options.MaxBatchSize = 100;             // Maximum batch size

    // Feature flags
    options.EnableIntrospection = true;      // Allow schema exploration
    options.EnableCaching = true;             // Enable query result caching
    options.EnableSubscriptions = true;       // Enable GraphQL subscriptions
    options.EnableDataLoading = true;          // Enable DataLoader batching
    options.EnableSchemaStitching = true;     // Enable schema stitching
    options.EnableDetailedErrorMessages = false; // Disable in production for security
    options.EnablePerformanceMetrics = true;    // Collect performance metrics

    // Caching configuration
    options.CacheTTLSeconds = 300;          // 5 minutes cache TTL
    options.CacheMaxSizeBytes = 52428800;  // 50MB maximum cache size

    // Subscription settings
    options.MaxSubscriptionConnections = 100; // Maximum concurrent connections
    options.SubscriptionTimeoutMs = 30000;    // 30 seconds timeout
    options.HeartbeatIntervalMs = 30000;     // 30 seconds heartbeat

    // DataLoader settings
    options.DataLoaderBatchSize = 100;       // Batch size for DataLoader
    options.DataLoaderDelayMs = 50;          // Delay before flushing batches

    // Remote schema options
    options.EnableRemoteSchemaIntrospection = true;  // Enable remote schema introspection
    options.RemoteSchemaTimeoutMs = 30000;        // 30 seconds timeout

    // Error handling
    options.LogInternalErrors = true;         // Log internal errors
    options.IncludeDetailedErrorMessages = false; // Don't expose internal errors to clients
});
```

### 2. Configuration via appsettings.json

You can configure the engine using standard .NET configuration files:

```json
{
  "GraphQL": {
    "ServiceName": "MyGraphQLService",
    "Version": "2.0.0",
    "MaxQueryComplexity": 5000,
    "MaxQueryDepth": 15,
    "QueryTimeoutMs": 30000,
    "EnableCaching": true,
    "CacheTTLSeconds": 300,
    "EnableSubscriptions": true
  }
}
```

Then register the configuration in your startup:

```csharp
services.AddGraphQLEngine(Configuration.GetSection("GraphQL"));
```

### 3. Predefined Configuration Profiles

The engine provides predefined configuration profiles for common scenarios:

```csharp
// Strict configuration for production (low limits)
services.AddGraphQLEngineStrict();

// Default configuration (balanced)
services.AddGraphQLEngineDefault();

// Permissive configuration for development
services.AddGraphQLEnginePermissive();
```

### 4. All Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `ServiceName` | string | "dotnet-graphql-engine" | Service identifier |
| `Version` | string | "1.0.0" | Service version |
| `MaxQueryComplexity` | int | 5000 | Maximum query complexity score |
| `MaxQueryDepth` | int | 10 | Maximum query nesting depth |
| `MaxQueryLength` | int | 10000 | Maximum query length in characters |
| `MaxQueryFields` | int | 200 | Maximum fields per query |
| `QueryTimeoutMs` | int | 30000 | Query execution timeout (ms) |
| `MaxBatchSize` | int | 100 | Maximum batch size for queries |
| `EnableIntrospection` | bool | true | Enable GraphQL introspection |
| `EnableCaching` | bool | true | Enable query result caching |
| `EnableSubscriptions` | bool | true | Enable GraphQL subscriptions |
| `EnableDataLoading` | bool | true | Enable DataLoader batching |
| `EnableSchemaStitching` | bool | true | Enable schema stitching |
| `EnableDetailedErrorMessages` | bool | false | Show detailed errors (disable in production) |
| `EnablePerformanceMetrics` | bool | true | Collect performance metrics |
| `CacheTTLSeconds` | int | 300 | Cache time-to-live (seconds) |
| `CacheMaxSizeBytes` | int | 52428800 | Maximum cache size (bytes) |
| `MaxSubscriptionConnections` | int | 100 | Maximum concurrent subscriptions |
| `SubscriptionTimeoutMs` | int | 30000 | Subscription timeout (ms) |
| `HeartbeatIntervalMs` | int | 30000 | Subscription heartbeat (ms) |
| `DataLoaderBatchSize` | int | 100 | DataLoader batch size |
| `DataLoaderDelayMs` | int | 50 | DataLoader batch delay (ms) |
| `EnableRemoteSchemaIntrospection` | bool | true | Enable remote schema introspection |
| `RemoteSchemaTimeoutMs` | int | 30000 | Remote schema timeout (ms) |
| `LogInternalErrors` | bool | true | Log internal errors |
| `IncludeDetailedErrorMessages` | bool | false | Include detailed errors in responses |

### 5. Validation

All configuration options are validated using DataAnnotations:

- Required fields must be provided
- Numeric values have minimum/maximum constraints
- String values have length constraints
- Invalid configurations throw exceptions at startup

Example of validation error:
```
Invalid GraphQL engine options: 
- MaxQueryComplexity must be greater than 0
- CacheTTLSeconds cannot be negative
```

## Features

- ✅ Code-first schema definition
- ✅ Query execution and validation
- ✅ Schema introspection
- ✅ Query complexity analysis
- ✅ Caching support
- ✅ Subscription support
- ✅ DataLoader integration
- ✅ Multiple schema support
- ✅ Schema stitching
- ✅ In-memory repository

## Examples

We provide several practical examples to help you get started with the .NET GraphQL Engine. You can find them in the `examples/` directory:

- [BasicUsage.cs](examples/BasicUsage.cs) - Minimal setup and first call.
- [AdvancedUsage.cs](examples/AdvancedUsage.cs) - Configuration, custom options, and error handling.
- [IntegrationExample.cs](examples/IntegrationExample.cs) - Integrating into an ASP.NET Core project.

For more complex scenarios, check out the existing files in the [examples/](examples/) directory.

## Docker Support

We provide Docker support for easy deployment and development.

### Running with Docker Compose

To run the application and Redis dependency using Docker Compose:

```bash
docker-compose up --build
```

The application will be available at `http://localhost:8080`.

### Building the Docker Image

To build the Docker image manually:

```bash
docker build -t dotnet-graphql-engine .
```

## Performance Benchmarks

We use [BenchmarkDotNet](https://benchmarkdotnet.org/) to measure and track performance metrics.

### Running Benchmarks Locally

```bash
# Navigate to benchmarks directory
cd benchmarks/dotnet-graphql-engine.Benchmarks

# Run all benchmarks
dotnet run -c Release -- --filter *

# Run benchmarks with memory diagnostics
dotnet run -c Release -- --filter * --memory
```

### Benchmark Categories
- **Query Execution**: Simple, nested, and complex query performance
- **Schema Operations**: Schema creation and type management  
- **Resolver Registration**: Field resolver registration performance

### CI Integration

Benchmarks run automatically in CI to track performance over time. See `.github/workflows/benchmarks.yml` for details.

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

