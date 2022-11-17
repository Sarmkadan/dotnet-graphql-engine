// AdvancedUsage.cs
// Demonstrates custom configuration, error handling, and complexity analysis.

using Microsoft.Extensions.DependencyInjection;
using GraphQLEngine.Configuration;
using GraphQLEngine.Exceptions;
using GraphQLEngine.Services.GraphQL;

var services = new ServiceCollection();

// Configure advanced options
services.AddGraphQLEngine(options =>
{
    options.MaxQueryComplexity = 1000;
    options.MaxQueryDepth = 5;
    options.EnableCaching = true;
    options.CacheTtlSeconds = 60;
});

var provider = services.BuildServiceProvider();
var executionService = provider.GetRequiredService<GraphQLExecutionService>();

try
{
    // Deep/complex query that might fail complexity analysis
    var complexQuery = new GraphQLQuery("{ user { posts { comments { author { name } } } } }");
    var result = await executionService.ExecuteAsync(complexQuery);
    
    if (result.Errors != null && result.Errors.Any())
    {
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"Error: {error.Message}");
        }
    }
    else
    {
        Console.WriteLine($"Data: {result.Data}");
    }
}
catch (GraphQLException ex)
{
    Console.WriteLine($"Engine Exception: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"General Exception: {ex.Message}");
}
