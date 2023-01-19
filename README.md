# .NET GraphQL Engine

A production‑grade, code‑first GraphQL server for .NET applications.

![Build](https://github.com/sarmkadan/dotnet-graphql-engine/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/dotnet-graphql-engine)

## Installation

### Prerequisites

- .NET 9.0 SDK or later
- Git (for source installation)

...

## Configuration

The engine is configured through the `GraphQLEngineOptions` (or the alternative
`DotnetGraphqlEngineOptions`) class which is bound via the standard .NET
`IOptions<T>` pattern. All settings can be supplied through code, an
`appsettings.json` file, or any other configuration source supported by
`Microsoft.Extensions.Configuration`.

### 1. Code‑based configuration

```csharp
services.AddGraphQLEngine(options =>
{
    options.ServiceName = "MyGraphQLService";
    options.Version = "2.0.0";

    // Query limits
    options.MaxQueryComplexity = 5000;
    options.MaxQueryDepth = 15;
    options.MaxQueryLength = 20000;
    options.QueryTimeoutMs = 30000;

    // Feature flags
    options.EnableIntrospection = true;
    options.EnableCaching = true;
    options.EnableSubscriptions = true;
    options.EnableDataLoading = true;
    options.EnableSchemaStitching = true;
    options.EnableDetailedErrorMessages = false;
    options.EnablePerformanceMetrics = true;

    // Caching
    options.CacheTTLSeconds = 300;
    options.CacheMaxSizeBytes = 52428800;

    // Subscriptions
    options.MaxSubscriptionConnections = 100;
    options.SubscriptionTimeoutMs = 30000;
    options.HeartbeatIntervalMs = 30000;

    // DataLoader
    options.DataLoaderBatchSize = 100;
    options.DataLoaderDelayMs = 50;

    // Federation
    options.EnableFederation = false;
    options.FederationDiscoveryEndpoint = "/.well-known/federation";
    options.FederationTimeout = TimeSpan.FromSeconds(30);
    options.EntityCacheTtlSeconds = 300;
    options.EntityCacheMaxSize = 100000;

    // Remote schema
    options.EnableRemoteSchemaIntrospection = true;
    options.RemoteSchemaTimeoutMs = 30000;

    // Logging / error handling
    options.LogInternalErrors = true;
    options.IncludeDetailedErrorMessages = false;
});
```

### 2. `appsettings.json` (or `appsettings.Development.json`)

All configurable values are listed in the example file
[`appsettings.example.json`](appsettings.example.json). A minimal excerpt looks
like this:

```json
{
  "GraphQL": {
    "ServiceName": "dotnet-graphql-engine",
    "Version": "1.0.0",
    "MaxQueryComplexity": 5000,
    "MaxQueryDepth": 10,
    "MaxQueryLength": 10000,
    "QueryTimeoutMs": 30000,
    "EnableIntrospection": true,
    "EnableCaching": true,
    "EnableSubscriptions": true,
    "EnableDataLoading": true,
    "EnableSchemaStitching": true,
    "EnableDetailedErrorMessages": false,
    "EnablePerformanceMetrics": true,
    "CacheTTLSeconds": 300,
    "CacheMaxSizeBytes": 52428800,
    "MaxSubscriptionConnections": 100,
    "SubscriptionTimeoutMs": 30000,
    "HeartbeatIntervalMs": 30000,
    "DataLoaderBatchSize": 100,
    "DataLoaderDelayMs": 50,
    "EnableRemoteSchemaIntrospection": true,
    "RemoteSchemaTimeoutMs": 30000,
    "LogInternalErrors": true,
    "IncludeDetailedErrorMessages": false
  }
}
```

### 3. Full list of configuration options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `ServiceName` | `string` | `"dotnet-graphql-engine"` | Identifier for the service (used in logs). |
| `Version` | `string` | `"1.0.0"` | Service version. |
| `MaxQueryComplexity` | `int` | `5000` | Upper bound for query complexity score. |
| `MaxQueryDepth` | `int` | `10` | Maximum nesting depth of a query. |
| `MaxQueryLength` | `int` | `10000` | Maximum number of characters in a query. |
| `QueryTimeoutMs` | `int` | `30000` | Execution timeout in milliseconds. |
| `MaxQueryFields` | `int` | `200` | Maximum number of fields per query. |
| `MaxBatchSize` | `int` | `100` | Maximum size for batched queries. |
| `EnableIntrospection` | `bool` | `true` | Allow schema introspection. |
| `EnableCaching` | `bool` | `true` | Enable result caching. |
| `EnableSubscriptions` | `bool` | `true` | Enable GraphQL subscriptions. |
| `EnableDataLoading` | `bool` | `true` | Enable DataLoader batching. |
| `EnableSchemaStitching` | `bool` | `true` | Enable stitching of multiple schemas. |
| `EnableDetailedErrorMessages` | `bool` | `false` | Show detailed errors (disable in production). |
| `EnablePerformanceMetrics` | `bool` | `true` | Collect performance metrics. |
| `EnableFederation` | `bool` | `false` | Enable GraphQL Federation support. |
| `FederationDiscoveryEndpoint` | `string` | `"/.well-known/federation"` | Endpoint for federation discovery. |
| `FederationTimeout` | `TimeSpan` | `00:00:30` | Timeout for federation calls. |
| `EntityCacheTtlSeconds` | `int` | `300` | TTL for federation entity cache. |
| `EntityCacheMaxSize` | `int` | `100000` | Max entries for federation entity cache. |
| `CacheTTLSeconds` | `int` | `300` | TTL for query result cache. |
| `CacheMaxSize` | `int` | `100000` | Max entries for query cache. |
| `CacheMaxSizeBytes` | `int` | `52428800` | Max size (bytes) for query cache. |
| `MaxSubscriptionConnections` | `int` | `100` | Max concurrent subscription connections. |
| `SubscriptionTimeoutMs` | `int` | `30000` | Subscription connection timeout. |
| `HeartbeatIntervalMs` | `int` | `30000` | Heartbeat interval for subscriptions. |
| `DataLoaderBatchSize` | `int` | `100` | Batch size for DataLoader. |
| `DataLoaderDelayMs` | `int` | `50` | Delay before flushing DataLoader batches. |
| `EnableRemoteSchemaIntrospection` | `bool` | `true` | Allow remote schema introspection. |
| `RemoteSchemaTimeoutMs` | `int` | `30000` | Timeout for remote schema calls. |
| `LogInternalErrors` | `bool` | `true` | Log internal engine errors. |
| `IncludeDetailedErrorMessages` | `bool` | `false` | Include detailed errors in client responses. |

### 4. Validation

All options are validated automatically via DataAnnotations. Invalid values
cause the application to fail at startup with a clear list of validation
errors. The validator is registered in `DependencyInjection.cs` and runs as
part of the `IValidateOptions<T>` pipeline.

### 5. Sensitive defaults

No secret keys, passwords or connection strings are hard‑coded in the options.
All defaults are generic and safe for production; any environment‑specific
values (e.g., database connection strings, authentication credentials) must
be supplied through external configuration sources.

---


## QueryExecutionBenchmarks

The `QueryExecutionBenchmarks` class provides performance benchmarks for GraphQL query execution using BenchmarkDotNet. It measures execution time and memory allocation for various query patterns including simple queries, nested queries, complex multi-field queries, large queries with deep nesting, introspection queries, and queries with arguments.

This benchmark suite helps identify performance regressions and optimize query execution in the GraphQL engine.



### Usage Example

```csharp
using BenchmarkDotNet.Running;
using GraphQLEngine.Benchmarks;

// Run all benchmarks
var summary = BenchmarkRunner.Run<QueryExecutionBenchmarks>();

// Or run specific benchmarks
// BenchmarkRunner.Run<QueryExecutionBenchmarks>(config => config
//     .AddJob(Job.Dry) // Dry run to verify benchmarks
// );
```

### Example Benchmark Setup

```csharp
var benchmarks = new QueryExecutionBenchmarks();

// Initialize the benchmark environment
benchmarks.Setup();

// Execute a simple query benchmark
await benchmarks.SimpleQuery();

// Execute a nested query benchmark
await benchmarks.NestedQuery();

// Execute a complex multi-field query benchmark
await benchmarks.ComplexQuery();

// Execute a large query with deep nesting benchmark
await benchmarks.LargeQuery();

// Execute an introspection query benchmark
await benchmarks.IntrospectionQuery();

// Execute multiple simple queries benchmark
await benchmarks.MultipleSimpleQueries();

// Execute a query with arguments benchmark
await benchmarks.QueryWithArguments();

// Cleanup resources
benchmarks.Cleanup();
```

## QueryField

The `QueryField` class represents a selected field in a GraphQL query, including its nested selections and arguments. It's used internally for query parsing, execution, and complexity analysis. Each `QueryField` instance captures the field name, optional alias, type condition (for inline fragments), arguments, and nested fields.

### Usage Example

```csharp
using GraphQLEngine.Domain.Entities;

// Create a simple field with an alias
var userField = new QueryField(
    name: "user",
    alias: "currentUser"
);

// Create a field with arguments
var postsField = new QueryField(
    name: "posts",
    arguments: new[] { new QueryArgument("first", 10), new QueryArgument("orderBy", "DATE_DESC") }
);

// Create a field with nested selections
var nestedField = new QueryField(
    name: "user",
    fields: new[] {
        new QueryField(name: "id"),
        new QueryField(name: "name"),
        new QueryField(name: "email"),
        new QueryField(
            name: "posts",
            arguments: new[] { new QueryArgument("first", 5) },
            fields: new[] {
                new QueryField(name: "id"),
                new QueryField(name: "title"),
                new QueryField(name: "publishedDate")
            }
        )
    }
);

// Create an inline fragment field
var inlineFragmentField = new QueryField(
    name: "node",
    typeCondition: "User",
    fields: new[] {
        new QueryField(name: "id"),
        new QueryField(name: "name")
    }
);
```

## GraphQLException

The `GraphQLException` class serves as the base exception type for all GraphQL engine operations. It provides standardized error handling with support for custom error codes and extensible metadata through the `Extensions` dictionary. This exception hierarchy enables consistent error reporting across the entire GraphQL API surface.

### Usage Example

```csharp
using GraphQLEngine.Exceptions;

// Create a base GraphQL exception with custom error code
var exception = new GraphQLException("Invalid query syntax", "QUERY_SYNTAX_ERROR");

// Add custom extension data for client consumption
exception.AddExtension("query", "{ user { name }");
exception.AddExtension("timestamp", DateTime.UtcNow);

// Throw the exception
throw exception;

// Create a schema exception
var schemaException = new SchemaException("Type 'User' not found in schema");
throw schemaException;

// Create an execution exception with field context
var executionException = new ExecutionException(
    "Field 'user.posts' returned null",
    "user.posts",
    42
);
executionException.AddExtension("parentType", "User");
throw executionException;

// Create a query complexity exception
var complexityException = new QueryComplexityException(
    "Query complexity score 1250 exceeds maximum of 1000",
    1250,
    1000
);
throw complexityException;

// Create a validation exception with multiple errors
var validationException = new ValidationException(
    "Query validation failed",
    new List<string> {
        "Field 'user' must have at least one selection",
        "Argument 'id' is required"
    }
);
throw validationException;

// Create a data loader exception
var loaderException = new DataLoaderException(
    "Failed to load data for 'UserLoader'",
    "UserLoader"
);
throw loaderException;
```

## CollectionExtensions

The `CollectionExtensions` class provides a comprehensive set of extension methods for working with collections and enumerables in .NET. It includes utilities for checking collection state, safe item access, batching, filtering, transformation, and statistical operations, reducing boilerplate code and improving code readability.


### Usage Example

```csharp
using GraphQLEngine.Common.Utilities;

// Sample data
var users = new List<User> {
    new User { Id = 1, Name = "Alice", Age = 30 },
    new User { Id = 2, Name = "Bob", Age = 25 },
    new User { Id = 3, Name = "Charlie", Age = 30 }
};

// Check if collection is null or empty
if (users.IsNullOrEmpty())
{
    Console.WriteLine("No users found");
}

// Check if collection has items
if (users.HasItems())
{
    Console.WriteLine($"Found {users.Count} users");
}

// Get first item or null instead of throwing
var firstUser = users.FirstOrNull();

// Safely add items to a collection
var newUsers = new List<User>();
newUsers.AddIfNotNull(new User { Id = 4, Name = "Diana", Age = 28 });

// Add multiple items to a collection
var moreUsers = new List<User> { new User { Id = 5, Name = "Eve", Age = 22 } };
newUsers.AddRange(moreUsers);

// Remove multiple items from a collection
newUsers.RemoveRange(new[] { users[0], users[1] });

// Split a collection into batches
var userBatches = users.Batch(2);
foreach (var batch in userBatches)
{
    Console.WriteLine($"Batch with {batch.Count} users");
}

// Get distinct items by a key selector
var uniqueAges = users.DistinctBy(u => u.Age);

// Find index of an item
var index = users.IndexOf(users.First(u => u.Name == "Bob"));

// Execute action for each item
users.ForEach(u => Console.WriteLine(u.Name));

// Execute action with index
users.ForEachWithIndex((user, idx) => Console.WriteLine($"User {idx}: {user.Name}"));

// Check if all items match a value
var allAdults = users.All(new User { Age = 18 }); // false

// Combine multiple collections
var combined = users.Combine(new[] { new User { Id = 6, Name = "Frank", Age = 27 } });

// Get random item
var randomUser = users.Random();

// Shuffle collection
var shuffledUsers = users.Shuffle();

// Count items by key
var ageCounts = users.CountBy(u => u.Age);
// {30: 2, 25: 1}

// Convert to dictionary with default value
var userDict = users.ToDictionary(u => u.Id, "default");

// Sort by multiple keys
var sortedUsers = users.OrderByMany(u => u.Age, u => u.Name);

// Flatten nested collections
var nestedLists = new List<List<User>> {
    new List<User> { users[0], users[1] },
    new List<User> { users[2] }
};
var flattened = nestedLists.Flatten();

// Get median value
var ages = new List<int> { 22, 25, 27, 28, 30, 30 };
var medianAge = ages.Median(); // 27
```

## Quick Start

... *(rest of the original README continues unchanged)*
