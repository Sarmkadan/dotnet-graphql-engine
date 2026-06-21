# dotnet-graphql-engine

A production-grade, code-first GraphQL server for .NET.

![Build](https://github.com/sarmkadan/dotnet-graphql-engine/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/dotnet-graphql-engine)

## Installation

```bash
git clone https://github.com/sarmkadan/dotnet-graphql-engine.git
cd dotnet-graphql-engine
dotnet build
```

## Quick Start

```csharp
// Define a User type
var userType = new GraphQLType { Name = "User", ... };

// Create schema
var schemaService = serviceProvider.GetRequiredService<SchemaService>();
var schema = schemaService.CreateSchema("MyAPI");
schemaService.AddType("MyAPI", userType);

// Execute a query
var executionService = serviceProvider.GetRequiredService<GraphQLEngine.Services.GraphQL.GraphQLExecutionService>();
var query = new GraphQLQuery("{ user(id: \"1\") { id } }");
var context = await executionService.ExecuteAsync(query);
```

## Configuration

Configure the engine via `GraphQLEngineOptions`:

```csharp
services.AddGraphQLEngine(options =>
{
    options.MaxQueryComplexity = 5000;
    options.EnableCaching = true;
});
```

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

