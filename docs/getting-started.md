# Getting Started with dotnet-graphql-engine

This guide will help you build your first GraphQL API in 15 minutes.

## Prerequisites

- .NET 10 SDK or later
- A text editor or IDE (Visual Studio, VS Code, Rider)
- Basic understanding of GraphQL concepts

## Installation

### 1. Create a New Project

```bash
dotnet new web -n MyGraphQLApi
cd MyGraphQLApi
```

### 2. Install NuGet Package (When Available)

```bash
dotnet add package Sarmkadan.GraphQLEngine
```

Or reference the local project:

```bash
dotnet add reference ../dotnet-graphql-engine/dotnet-graphql-engine.csproj
```

## Your First GraphQL API

### Step 1: Configure Services

Edit `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add GraphQL Engine
builder.Services.AddGraphQLEngine(options =>
{
    options.MaxQueryComplexity = 5000;
    options.MaxQueryDepth = 10;
    options.EnableCaching = true;
    options.CacheTtlSeconds = 300;
});

var app = builder.Build();

// Map GraphQL endpoints
app.MapGraphQL();
app.MapGraphQLSchema();
app.MapHealthCheck();

app.Run();
```

### Step 2: Define Your First Type

Create `Models/User.cs`:

```csharp
public class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Step 3: Create a Service

Create `Services/UserService.cs`:

```csharp
public class UserService
{
    private static readonly List<User> Users = new()
    {
        new() { Id = "1", Name = "Alice", Email = "alice@example.com", CreatedAt = DateTime.UtcNow.AddDays(-7) },
        new() { Id = "2", Name = "Bob", Email = "bob@example.com", CreatedAt = DateTime.UtcNow.AddDays(-3) }
    };

    public Task<User?> GetUserAsync(string id)
    {
        return Task.FromResult(Users.FirstOrDefault(u => u.Id == id));
    }

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return Task.FromResult(Users.AsEnumerable());
    }

    public Task<User> CreateUserAsync(string name, string email)
    {
        var user = new User
        {
            Id = (Users.Count + 1).ToString(),
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        Users.Add(user);
        return Task.FromResult(user);
    }
}
```

### Step 4: Configure GraphQL Schema

In `Program.cs`, after services are added:

```csharp
// Build the schema
var graphqlEngine = app.Services.GetRequiredService<SchemaService>();
var schema = graphqlEngine.CreateSchema("DefaultAPI");

// Define User type
var userType = new GraphQLType
{
    Name = "User",
    Description = "A user in the system",
    Fields = new List<GraphQLField>
    {
        new() { Name = "id", Type = "ID!", Description = "Unique user identifier" },
        new() { Name = "name", Type = "String!", Description = "User's full name" },
        new() { Name = "email", Type = "String!", Description = "User's email address" },
        new() { Name = "createdAt", Type = "DateTime!", Description = "Account creation time" }
    }
};

graphqlEngine.AddType("DefaultAPI", userType);

// Define Query type
var queryType = new GraphQLType
{
    Name = "Query",
    Fields = new List<GraphQLField>
    {
        new() 
        { 
            Name = "user", 
            Type = "User", 
            Description = "Get a user by ID",
            Arguments = new List<GraphQLField>
            {
                new() { Name = "id", Type = "ID!", Description = "User ID" }
            }
        },
        new() 
        { 
            Name = "users", 
            Type = "[User!]!", 
            Description = "Get all users"
        }
    }
};

graphqlEngine.AddType("DefaultAPI", queryType);
```

### Step 5: Test Your API

```bash
dotnet run
```

Your API is now available at `http://localhost:5000`

#### Query Users

```bash
curl -X POST http://localhost:5000/graphql \
  -H "Content-Type: application/json" \
  -d '{
    "query": "{ users { id name email createdAt } }"
  }'
```

#### Get Specific User

```bash
curl -X POST http://localhost:5000/graphql \
  -H "Content-Type: application/json" \
  -d '{
    "query": "{ user(id: \"1\") { id name email } }"
  }'
```

## Next Steps

### 1. Add Mutations

Add a mutation type to create users:

```csharp
var mutationType = new GraphQLType
{
    Name = "Mutation",
    Fields = new List<GraphQLField>
    {
        new()
        {
            Name = "createUser",
            Type = "User!",
            Description = "Create a new user",
            Arguments = new List<GraphQLField>
            {
                new() { Name = "name", Type = "String!", Description = "User name" },
                new() { Name = "email", Type = "String!", Description = "User email" }
            }
        }
    }
};

graphqlEngine.AddType("DefaultAPI", mutationType);
```

### 2. Enable Query Complexity Analysis

```csharp
var analysisService = app.Services.GetRequiredService<QueryAnalysisService>();

// Set limits
analysisService.SetMaxComplexity(5000);
analysisService.SetMaxDepth(10);
```

### 3. Add DataLoader for Batch Operations

```csharp
var dataLoaderService = app.Services.GetRequiredService<DataLoaderService>();

dataLoaderService.RegisterBatchFunction("GetUsersByIds", async (ids) =>
{
    var userService = app.Services.GetRequiredService<UserService>();
    return await userService.GetUsersByIdsAsync(ids.Cast<string>());
});
```

### 4. Enable Real-Time Subscriptions

```csharp
var subscriptionService = app.Services.GetRequiredService<SubscriptionService>();

var subscriptionType = new GraphQLType
{
    Name = "Subscription",
    Fields = new List<GraphQLField>
    {
        new()
        {
            Name = "userCreated",
            Type = "User!",
            Description = "Fired when a new user is created"
        }
    }
};

graphqlEngine.AddType("DefaultAPI", subscriptionType);
```

## Best Practices

### 1. Use Descriptive Names

```csharp
// Good
new() { Name = "getActiveUsersByRole", Type = "[User!]!" }

// Avoid
new() { Name = "users2", Type = "[User!]!" }
```

### 2. Always Specify Descriptions

```csharp
// Good
new() 
{ 
    Name = "email", 
    Type = "String", 
    Description = "User's primary email address, may be null if not verified"
}

// Avoid
new() { Name = "email", Type = "String" }
```

### 3. Use Non-Nullable Types for Required Fields

```csharp
// For fields that must always have a value
new() { Name = "id", Type = "ID!" }  // ! means non-nullable
new() { Name = "name", Type = "String!" }

// For optional fields
new() { Name = "nickname", Type = "String" }  // nullable
```

### 4. Organize Types Logically

```csharp
// Create a type for each entity
var userType = new GraphQLType { Name = "User", ... };
var postType = new GraphQLType { Name = "Post", ... };
var commentType = new GraphQLType { Name = "Comment", ... };

// Add related mutations together
var mutationType = new GraphQLType
{
    Name = "Mutation",
    Fields = new List<GraphQLField>
    {
        new() { Name = "createUser", ... },
        new() { Name = "updateUser", ... },
        new() { Name = "deleteUser", ... }
    }
};
```

### 5. Implement Error Handling

```csharp
try
{
    var context = await executionService.ExecuteAsync(query);
    if (context.Errors.Any())
    {
        return Results.BadRequest(new { errors = context.Errors });
    }
    return Results.Ok(context.Data);
}
catch (GraphQLException ex)
{
    return Results.BadRequest(new { error = ex.Message });
}
```

## Common Patterns

### Filtering Results

```csharp
var queryType = new GraphQLType
{
    Name = "Query",
    Fields = new List<GraphQLField>
    {
        new()
        {
            Name = "usersByName",
            Type = "[User!]!",
            Arguments = new List<GraphQLField>
            {
                new() { Name = "name", Type = "String!", Description = "Filter by name" },
                new() { Name = "limit", Type = "Int", Description = "Max results" }
            }
        }
    }
};
```

### Pagination

```csharp
new()
{
    Name = "users",
    Type = "[User!]!",
    Arguments = new List<GraphQLField>
    {
        new() { Name = "first", Type = "Int", Description = "Items per page" },
        new() { Name = "after", Type = "String", Description = "Cursor for pagination" }
    }
}
```

### Nested Types

```csharp
// Post type with nested User
var postType = new GraphQLType
{
    Name = "Post",
    Fields = new List<GraphQLField>
    {
        new() { Name = "id", Type = "ID!" },
        new() { Name = "title", Type = "String!" },
        new() { Name = "author", Type = "User!" }  // Nested User type
    }
};
```

## Troubleshooting

### Q: "Type not found" error

**A:** Make sure you've added the type to the schema:
```csharp
graphqlEngine.AddType("DefaultAPI", userType);
```

### Q: "Query timeout"

**A:** Increase the timeout:
```csharp
options.QueryTimeoutMs = 60000;  // 60 seconds
```

### Q: "Port already in use"

**A:** Change the port:
```bash
dotnet run --urls http://localhost:5001
```

## Resources

- [Main Documentation](../README.md)
- [API Reference](./api-reference.md)
- [Architecture Guide](./architecture.md)
- [FAQ](./faq.md)
