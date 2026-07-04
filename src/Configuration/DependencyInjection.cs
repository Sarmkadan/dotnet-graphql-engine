#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Data.Repositories;
using GraphQLEngine.Domain.Entities;
using ExecutionContext = GraphQLEngine.Domain.Entities.ExecutionContext;
using GraphQLEngine.Domain.ValueObjects;
using GraphQLEngine.Services.DataLoader;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Services.QueryAnalysis;
using GraphQLEngine.Services.Schema;
using GraphQLEngine.Services.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        // Configure options with IOptions pattern
        services.AddOptions<GraphQLEngineOptions>();

        if (configure != null)
        {
            services.Configure(configure);
        }

        // Validate options
        services.AddSingleton<IValidateOptions<GraphQLEngineOptions>>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<GraphQLEngineOptions>>();
            return new GraphQLEngineOptionsValidator(options);
        });

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
        services.AddSingleton<SubscriptionConfig>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<GraphQLEngineOptions>>();
            return new SubscriptionConfig
            {
                Enabled = options.Value.EnableSubscriptions,
                MaxConnections = options.Value.MaxSubscriptionConnections,
                ConnectionTimeoutMs = options.Value.SubscriptionTimeoutMs,
                HeartbeatIntervalMs = options.Value.HeartbeatIntervalMs
            };
        });
        services.AddScoped<SubscriptionService>();

        // Register schema stitching configuration
        services.AddSingleton<SchemaStitchingConfig>(_ => new SchemaStitchingConfig("default"));

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

/// <summary>
/// Validates GraphQL engine configuration options
/// </summary>
internal sealed class GraphQLEngineOptionsValidator : IValidateOptions<GraphQLEngineOptions>
{
    private readonly IOptions<GraphQLEngineOptions> _options;

    public GraphQLEngineOptionsValidator(IOptions<GraphQLEngineOptions> options)
    {
        _options = options;
    }

    public ValidateOptionsResult Validate(string? name, GraphQLEngineOptions options)
    {
        var errors = options.Validate();

        if (errors.Count == 0)
        {
            return ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail(errors);
    }
}