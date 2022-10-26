# Frequently Asked Questions

## General Questions

### Q: What is dotnet-graphql-engine?

**A:** A production-grade, code-first GraphQL server for .NET 10. It lets you define GraphQL schemas entirely in C# without needing SDL files, with advanced features like schema stitching, DataLoader support, subscriptions, and query complexity analysis.

### Q: Why should I use this over other GraphQL libraries?

**A:** This engine provides:
- Pure code-first approach (no SDL files)
- Type-safe resolver definitions
- Built-in performance optimizations (caching, DataLoader, complexity analysis)
- Enterprise features (schema stitching, subscriptions)
- Zero external dependencies beyond Microsoft packages
- Lightweight and fast

### Q: Is it production-ready?

**A:** Yes. The engine has been designed for production use with comprehensive error handling, security features, and performance optimizations. All classes contain real business logic, not stubs.

### Q: What .NET versions does it support?

**A:** .NET 10 and later. We recommend using the latest stable .NET version.

### Q: Can I use this with existing GraphQL clients?

**A:** Yes. The engine implements standard GraphQL over HTTP, compatible with any GraphQL client library (Apollo Client, Relay, Urql, etc.).

## Installation & Setup

### Q: How do I install the engine?

**A:** 
```bash
# Option 1: NuGet (when available)
dotnet add package Sarmkadan.GraphQLEngine

# Option 2: Clone repository
git clone https://github.com/vladyslavzaiets/dotnet-graphql-engine.git
dotnet add reference dotnet-graphql-engine/dotnet-graphql-engine.csproj
```

### Q: What are the dependencies?

**A:** Only Microsoft packages:
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Logging
- System.Collections.Concurrent
- System.Reflection

### Q: How do I configure the engine?

**A:**
```csharp
services.AddGraphQLEngine(options =>
{
    options.MaxQueryComplexity = 5000;
    options.EnableCaching = true;
    options.EnableSubscriptions = true;
});
```

### Q: Can I use appsettings.json for configuration?

**A:** Yes:
```csharp
services.AddGraphQLEngine(builder.Configuration.GetSection("GraphQL"));
```

With appsettings.json:
```json
{
  "GraphQL": {
    "MaxQueryComplexity": 5000,
    "MaxQueryDepth": 10
  }
}
```

## Development

### Q: How do I define a GraphQL type?

**A:**
```csharp
var userType = new GraphQLType
{
    Name = "User",
    Description = "A user in the system",
    Fields = new List<GraphQLField>
    {
        new() { Name = "id", Type = "ID!", Description = "User ID" },
        new() { Name = "name", Type = "String!", Description = "User name" }
    }
};

schemaService.AddType("MyAPI", userType);
```

### Q: How do I create a resolver?

**A:** Resolvers are implicit based on field names and method signatures:

```csharp
public async Task<User> GetUserAsync(string id)
{
    return await _userService.GetUserAsync(id);
}
```

The method name matches the field name (case-insensitive).

### Q: How do I handle query variables?

**A:**
```csharp
var query = new GraphQLQuery("query GetUser($id: ID!) { user(id: $id) { name } }");
var variables = new Dictionary<string, object> { { "id", "123" } };
var context = await executionService.ExecuteAsync(query, variables);
```

### Q: Can I create custom directives?

**A:** Yes, implement `IGraphQLDirective`:

```csharp
public class AuthDirective : IGraphQLDirective
{
    public string Name => "@auth";
    public Task<object?> ApplyAsync(object? input, Dictionary<string, object> args)
    {
        // Custom logic
        return Task.FromResult(input);
    }
}
```

### Q: How do I handle errors?

**A:**
```csharp
try
{
    var context = await executionService.ExecuteAsync(query);
    if (context.Errors.Any())
    {
        foreach (var error in context.Errors)
        {
            Console.WriteLine($"{error.Message} at {error.Field}");
        }
    }
}
catch (GraphQLException ex)
{
    // Handle exception
}
```

## Performance

### Q: Why is my query slow?

**A:** Common causes:
1. **N+1 queries** - Use DataLoader for batch loading
2. **No caching** - Enable caching for repeated queries
3. **Complex query** - Reduce complexity or optimize resolvers
4. **Database issues** - Check database performance

### Q: What is query complexity?

**A:** A score representing query cost:
- Simple fields = 1 point
- Array fields = multiplied by list size
- Nested fields = accumulated

High-complexity queries are rejected to prevent DoS.

### Q: How do I use DataLoader?

**A:**
```csharp
dataLoaderService.RegisterBatchFunction("GetUsers",
    async (userIds) => await db.Users
        .Where(u => userIds.Contains(u.Id))
        .ToListAsync());

// In resolver
var user = await dataLoaderService.LoadAsync("GetUsers", "user-123");
```

### Q: How does caching work?

**A:** Query results are cached based on exact query string + variables:
- Cache key = Hash(query + variables)
- TTL = 5 minutes (configurable)
- LRU eviction when cache fills
- Automatically managed

### Q: Can I invalidate cache?

**A:**
```csharp
var cacheService = serviceProvider.GetRequiredService<ICacheService>();

// Remove specific key
await cacheService.RemoveAsync(cacheKey);

// Clear entire cache
await cacheService.ClearAsync();
```

## Schema Stitching

### Q: What is schema stitching?

**A:** Composing multiple GraphQL schemas into one unified schema. Allows querying across multiple GraphQL APIs transparently.

### Q: How do I stitch schemas?

**A:**
```csharp
var config = new SchemaStitchingConfig
{
    Enabled = true,
    BaseUrl = "http://api.example.com",
    DiscoveryEndpoint = "/graphql",
    Timeout = TimeSpan.FromSeconds(30)
};
var stitched = await schemaService.StitchSchemaAsync(mainSchema, config);
```

### Q: Can I stitch multiple schemas?

**A:** Yes, call `StitchSchemaAsync` multiple times with different configs.

### Q: What if remote schema is unavailable?

**A:** Stitching will fail. Implement retry logic and fallbacks:

```csharp
try
{
    var stitched = await schemaService.StitchSchemaAsync(schema, config);
}
catch (HttpRequestException)
{
    // Use fallback schema or cache
}
```

## Subscriptions

### Q: What are subscriptions?

**A:** Real-time connections that push data to clients when things change, using WebSocket.

### Q: How do I create a subscription?

**A:**
```csharp
subscriptionService.Subscribe(clientId, "UserUpdated", async (update) =>
{
    Console.WriteLine($"User updated: {update}");
});
```

### Q: How do I publish to subscribers?

**A:**
```csharp
var eventBus = serviceProvider.GetRequiredService<EventBus>();
await eventBus.PublishAsync("UserUpdated", new { id = "1", name = "Jane" });
```

### Q: Are subscriptions persistent?

**A:** No, they're connection-based. Reconnection required after disconnect.

## Deployment

### Q: How do I deploy to production?

**A:**
```bash
dotnet publish -c Release -o ./publish
cd publish
ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS=http://0.0.0.0:5000 \
  dotnet dotnet-graphql-engine.dll
```

### Q: How do I use Docker?

**A:**
```bash
docker build -t myapp .
docker run -p 5000:5000 myapp
```

### Q: Can I use Kubernetes?

**A:** Yes, see the deployment guide for K8s manifests.

### Q: What resources does it need?

**A:**
- Minimum: 1GB RAM, single core
- Recommended: 2GB RAM, 2-4 cores
- Storage: Depends on cache size

### Q: How do I scale horizontally?

**A:** Run multiple instances behind a load balancer. No shared state issues.

## Security

### Q: Is it secure?

**A:** Yes, with proper configuration:
- Query complexity limits prevent DoS
- Input validation in resolvers
- Authentication via middleware
- HTTPS/TLS support
- Rate limiting support

### Q: How do I add authentication?

**A:**
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* config */ });

// Middleware
app.UseAuthentication();
app.UseAuthorization();
```

### Q: How do I check user info in resolvers?

**A:**
```csharp
public async Task<User> GetUserAsync(ExecutionContext context)
{
    var userId = context.GetHeader("X-User-Id");
    var isAdmin = context.GetData("isAdmin");
    // ...
}
```

### Q: Can I do field-level authorization?

**A:** Yes, check permissions in resolvers:

```csharp
public async Task<User> GetUserAsync(string id, ExecutionContext context)
{
    if (!context.GetData("isAdmin") is bool admin || !admin)
        throw new GraphQLException("Not authorized");
    return await _userService.GetUserAsync(id);
}
```

## Troubleshooting

### Q: I get "Type not found" error

**A:** Make sure you added the type to schema:
```csharp
schemaService.AddType("MyAPI", userType);
```

### Q: Queries are slow

**A:** 
1. Check query complexity
2. Enable DataLoader
3. Enable caching
4. Profile resolvers
5. Check database performance

### Q: Memory usage is high

**A:**
```csharp
options.CacheMaxSizeBytes = 26214400;  // 25 MB
options.CacheTtlSeconds = 60;          // 1 minute
```

### Q: Port 5000 is in use

**A:**
```bash
# Change port
dotnet run --urls http://localhost:5001
```

### Q: WebSocket connections fail

**A:** Check:
1. Reverse proxy WebSocket support
2. Timeout settings
3. Firewall rules
4. Port accessibility

### Q: Cache not working

**A:** Verify:
1. `EnableCaching = true`
2. Query is identical (including spacing)
3. Variables are identical
4. Cache TTL hasn't expired

## Contributing

### Q: How do I contribute?

**A:**
1. Fork repository
2. Create feature branch
3. Make changes
4. Add tests
5. Submit pull request

See CONTRIBUTING.md in repository root.

### Q: What's the code style?

**A:**
- PascalCase for classes/methods
- camelCase for variables
- Nullable reference types enabled
- XML documentation for public APIs
- Max 30 lines per method

### Q: How do I run tests?

**A:**
```bash
dotnet test
```

## Support

### Q: Where do I get help?

**A:** 
- 📖 [Documentation](https://github.com/vladyslavzaiets/dotnet-graphql-engine)
- 💬 [GitHub Issues](https://github.com/vladyslavzaiets/dotnet-graphql-engine/issues)
- 📧 [Email](mailto:rutova2@gmail.com)

### Q: How do I report bugs?

**A:** Open a GitHub issue with:
1. Description of bug
2. Steps to reproduce
3. Expected vs actual behavior
4. Environment (OS, .NET version, etc.)

### Q: Can I request features?

**A:** Yes, open a GitHub discussion or issue describing the feature.

### Q: Is there commercial support?

**A:** Contact for more information: rutova2@gmail.com
