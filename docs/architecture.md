# Architecture Guide

This document describes the internal architecture of dotnet-graphql-engine.

## System Design

### Layered Architecture

The engine is organized in clean layers with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────────┐
│                      HTTP/WebSocket Layer                        │
│         GraphQLController, HealthCheckController, etc.          │
│                      Middleware Pipeline                         │
│       Authentication, Logging, Rate Limiting, Error Handling    │
└────────────────────┬────────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────────┐
│                  GraphQL Execution Engine                        │
│  GraphQLExecutionService (Query Parsing, Validation, Execution) │
│           QueryAnalysisService (Complexity Analysis)            │
│                  CacheService (Result Caching)                  │
└────────────────────┬────────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────────┐
│                    Resolution Layer                              │
│        SchemaService (Type Definition & Resolution)              │
│          DataLoaderService (Batch Loading)                      │
│         ErrorFormattingService (Error Standardization)          │
└────────────────────┬────────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────────┐
│                   Data Access Layer                              │
│           Repository Pattern (IRepository<T>)                    │
│      External APIs (Schema Stitching, Webhooks)                 │
│            Subscriptions & Event System                         │
└─────────────────────────────────────────────────────────────────┘
```

## Core Components

### 1. GraphQL Execution Service

**Location:** `src/Services/GraphQL/GraphQLExecutionService.cs`

Responsible for:
- Parsing GraphQL queries and mutations
- Validating against schema
- Executing resolvers
- Managing execution context
- Collecting errors

**Key Methods:**
```csharp
Task<ExecutionContext> ExecuteAsync(GraphQLQuery query)
Task<ExecutionContext> ExecuteAsync(GraphQLQuery query, Dictionary<string, object> variables)
```

**Internal Flow:**
1. Parse query string into GraphQLQuery object
2. Validate query against schema
3. Analyze query complexity
4. Check cache for result
5. Execute resolvers for each field
6. Format result and errors
7. Cache result if configured

### 2. Schema Service

**Location:** `src/Services/Schema/SchemaService.cs`

Manages:
- Schema creation and registration
- Type definitions
- Field resolution
- Type introspection
- Schema stitching

**Key Methods:**
```csharp
GraphQLSchema CreateSchema(string name)
void AddType(string schemaName, GraphQLType type)
string ExportAsSDL(string schemaName)
Task<GraphQLSchema> StitchSchemaAsync(GraphQLSchema schema, SchemaStitchingConfig config)
```

**Type Registry:**
- Maintains in-memory type registry
- Supports multiple named schemas
- Fast O(1) type lookups
- Lazy-loaded remote schemas

### 3. Query Analysis Service

**Location:** `src/Services/QueryAnalysis/QueryAnalysisService.cs`

Performs:
- Complexity scoring (by field and total)
- Depth analysis (maximum nesting level)
- Field count validation
- Complexity classification (LOW, MEDIUM, HIGH, CRITICAL)

**Algorithm:**
```
For each field in query:
  - Assign base complexity (default 1)
  - Multiply by list multiplier if returns array
  - Add nested field complexities
  - Track maximum depth
```

**Classifications:**
- **LOW** (0-1000): Simple queries, execute immediately
- **MEDIUM** (1000-3000): Standard queries, apply caching
- **HIGH** (3000-6000): Complex queries, may timeout
- **CRITICAL** (>6000): Rejected by default

### 4. DataLoader Service

**Location:** `src/Services/DataLoader/DataLoaderService.cs`

Prevents N+1 queries through:
- Request batching within execution scope
- Automatic batch function invocation
- Result memoization
- Configurable batch sizes

**How It Works:**

```
Resolver A requests item "1" ─┐
Resolver B requests item "2" ├─→ Batched Request
Resolver C requests item "3" ┴─→ [1, 2, 3]
                                    │
                            Batch Function (Single DB Call)
                                    │
                          Results ["item1", "item2", "item3"]
```

**Registration:**
```csharp
dataLoaderService.RegisterBatchFunction("GetUsers", 
    async (ids) => await db.Users.Where(u => ids.Contains(u.Id)).ToListAsync());
```

### 5. Cache Service

**Location:** `src/Services/Caching/DistributedCacheService.cs`

Manages:
- Query result caching
- Schema caching
- LRU eviction policy
- Configurable TTL and size limits

**Cache Key Generation:**
```
CacheKey = Hash(query + serialized(variables))
```

**Eviction Policy:**
- LRU (Least Recently Used)
- Time-based expiration (TTL)
- Size-based eviction (max bytes)

### 6. Subscription Service

**Location:** `src/Services/Subscriptions/SubscriptionService.cs`

Handles:
- WebSocket connection management
- Subscription registration
- Event delivery
- Connection cleanup

**Architecture:**
```
Client ──WebSocket──> ConnectionManager
                           │
                    ┌──────┼──────┐
                    │      │      │
                Handlers  Events  Cleanup
```

## Data Models

### ExecutionContext

Central object passed through execution:

```csharp
public class ExecutionContext
{
    public string ExecutionId { get; }          // Unique execution ID
    public DateTime StartTime { get; }          // When execution started
    public DateTime? EndTime { get; }           // When execution ended
    public TimeSpan Duration { get; }           // Total duration
    public List<ExecutionError> Errors { get; } // Collected errors
    public Dictionary<string, object> Variables { get; }  // Query variables
    public Dictionary<string, object> Data { get; }       // Execution data
}
```

### GraphQLSchema

The schema representation:

```csharp
public class GraphQLSchema
{
    public string Name { get; }                 // Schema name
    public GraphQLType QueryType { get; }       // Root query type
    public GraphQLType? MutationType { get; }   // Root mutation type
    public GraphQLType? SubscriptionType { get; }  // Root subscription type
    public Dictionary<string, GraphQLType> TypeRegistry { get; }  // All types
}
```

### GraphQLType

Type definition:

```csharp
public class GraphQLType
{
    public string Name { get; }                 // Type name
    public string? Description { get; }         // Documentation
    public string Kind { get; }                 // OBJECT, SCALAR, ENUM, etc.
    public List<GraphQLField> Fields { get; }  // Type fields
    public List<string> Interfaces { get; }    // Implemented interfaces
}
```

## Service Flow

### Query Execution Flow

```
Request ─────────────────────┐
                              │
                    Parse Query
                              │
                    Validate Query
                              │
                 Analyze Complexity
                              │
              Check Query Allowed?  ──(Rejected)──→ Error Response
                              │
                              ├──(Cached)──→ Return Cached Result
                              │
                    Build Execution Context
                              │
                      Execute Resolvers
                              │
                    Collect Results & Errors
                              │
                       Format Response
                              │
                     Cache Result (if enabled)
                              │
                   Return Response to Client
```

### Resolver Execution

```
Field Resolution:
1. Get field definition from schema
2. Get resolver function (or use default)
3. Prepare arguments from query
4. Call resolver with context
5. Process returned value
6. If array, call resolver for each item
7. If object, resolve nested fields
8. Collect any errors
9. Return resolved value
```

## Middleware Pipeline

```
Request
  ↓
[LoggingMiddleware] ──→ Log request details
  ↓
[AuthenticationMiddleware] ──→ Verify authentication
  ↓
[RateLimitingMiddleware] ──→ Check rate limits
  ↓
[ErrorHandlingMiddleware] ──→ Catch exceptions
  ↓
[Controller/Handler]
  ↓
Response ←─ [ErrorHandlingMiddleware] (format errors)
```

## Performance Considerations

### 1. Query Caching Strategy

```
Query Execution:
┌─────────────────┐
│ Exact Query Hit?│
├─────────────────┤
│  YES → Return   │
│       Cached    │
│  NO → Execute   │
└─────────────────┘
       │
       ↓
Cache Key = Hash(QueryString + Variables)
```

**Benefits:**
- Eliminates redundant executions
- Reduces database load
- Improves response time

### 2. DataLoader Batching

```
Without DataLoader (N+1):
for each user:
  db.query(posts where user_id = id)  // N queries

With DataLoader:
ids = [1,2,3,4,5]
db.query(posts where user_id in (1,2,3,4,5))  // 1 query
```

### 3. Schema Compilation

Schemas are compiled once and cached:
```
First Introspection Query → Compile Schema → Cache
Subsequent Introspection → Return Cached Schema
```

## Extensibility Points

### 1. Custom Resolvers

Implement `IFieldResolver` to customize field resolution:

```csharp
public class CustomResolver : IFieldResolver
{
    public Task<object?> ResolveAsync(GraphQLField field, object parent)
    {
        // Custom resolution logic
    }
}
```

### 2. Custom Directives

Implement `IGraphQLDirective` to add custom directives:

```csharp
public class AuthDirective : IGraphQLDirective
{
    public string Name => "@auth";
    public Task<object?> ApplyAsync(object? input, Dictionary<string, object> args)
    {
        // Check authentication
    }
}
```

### 3. Custom Error Formatting

Extend `ErrorFormattingService` to customize error output:

```csharp
public class CustomErrorFormatter : ErrorFormattingService
{
    public override ExecutionError FormatError(Exception ex)
    {
        // Custom error formatting
    }
}
```

### 4. Custom Data Sources

Implement `IRepository<T>` for custom data access:

```csharp
public class CustomRepository<T> : IRepository<T>
{
    public async Task<T?> GetByIdAsync(string id)
    {
        // Custom implementation
    }
}
```

## Thread Safety

- **ExecutionContext**: Immutable per execution (thread-safe)
- **SchemaService**: Thread-safe (reader-writer lock for type registry)
- **CacheService**: Thread-safe (concurrent dictionary)
- **DataLoaderService**: Thread-safe within execution scope
- **SubscriptionService**: Thread-safe (concurrent connections)

## Memory Management

### Cache Memory Strategy

```
Cache Size: 50 MB (default)
├─ Query Results: 40 MB
├─ Schema Definitions: 5 MB
└─ Type Registry: 5 MB

LRU Eviction when exceeding limit
TTL Expiration (300s default)
```

### Subscription Memory

```
Per Active Connection:
├─ Connection State: ~2 KB
├─ Query Buffer: Variable
└─ Result Buffer: Variable

Max Default Connections: Unlimited
(Configure based on server capacity)
```

## Security Considerations

### Query Complexity Limits

Prevent DoS attacks through expensive queries:

```
Complexity = Sum of field complexities
Hard limits:
  - Max Complexity: 5000
  - Max Depth: 10
  - Max Fields: 200
```

### Type Safety

- Full type checking at compile time
- Null reference handling with nullable reference types
- Safe field resolution with null propagation

### Error Messages

- Development: Detailed error messages with stack traces
- Production: Sanitized error messages (prevent information leakage)

## Testing Strategies

### Unit Testing

```csharp
[Test]
public async Task ExecuteQuery_SimpleQuery_ReturnsResult()
{
    var service = new GraphQLExecutionService();
    var query = new GraphQLQuery("{ hello }");
    var context = await service.ExecuteAsync(query);
    
    Assert.That(context.Errors, Is.Empty);
    Assert.That(context.Data, Is.Not.Null);
}
```

### Integration Testing

```csharp
[Test]
public async Task FullFlow_QueryWithMutation_UpdatesCacheAndSubscriptions()
{
    // Setup
    var serviceProvider = BuildServiceProvider();
    var executionService = serviceProvider.GetRequiredService<GraphQLExecutionService>();
    
    // Execute query
    var result = await executionService.ExecuteAsync(query);
    
    // Verify cache
    var cached = cacheService.GetAsync(cacheKey);
    Assert.That(cached, Is.Not.Null);
}
```

## Monitoring and Diagnostics

### Built-in Metrics

```csharp
var metrics = context.GetMetrics();
{
    "executionTimeMs": 12.5,
    "fieldResolutionTimes": {
        "user": 5.2,
        "posts": 7.3
    },
    "cacheHits": 1,
    "cacheMisses": 2
}
```

### Logging Points

- Query execution start/end
- Cache hits/misses
- Resolver invocation
- Error collection
- Subscription events

### Health Checks

- Cache service health
- Schema service health
- External API connectivity
- Subscription connection count
