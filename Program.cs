#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Configuration;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Services.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GraphQLEngine;

/// <summary>
/// Main entry point for the GraphQL Engine
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== GraphQL Engine - Startup ===");
        Console.WriteLine();

        // Configure dependency injection
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add GraphQL engine with custom configuration
        services.AddGraphQLEngine(options =>
        {
            options.ServiceName = "dotnet-graphql-engine";
            options.Version = "1.0.0";
            options.MaxQueryComplexity = 5000;
            options.MaxQueryDepth = 10;
            options.QueryTimeoutMs = 30000;
            options.EnableSubscriptions = true;
            options.EnableCaching = true;
            options.EnableDataLoading = true;
            options.EnableSchemaStitching = true;
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("GraphQL Engine starting...");

            // Initialize schema service and create a sample schema
            var schemaService = serviceProvider.GetRequiredService<SchemaService>();
            var executionService = serviceProvider.GetRequiredService<GraphQLExecutionService>();

            // Create a sample schema
            CreateSampleSchema(schemaService, logger);

            // Execute a sample query
            await ExecuteSampleQuery(schemaService, executionService, logger);

            // Display service information
            DisplayServiceInfo(serviceProvider, logger);

            logger.LogInformation("GraphQL Engine ready for operations");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in GraphQL Engine");
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            await serviceProvider.DisposeAsync();
        }
    }

    /// <summary>
    /// Creates a sample GraphQL schema
    /// </summary>
    static void CreateSampleSchema(SchemaService schemaService, ILogger<Program> logger)
    {
        logger.LogInformation("Creating sample schema...");

        try
        {
            // Create schema
            var schema = schemaService.CreateSchema("SampleAPI");
            schema.Description = "Sample GraphQL API Schema";

            // Create User type
            var userType = new GraphQLType("User", GraphQLTypeKind.Object);
            userType.Description = "Represents a user in the system";

            var idField = new GraphQLField("id", "ID");
            idField.Description = "User ID";

            var nameField = new GraphQLField("name", "String");
            nameField.Description = "User full name";

            var emailField = new GraphQLField("email", "String");
            emailField.Description = "User email address";

            userType.AddField(idField);
            userType.AddField(nameField);
            userType.AddField(emailField);

            schemaService.AddType("SampleAPI", userType);

            // Create Query root type
            var queryType = new GraphQLType("Query", GraphQLTypeKind.Object);
            queryType.Description = "Root query type";

            var getUserField = new GraphQLField("getUser", "User");
            getUserField.Description = "Get a user by ID";

            var idArg = new GraphQLArgument("id", "ID", isRequired: true);
            getUserField.AddArgument(idArg);

            var getUsersField = new GraphQLField("getUsers", "User");
            getUsersField.Description = "Get all users";

            queryType.AddField(getUserField);
            queryType.AddField(getUsersField);

            // Add query type to schema
            schema.QueryType = queryType;
            schemaService.AddType("SampleAPI", queryType);

            // Log schema information
            var summary = schema.GetSummary();
            logger.LogInformation("Schema created: {SchemaSummary}", summary);

            // Display schema SDL
            var sdl = schemaService.ExportAsSDL("SampleAPI");
            Console.WriteLine();
            Console.WriteLine("=== Sample Schema Definition ===");
            Console.WriteLine(sdl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating sample schema");
            throw;
        }
    }

    /// <summary>
    /// Executes a sample GraphQL query
    /// </summary>
    static async Task ExecuteSampleQuery(SchemaService schemaService,
        GraphQLExecutionService executionService, ILogger<Program> logger)
    {
        logger.LogInformation("Executing sample query...");

        try
        {
            // Create a sample query
            var query = new GraphQLQuery("{ getUser(id: \"1\") { id name email } }");
            query.AddSelectedField("getUser");
            query.SetVariable("userId", "1");

            // Execute the query
            var executionContext = await executionService.ExecuteAsync(query);

            logger.LogInformation("Query execution completed");
            logger.LogInformation("Execution summary: {Summary}", executionContext.GetSummary());

            Console.WriteLine();
            Console.WriteLine("=== Query Execution Result ===");
            Console.WriteLine($"Query ID: {query.Id}");
            Console.WriteLine($"Duration: {executionContext.DurationMs}ms");
            Console.WriteLine($"Fields Requested: {executionContext.RequestedFieldCount}");
            Console.WriteLine($"Fields Resolved: {executionContext.ResolvedFieldCount}");
            Console.WriteLine($"State: {executionContext.State}");

            if (executionContext.Errors.Any())
            {
                Console.WriteLine();
                Console.WriteLine("Errors:");
                foreach (var error in executionContext.Errors)
                    Console.WriteLine($"  - {error.Message}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing sample query");
            throw;
        }
    }

    /// <summary>
    /// Displays information about registered services
    /// </summary>
    static void DisplayServiceInfo(IServiceProvider serviceProvider, ILogger<Program> logger)
    {
        var executionService = serviceProvider.GetRequiredService<GraphQLExecutionService>();
        var stats = executionService.GetStatistics();

        Console.WriteLine();
        Console.WriteLine("=== Service Statistics ===");
        foreach (var stat in stats)
            Console.WriteLine($"{stat.Key}: {stat.Value}");
    }
}
