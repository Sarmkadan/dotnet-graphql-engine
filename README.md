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

Configure the engine via `GraphQLEngineOptions`:

```csharp
services.AddGraphQLEngine(options =>
{
    options.MaxQueryComplexity = 5000;
    options.MaxQueryDepth = 15;
    options.EnableCaching = true;
    options.CacheTTLSeconds = 300;
    options.CacheMaxSize = 1000;
    options.EnableIntrospection = true;
    options.EnableSubscriptions = true;
});
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

