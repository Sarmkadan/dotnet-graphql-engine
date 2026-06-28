// IntegrationExample.cs
// Demonstrates wiring into ASP.NET Core Dependency Injection.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using GraphQLEngine.Configuration;
using GraphQLEngine.Services.GraphQL;

var builder = WebApplication.CreateBuilder(args);

// Register engine in DI container
builder.Services.AddGraphQLEngine(options =>
{
    options.EnableIntrospection = true;
});

var app = builder.Build();

// Use middleware/service in endpoints
app.MapPost("/graphql", async (GraphQLQuery query, IGraphQLExecutionService executionService) =>
{
    var result = await executionService.ExecuteAsync(query);
    return Results.Ok(result);
});

app.Run();
