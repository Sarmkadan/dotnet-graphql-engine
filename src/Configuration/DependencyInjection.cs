// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Data.Repositories;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Domain.ValueObjects;
using GraphQLEngine.Services.DataLoader;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Services.QueryAnalysis;
using GraphQLEngine.Services.Schema;
using GraphQLEngine.Services.Subscriptions;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQLEngine.Configuration;

/// <summary>
/// Dependency injection configuration for the GraphQL engine
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all GraphQL engine services to the service collection
    /// </summary>
    public static IServiceCollection AddGraphQLEngine(
        this IServiceCollection services,
        Action<GraphQLEngineOptions>? configure = null)
    {
        // Create and configure options
        var options = new GraphQLEngineOptions();
        configure?.Invoke(options);

        // Validate options
        if (!options.Validate(out var errors))
            throw new InvalidOperationException(
                $"Invalid GraphQL engine options: {string.Join(", ", errors)}");

        // Register options as singleton
        services.AddSingleton(options);

        // Register repositories
        services.AddSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));
        services.AddSingleton<IRepository<GraphQLSchema>, InMemoryRepository<GraphQLSchema>>();
        services.AddSingleton<IRepository<GraphQLType>, InMemoryRepository<GraphQLType>>();
        services.AddSingleton<IRepository<GraphQLQuery>, InMemoryRepository<GraphQLQuery>>();
        services.AddSingleton<IRepository<ExecutionContext>, InMemoryRepository<ExecutionContext>>();
        services.AddSingleton<IRepository<DataLoaderRequest>, InMemoryRepository<DataLoaderRequest>>();

        // Register services
        services.AddScoped<GraphQLExecutionService>();
        services.AddScoped<SchemaService>();
        services.AddScoped<QueryAnalysisService>();
        services.AddScoped<DataLoaderService>();

        // Register subscription configuration and service
        var subscriptionConfig = new SubscriptionConfig
        {
            Enabled = options.EnableSubscriptions,
            MaxConnections = options.MaxSubscriptionConnections,
            ConnectionTimeoutMs = options.SubscriptionTimeoutMs,
            HeartbeatIntervalMs = options.HeartbeatIntervalMs
        };
        services.AddSingleton(subscriptionConfig);
        services.AddScoped<SubscriptionService>();

        // Register schema stitching configuration
        var stitchingConfig = new SchemaStitchingConfig("default");
        services.AddSingleton(stitchingConfig);

        return services;
    }

    /// <summary>
    /// Gets a preconfigured service provider for testing
    /// </summary>
    public static IServiceProvider CreateTestServiceProvider(
        Action<GraphQLEngineOptions>? configure = null)
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add GraphQL engine
        services.AddGraphQLEngine(configure);

        return services.BuildServiceProvider();
    }
}

/// <summary>
/// Extension methods for service configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds GraphQL engine with default configuration
    /// </summary>
    public static IServiceCollection AddGraphQLEngineDefault(
        this IServiceCollection services)
    {
        return services.AddGraphQLEngine();
    }

    /// <summary>
    /// Adds GraphQL engine with strict query limits
    /// </summary>
    public static IServiceCollection AddGraphQLEngineStrict(
        this IServiceCollection services)
    {
        return services.AddGraphQLEngine(options =>
        {
            options.MaxQueryComplexity = 1000;
            options.MaxQueryDepth = 5;
            options.QueryTimeoutMs = 10000;
            options.EnableDetailedErrorMessages = false;
        });
    }

    /// <summary>
    /// Adds GraphQL engine with permissive configuration (development)
    /// </summary>
    public static IServiceCollection AddGraphQLEnginePermissive(
        this IServiceCollection services)
    {
        return services.AddGraphQLEngine(options =>
        {
            options.MaxQueryComplexity = 50000;
            options.MaxQueryDepth = 20;
            options.QueryTimeoutMs = 60000;
            options.EnableDetailedErrorMessages = true;
            options.EnableIntrospection = true;
        });
    }
}
