# API Reference

Complete reference for all public APIs in dotnet-graphql-engine.

## HTTP Endpoints

### POST /graphql

Execute GraphQL queries and mutations.

**Request:**
```json
{
  "query": "{ user(id: \"1\") { id name email } }",
  "variables": { "userId": "1" },
  "operationName": "GetUser"
}
```

**Response (Success):**
```json
{
  "data": {
    "user": {
      "id": "1",
      "name": "John Doe",
      "email": "john@example.com"
    }
  }
}
```

**Response (Error):**
```json
{
  "errors": [
    {
      "message": "User not found",
      "extensions": {
        "field": "user",
        "complexity": 50
      }
    }
  ]
}
```

**Query Parameters:**
- `query` (string, required) - GraphQL query string
- `variables` (object, optional) - Query variables
- `operationName` (string, optional) - Operation name for multi-op documents

**Headers:**
- `Content-Type: application/json` (required)
- `X-User-Id` (optional) - User identifier
- `Authorization` (optional) - Authentication token

**Status Codes:**
- `200 OK` - Query executed successfully
- `400 Bad Request` - Invalid query or configuration
- `401 Unauthorized` - Authentication required
- `429 Too Many Requests` - Rate limit exceeded
- `500 Internal Server Error` - Server error

### GET /graphql/schema

Get the GraphQL schema in SDL (Schema Definition Language) format.

**Response:**
```graphql
type User {
  id: ID!
  name: String!
  email: String
  createdAt: DateTime!
}

type Query {
  user(id: ID!): User
  users: [User!]!
}
```

**Query Parameters:**
- `format` (string, optional) - Response format: `sdl` (default) or `json`

### GET /health

Health check endpoint.

**Response (Healthy):**
```json
{
  "status": "Healthy",
  "timestamp": "2026-05-04T10:30:00Z",
  "components": {
    "cache": "Healthy",
    "database": "Healthy",
    "subscriptions": "Healthy"
  }
}
```

**Response (Degraded):**
```json
{
  "status": "Degraded",
  "timestamp": "2026-05-04T10:30:00Z",
  "components": {
    "cache": "Healthy",
    "database": "Unhealthy",
    "subscriptions": "Healthy"
  }
}
```

## Service APIs

### GraphQLExecutionService

Primary service for executing queries.

```csharp
public interface IGraphQLExecutionService
{
    Task<ExecutionContext> ExecuteAsync(GraphQLQuery query);
    Task<ExecutionContext> ExecuteAsync(GraphQLQuery query, Dictionary<string, object> variables);
    Task<ExecutionContext> ExecuteAsync(GraphQLQuery query, ExecutionContext context);
}
```

**Methods:**

#### ExecuteAsync(GraphQLQuery query)

Execute a GraphQL query without variables.

```csharp
var query = new GraphQLQuery("{ users { id name } }");
var context = await executionService.ExecuteAsync(query);

if (context.Errors.Any())
{
    foreach (var error in context.Errors)
    {
        Console.WriteLine(error.Message);
    }
}
else
{
    var result = context.Data["users"];
}
```

#### ExecuteAsync(GraphQLQuery query, Dictionary<string, object> variables)

Execute with query variables.

```csharp
var query = new GraphQLQuery("query GetUser($id: ID!) { user(id: $id) { name } }");
var variables = new Dictionary<string, object> { { "id", "123" } };
var context = await executionService.ExecuteAsync(query, variables);
```

#### ExecuteAsync(GraphQLQuery query, ExecutionContext context)

Execute with pre-configured context.

```csharp
var context = new ExecutionContext
{
    ExecutionId = Guid.NewGuid().ToString(),
    Data = new Dictionary<string, object>
    {
        { "userId", "123" },
        { "isAdmin", true }
    }
};
var result = await executionService.ExecuteAsync(query, context);
```

### SchemaService

Manage GraphQL schema and types.

```csharp
public interface ISchemaService
{
    GraphQLSchema CreateSchema(string name);
    void AddType(string schemaName, GraphQLType type);
    string ExportAsSDL(string schemaName);
    GraphQLType? GetType(string schemaName, string typeName);
    IEnumerable<GraphQLType> GetAllTypes(string schemaName);
    Task<GraphQLSchema> StitchSchemaAsync(GraphQLSchema schema, SchemaStitchingConfig config);
}
```

**Methods:**

#### CreateSchema(string name)

Create a new schema.

```csharp
var schema = schemaService.CreateSchema("MyAPI");
```

#### AddType(string schemaName, GraphQLType type)

Register a type in the schema.

```csharp
var userType = new GraphQLType
{
    Name = "User",
    Fields = new List<GraphQLField> { ... }
};
schemaService.AddType("MyAPI", userType);
```

#### ExportAsSDL(string schemaName)

Export schema as SDL string.

```csharp
var sdl = schemaService.ExportAsSDL("MyAPI");
Console.WriteLine(sdl);
// Output:
// type User {
//   id: ID!
//   name: String!
// }
```

#### GetType(string schemaName, string typeName)

Get a specific type.

```csharp
var userType = schemaService.GetType("MyAPI", "User");
if (userType != null)
{
    Console.WriteLine($"Type {userType.Name} has {userType.Fields.Count} fields");
}
```

#### GetAllTypes(string schemaName)

Get all types in schema.

```csharp
var allTypes = schemaService.GetAllTypes("MyAPI");
foreach (var type in allTypes)
{
    Console.WriteLine(type.Name);
}
```

#### StitchSchemaAsync(GraphQLSchema schema, SchemaStitchingConfig config)

Compose multiple GraphQL APIs.

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

### QueryAnalysisService

Analyze and limit query complexity.

```csharp
public interface IQueryAnalysisService
{
    QueryComplexity AnalyzeQuery(GraphQLQuery query);
    bool IsQueryAllowed(GraphQLQuery query);
    void SetMaxComplexity(int maxComplexity);
    void SetMaxDepth(int maxDepth);
    void SetMaxFields(int maxFields);
}
```

**Methods:**

#### AnalyzeQuery(GraphQLQuery query)

Analyze query complexity.

```csharp
var query = new GraphQLQuery("{ user { posts { comments { text } } } }");
var analysis = analysisService.AnalyzeQuery(query);

Console.WriteLine($"Complexity: {analysis.TotalComplexity}");
Console.WriteLine($"Depth: {analysis.MaxDepth}");
Console.WriteLine($"Level: {analysis.Level}");
// Output:
// Complexity: 150
// Depth: 3
// Level: MEDIUM
```

#### IsQueryAllowed(GraphQLQuery query)

Check if query passes complexity limits.

```csharp
if (analysisService.IsQueryAllowed(query))
{
    await executionService.ExecuteAsync(query);
}
else
{
    throw new GraphQLException("Query exceeds complexity limits");
}
```

#### SetMaxComplexity(int maxComplexity)

Set maximum complexity score.

```csharp
analysisService.SetMaxComplexity(5000);
```

#### SetMaxDepth(int maxDepth)

Set maximum query depth.

```csharp
analysisService.SetMaxDepth(10);
```

#### SetMaxFields(int maxFields)

Set maximum field count.

```csharp
analysisService.SetMaxFields(200);
```

### DataLoaderService

Batch load data to prevent N+1 queries.

```csharp
public interface IDataLoaderService
{
    void RegisterBatchFunction<T, TResult>(
        string name, 
        Func<IEnumerable<T>, Task<IEnumerable<TResult>>> batchFunction,
        int? batchSize = null);
    
    Task<object> LoadAsync(string batchName, object key);
    Task<IEnumerable<object>> LoadManyAsync(string batchName, IEnumerable<object> keys);
    Task FlushAsync();
}
```

**Methods:**

#### RegisterBatchFunction<T, TResult>(string name, Func<...>, int?)

Register a batch function.

```csharp
dataLoaderService.RegisterBatchFunction<string, User>(
    "GetUsers",
    async (userIds) =>
    {
        return await db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();
    },
    batchSize: 100);
```

#### LoadAsync(string batchName, object key)

Load a single item.

```csharp
var user = await dataLoaderService.LoadAsync("GetUsers", "user-123");
```

#### LoadManyAsync(string batchName, IEnumerable<object> keys)

Load multiple items.

```csharp
var userIds = new[] { "1", "2", "3" };
var users = await dataLoaderService.LoadManyAsync("GetUsers", userIds);
```

#### FlushAsync()

Execute pending batches.

```csharp
await dataLoaderService.FlushAsync();
```

### SubscriptionService

Manage real-time subscriptions.

```csharp
public interface ISubscriptionService
{
    SubscriptionConnection CreateConnection(string clientId, string query);
    void Subscribe(string clientId, string eventType, Func<object, Task> handler);
    void Unsubscribe(string clientId, string eventType);
    int GetActiveSubscriptionCount();
    void CloseConnection(string clientId);
}
```

**Methods:**

#### CreateConnection(string clientId, string query)

Create a subscription connection.

```csharp
var connection = subscriptionService.CreateConnection(clientId, subscriptionQuery);
```

#### Subscribe(string clientId, string eventType, Func<object, Task> handler)

Subscribe to an event.

```csharp
subscriptionService.Subscribe(clientId, "UserUpdated", async (update) =>
{
    Console.WriteLine($"User updated: {update}");
    await SendToClientAsync(clientId, update);
});
```

#### Unsubscribe(string clientId, string eventType)

Unsubscribe from an event.

```csharp
subscriptionService.Unsubscribe(clientId, "UserUpdated");
```

#### GetActiveSubscriptionCount()

Get count of active subscriptions.

```csharp
var count = subscriptionService.GetActiveSubscriptionCount();
Console.WriteLine($"Active subscriptions: {count}");
```

#### CloseConnection(string clientId)

Close a subscription connection.

```csharp
subscriptionService.CloseConnection(clientId);
```

### CacheService

Manage query result caching.

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
    Task RemoveAsync(string key);
    Task ClearAsync();
    CacheStatistics GetStatistics();
}
```

**Methods:**

#### GetAsync<T>(string key)

Retrieve cached value.

```csharp
var cached = await cacheService.GetAsync<ExecutionContext>("query-key-123");
if (cached != null)
{
    return cached;
}
```

#### SetAsync<T>(string key, T value, TimeSpan? ttl)

Cache a value.

```csharp
await cacheService.SetAsync("query-key-123", executionContext, TimeSpan.FromMinutes(5));
```

#### RemoveAsync(string key)

Remove a cached value.

```csharp
await cacheService.RemoveAsync("query-key-123");
```

#### ClearAsync()

Clear entire cache.

```csharp
await cacheService.ClearAsync();
```

#### GetStatistics()

Get cache statistics.

```csharp
var stats = cacheService.GetStatistics();
Console.WriteLine($"Hit rate: {stats.HitRate:P}");
Console.WriteLine($"Size: {stats.SizeBytes / 1024 / 1024}MB");
```

## Domain Models

### GraphQLQuery

Represents a GraphQL query.

```csharp
public class GraphQLQuery
{
    public string QueryString { get; set; }
    public string? OperationName { get; set; }
    public DateTime ReceivedAt { get; set; }
}
```

### ExecutionContext

Execution context with results and errors.

```csharp
public class ExecutionContext
{
    public string ExecutionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration { get; }
    public List<ExecutionError> Errors { get; set; }
    public Dictionary<string, object> Variables { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public Dictionary<string, object> Extensions { get; set; }
}
```

### ExecutionError

Error information.

```csharp
public class ExecutionError
{
    public string Message { get; set; }
    public string? Field { get; set; }
    public int? LineNumber { get; set; }
    public int? ColumnNumber { get; set; }
    public List<object>? Path { get; set; }
    public Dictionary<string, object> Extensions { get; set; }
}
```

### GraphQLType

Type definition.

```csharp
public class GraphQLType
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Kind { get; set; }
    public List<GraphQLField> Fields { get; set; }
    public List<string> Interfaces { get; set; }
}
```

### GraphQLField

Field definition.

```csharp
public class GraphQLField
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string? Description { get; set; }
    public List<GraphQLField> Arguments { get; set; }
    public Dictionary<string, object> Directives { get; set; }
}
```

### QueryComplexity

Complexity analysis result.

```csharp
public class QueryComplexity
{
    public int TotalComplexity { get; set; }
    public int MaxDepth { get; set; }
    public int FieldCount { get; set; }
    public ComplexityLevel Level { get; set; }
    public Dictionary<string, int> FieldComplexities { get; set; }
}
```

## Dependency Injection

### Service Registration

Register all services:

```csharp
services.AddGraphQLEngine(options =>
{
    // Configuration
});
```

### Manual Registration

Register services individually:

```csharp
services.AddScoped<IGraphQLExecutionService, GraphQLExecutionService>();
services.AddScoped<ISchemaService, SchemaService>();
services.AddScoped<IQueryAnalysisService, QueryAnalysisService>();
services.AddScoped<IDataLoaderService, DataLoaderService>();
services.AddScoped<ICacheService, DistributedCacheService>();
services.AddScoped<ISubscriptionService, SubscriptionService>();
```

### Service Resolution

Resolve from dependency injection container:

```csharp
var executionService = serviceProvider.GetRequiredService<IGraphQLExecutionService>();
var schemaService = serviceProvider.GetRequiredService<ISchemaService>();
var analysisService = serviceProvider.GetRequiredService<IQueryAnalysisService>();
```

## Exceptions

### GraphQLException

Base GraphQL exception.

```csharp
public class GraphQLException : Exception
{
    public GraphQLException(string message);
    public GraphQLException(string message, Exception? innerException);
}
```

### Specific Exceptions

```csharp
public class GraphQLValidationException : GraphQLException { }
public class GraphQLExecutionException : GraphQLException { }
public class GraphQLComplexityException : GraphQLException { }
public class GraphQLAuthenticationException : GraphQLException { }
public class GraphQLAuthorizationException : GraphQLException { }
```

## Configuration Models

### GraphQLEngineOptions

```csharp
public class GraphQLEngineOptions
{
    public int MaxQueryComplexity { get; set; }
    public int MaxQueryDepth { get; set; }
    public int MaxQueryFields { get; set; }
    public int QueryTimeoutMs { get; set; }
    public int MaxBatchSize { get; set; }
    public bool EnableCaching { get; set; }
    public int CacheTtlSeconds { get; set; }
    public long CacheMaxSizeBytes { get; set; }
    public bool EnableSubscriptions { get; set; }
    public bool EnableMetrics { get; set; }
    public bool IncludeDetailedErrorMessages { get; set; }
}
```

### SchemaStitchingConfig

```csharp
public class SchemaStitchingConfig
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; }
    public string DiscoveryEndpoint { get; set; }
    public TimeSpan Timeout { get; set; }
}
```

### SubscriptionConfig

```csharp
public class SubscriptionConfig
{
    public bool Enabled { get; set; }
    public int MaxConnections { get; set; }
    public TimeSpan KeepAliveInterval { get; set; }
}
```
