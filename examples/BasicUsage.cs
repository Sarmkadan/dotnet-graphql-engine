// BasicUsage.cs
// Minimal setup to execute a GraphQL query.

using Microsoft.Extensions.DependencyInjection;
using GraphQLEngine.Configuration;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Services.Schema;

// 1. Setup DI
var services = new ServiceCollection();
services.AddGraphQLEngine();
var provider = services.BuildServiceProvider();

// 2. Initialize Schema
var schemaService = provider.GetRequiredService<SchemaService>();
var schema = schemaService.CreateSchema("BasicAPI");

// 3. Simple execution
var executionService = provider.GetRequiredService<GraphQLExecutionService>();
var query = new GraphQLQuery("{ __schema { types { name } } }");
var result = await executionService.ExecuteAsync(query);

Console.WriteLine($"Result: {result.Data}");
