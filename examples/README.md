# dotnet-graphql-engine Examples

This directory contains practical examples demonstrating various features of the dotnet-graphql-engine.

## Available Examples

### 1. [Basic API](./1-basic-api.cs)

**What you'll learn:**
- Creating a simple GraphQL API from scratch
- Defining types with fields and arguments
- Setting up queries and mutations
- Executing queries

**Use case:** Getting started with GraphQL Engine

**Time to run:** 5 minutes

```csharp
// Define a User type
var userType = new GraphQLType
{
    Name = "User",
    Fields = new List<GraphQLField>
    {
        new() { Name = "id", Type = "ID!", Description = "User ID" },
        new() { Name = "name", Type = "String!", Description = "User name" }
    }
};

// Execute a query
var query = new GraphQLQuery("{ users { id name } }");
var context = await executionService.ExecuteAsync(query);
```

### 2. [DataLoader Batching](./2-dataloader-batching.cs)

**What you'll learn:**
- Preventing N+1 query problems
- Batch loading data efficiently
- Registering batch functions
- Performance benefits of batching

**Use case:** Building high-performance GraphQL APIs with complex data relationships

**Performance gain:** 100x reduction in database queries

```csharp
// Register batch function
dataLoaderService.RegisterBatchFunction("GetPostsByUserIds",
    async (userIds) =>
    {
        // Single query for all userIds
        return await db.Posts
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync();
    });

// Use in resolvers - automatic batching
var post = await dataLoaderService.LoadAsync("GetPostsByUserIds", userId);
```

### 3. [Complexity Analysis](./3-complexity-analysis.cs)

**What you'll learn:**
- Analyzing query complexity
- Setting complexity limits
- Preventing DoS attacks
- Complexity classifications

**Use case:** Protecting your API from expensive queries

```csharp
var analysis = analysisService.AnalyzeQuery(query);
Console.WriteLine($"Complexity: {analysis.TotalComplexity}");
Console.WriteLine($"Depth: {analysis.MaxDepth}");
Console.WriteLine($"Level: {analysis.Level}");  // LOW, MEDIUM, HIGH, CRITICAL

if (!analysisService.IsQueryAllowed(query))
{
    throw new GraphQLException("Query exceeds complexity limits");
}
```

### 4. [Real-Time Subscriptions](./4-subscriptions.cs)

**What you'll learn:**
- Creating WebSocket connections for real-time data
- Subscribing to events
- Publishing events
- Managing subscription lifecycle

**Use case:** Building real-time applications (notifications, live feeds, collaborative editing)

```csharp
// Create subscription connection
var connection = subscriptionService.CreateConnection(clientId, query);

// Subscribe to events
subscriptionService.Subscribe(clientId, "UserUpdated", async (update) =>
{
    Console.WriteLine($"User updated: {update}");
});

// Publish events
await eventBus.PublishAsync("UserUpdated", userData);
```

### 5. [Schema Stitching](./5-schema-stitching.cs)

**What you'll learn:**
- Composing multiple GraphQL APIs
- Creating unified schema
- Cross-service querying
- Federation patterns

**Use case:** Building API gateways and unified schema from microservices

```csharp
var config = new SchemaStitchingConfig
{
    Enabled = true,
    BaseUrl = "http://user-service.local",
    DiscoveryEndpoint = "/graphql"
};

var stitched = await schemaService.StitchSchemaAsync(mainSchema, config);

// Now query across services
var query = @"
{
    user(id: ""1"") {
        id
        name
        posts {      // From different service
            id
            title
        }
    }
}";
```

### 6. [Authentication & Error Handling](./6-auth-error-handling.cs)

**What you'll learn:**
- Implementing authentication in resolvers
- Handling authorization
- Proper error handling
- Error formatting for different environments

**Use case:** Securing GraphQL APIs with proper authentication and error handling

```csharp
public async Task<User> GetUserAsync(string id, ExecutionContext context)
{
    // Get user from context
    var userId = context.GetHeader("X-User-Id");
    var isAdmin = context.GetData("isAdmin") is bool b && b;

    // Check permission
    if (userId != id && !isAdmin)
        throw new GraphQLException("Not authorized");

    return await _userService.GetUserAsync(id);
}
```

### 7. [Caching & Performance](./7-caching-performance.cs)

**What you'll learn:**
- Query result caching
- Cache hit/miss patterns
- Cache invalidation
- Performance monitoring
- Cache statistics

**Use case:** Optimizing query performance with intelligent caching

**Performance improvement:** 50-100x faster for cached queries

```csharp
// Caching is automatic, but you can monitor it
var stats = cacheService.GetStatistics();
Console.WriteLine($"Hit rate: {stats.HitRate:P}");
Console.WriteLine($"Cache size: {stats.SizeBytes / 1024 / 1024}MB");

// Manual cache control
await cacheService.RemoveAsync(cacheKey);
await cacheService.ClearAsync();
```

## Running the Examples

### Option 1: As Part of Your Application

Copy the relevant example code into your application:

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGraphQLEngine(options => { /* options */ });

// Copy example code here
var schemaService = builder.Services.GetRequiredService<SchemaService>();
// ... setup schema ...

var app = builder.Build();
app.Run();
```

### Option 2: Standalone

Create a new console application:

```bash
dotnet new console -n MyGraphQLApp
cd MyGraphQLApp
dotnet add package Sarmkadan.GraphQLEngine

# Copy example code to Program.cs
# Modify to call specific example

dotnet run
```

### Option 3: In Tests

Use examples in unit tests:

```csharp
[Test]
public async Task DataLoaderExample()
{
    var serviceProvider = new ServiceCollection()
        .AddGraphQLEngine()
        .BuildServiceProvider();

    // Run DataLoaderExample.Run(serviceProvider);
}
```

## Example Progression

We recommend exploring examples in this order:

1. **Start here:** Example 1 (Basic API)
   - Understand fundamental concepts
   - See how to define types and queries

2. **Add complexity:** Example 3 (Complexity Analysis)
   - Learn about query complexity
   - Understand limits and protection

3. **Improve performance:** Example 2 (DataLoader) + Example 7 (Caching)
   - Optimize queries
   - Learn batch operations and caching strategies

4. **Add features:** Example 4 (Subscriptions)
   - Enable real-time capabilities
   - Understand event-driven architecture

5. **Scale up:** Example 5 (Schema Stitching)
   - Compose multiple services
   - Build API gateway

6. **Production-ready:** Example 6 (Auth & Error Handling)
   - Implement security
   - Handle errors gracefully

## Common Patterns

### Query with Variables

```csharp
var query = new GraphQLQuery("query GetUser($id: ID!) { user(id: $id) { name } }");
var variables = new Dictionary<string, object> { { "id", "123" } };
var context = await executionService.ExecuteAsync(query, variables);
```

### Error Handling

```csharp
var context = await executionService.ExecuteAsync(query);
if (context.Errors.Any())
{
    foreach (var error in context.Errors)
    {
        Console.WriteLine($"{error.Message} at {error.Field}");
    }
}
```

### Type-Safe Field Resolution

```csharp
var userType = new GraphQLType
{
    Name = "User",
    Fields = new List<GraphQLField>
    {
        new() { Name = "id", Type = "ID!", Description = "User ID" },
        new() { Name = "name", Type = "String!", Description = "User name" },
        new() { Name = "email", Type = "String", Description = "User email" }
    }
};
```

## Troubleshooting

### "Type not found" error
Make sure you've added the type to schema:
```csharp
schemaService.AddType("MyAPI", userType);
```

### Query returns null
Check that resolver is returning correct type and isn't throwing exception.

### Cache not working
Verify `EnableCaching = true` and query is identical (including whitespace).

### Slow performance
- Use DataLoader for relationships
- Check query complexity
- Enable caching
- Profile database queries

## Performance Benchmarks

From the examples:

| Operation | Time | Improvement |
|-----------|------|-------------|
| First query (cold) | 50-100ms | Baseline |
| Cached query | <1ms | 50-100x faster |
| Batched operation (100 items) | 5ms | Sequential would be 500ms+ |
| Complex query (no cache) | 100ms | - |
| Complex query (cached) | <1ms | 100x+ faster |

## Next Steps

1. **Choose an example** matching your use case
2. **Adapt code** to your domain model
3. **Test thoroughly** with your data
4. **Monitor performance** using built-in metrics
5. **Scale confidently** with proper limits and caching

## Getting Help

- **Questions:** Check the [FAQ](../docs/faq.md)
- **Issues:** Report on [GitHub](https://github.com/vladyslavzaiets/dotnet-graphql-engine/issues)
- **Documentation:** See [docs/](../docs/) directory
- **Email:** rutova2@gmail.com

---

**Each example is production-ready code you can use in real applications.**
Adapt the patterns to your specific needs and domain model.
