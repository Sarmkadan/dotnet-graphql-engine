#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Example 1: Basic GraphQL API
///
/// Demonstrates a simple GraphQL API with queries and mutations for managing users.
/// This is a minimal example suitable for getting started.
///
/// Run:
///   dotnet new web -n BasicGraphQLApi
///   dotnet add package Sarmkadan.GraphQLEngine
///   Copy this file to Program.cs
///   dotnet run
/// </summary>

using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGraphQLEngine(options =>
{
    options.MaxQueryComplexity = 5000;
    options.MaxQueryDepth = 10;
    options.EnableCaching = true;
    options.CacheTtlSeconds = 300;
});

var app = builder.Build();

// Configure schema
var schemaService = app.Services.GetRequiredService<SchemaService>();
var schema = schemaService.CreateSchema("UserAPI");

// Define User type
var userType = new GraphQLType
{
    Name = "User",
    Description = "A user in the system",
    Fields = new List<GraphQLField>
    {
        new() { Name = "id", Type = "ID!", Description = "Unique user ID" },
        new() { Name = "name", Type = "String!", Description = "User's full name" },
        new() { Name = "email", Type = "String!", Description = "User's email" },
        new() { Name = "createdAt", Type = "DateTime!", Description = "Account creation date" }
    }
};

schemaService.AddType("UserAPI", userType);

// In-memory user storage
var users = new List<dynamic>
{
    new { Id = "1", Name = "Alice Johnson", Email = "alice@example.com", CreatedAt = DateTime.UtcNow.AddDays(-7) },
    new { Id = "2", Name = "Bob Smith", Email = "bob@example.com", CreatedAt = DateTime.UtcNow.AddDays(-3) },
    new { Id = "3", Name = "Charlie Brown", Email = "charlie@example.com", CreatedAt = DateTime.UtcNow.AddHours(-5) }
};

// Define Query type
var queryType = new GraphQLType
{
    Name = "Query",
    Description = "Root query",
    Fields = new List<GraphQLField>
    {
        new()
        {
            Name = "user",
            Type = "User",
            Description = "Get a user by ID",
            Arguments = new List<GraphQLField>
            {
                new() { Name = "id", Type = "ID!", Description = "User ID to fetch" }
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

schemaService.AddType("UserAPI", queryType);

// Define Mutation type
var mutationType = new GraphQLType
{
    Name = "Mutation",
    Description = "Root mutation",
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

schemaService.AddType("UserAPI", mutationType);

// Map endpoints
app.MapPost("/graphql", async (GraphQLQuery query, IGraphQLExecutionService executionService) =>
{
    try
    {
        var context = await executionService.ExecuteAsync(query);

        if (context.Errors.Any())
        {
            return Results.BadRequest(new { errors = context.Errors.Select(e => new { e.Message, e.Field }) });
        }

        return Results.Ok(new { data = context.Data });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/graphql/schema", async (ISchemaService schemaService) =>
{
    return schemaService.ExportAsSDL("UserAPI");
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

// Helper: Mock resolver dispatcher (simplified)
sealed public class SimpleResolver
{
    private readonly List<dynamic> _users;

    public SimpleResolver(List<dynamic> users) => _users = users;

    public dynamic? ResolveUser(string id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public IEnumerable<dynamic> ResolveUsers()
    {
        return _users;
    }

    public dynamic CreateUser(string name, string email)
    {
        var newUser = new
        {
            Id = (_users.Count + 1).ToString(),
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        // In real app, add to database
        return newUser;
    }
}
