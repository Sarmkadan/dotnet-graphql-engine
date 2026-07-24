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
using Microsoft.Extensions.Logging;
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
        // -----------------------------------------------------------------
        // Options registration (IOptions pattern)
        // -----------------------------------------------------------------
        services.AddOptions<GraphQLEngineOptions>();
        services.AddOptions<DotnetGraphqlEngineOptions>(); // new alternative options class

        if (configure != null)
        {
            services.Configure(configure);
        }

        // Validate options (both option types share the same validator logic)
        services.AddSingleton<IValidateOptions<GraphQLEngineOptions>>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<GraphQLEngineOptions>>();
            return new GraphQLEngineOptionsValidator(options);
        });

        services.AddSingleton<IValidateOptions<DotnetGraphqlEngineOptions>>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<DotnetGraphqlEngineOptions>>();
            return new DotnetGraphqlEngineOptionsValidator(options);
        });

        // -----------------------------------------------------------------
        // Repository registration
        // -----------------------------------------------------------------
        services.AddSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));
        services.AddSingleton<IRepository<GraphQLSchema>, InMemoryRepository<GraphQLSchema>>();
        services.AddSingleton<IRepository<GraphQLType>, InMemoryRepository<GraphQLType>>();
        services.AddSingleton<IRepository<GraphQLQuery>, InMemoryRepository<GraphQLQuery>>();
        services.AddSingleton<IRepository<ExecutionContext>, InMemoryRepository<ExecutionContext>>();
        services.AddSingleton<IRepository<DataLoaderRequest>, InMemoryRepository<DataLoaderRequest>>();

        // -----------------------------------------------------------------
        // Service registration
        // -----------------------------------------------------------------
        services.AddScoped<GraphQLExecutionService>();
        services.AddScoped<SchemaService>();
        services.AddScoped<QueryAnalysisService>();
        services.AddScoped<DataLoaderService>();

        // These were previously only constructible by hand; register them so
        // consumers can resolve them the same way as the rest of the engine.
        services.AddScoped<CacheService>(provider => new CacheService(
            provider.GetRequiredService<ILogger<CacheService>>(),
            provider.GetRequiredService<IOptions<GraphQLEngineOptions>>().Value));
        services.AddScoped<ErrorFormattingService>(provider => new ErrorFormattingService(
            provider.GetRequiredService<ILogger<ErrorFormattingService>>(),
            provider.GetRequiredService<IOptions<GraphQLEngineOptions>>().Value));
        // Singleton: the in-process bounded hash index (see PersistedQueryService)
        // must outlive a single request scope, otherwise every request rebuilds an
        // empty LRU cache and the eviction bound never actually caps process memory.
        services.AddSingleton<PersistedQueryService>();

        // -----------------------------------------------------------------
        // Subscription configuration and service
        // -----------------------------------------------------------------
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

        // -----------------------------------------------------------------
        // Schema stitching configuration
        // -----------------------------------------------------------------
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
    public static IServiceCollection AddGraphQLEngineDefault(this IServiceCollection services)
        => services.AddGraphQLEngine();

    public static IServiceCollection AddGraphQLEngineStrict(this IServiceCollection services)
        => services.AddGraphQLEngine(options =>
        {
            options.MaxQueryComplexity = 1000;
            options.MaxQueryDepth = 5;
            options.QueryTimeoutMs = 10000;
            options.EnableDetailedErrorMessages = false;
        });

    public static IServiceCollection AddGraphQLEnginePermissive(this IServiceCollection services)
        => services.AddGraphQLEngine(options =>
        {
            options.MaxQueryComplexity = 50000;
            options.MaxQueryDepth = 20;
            options.QueryTimeoutMs = 60000;
            options.EnableDetailedErrorMessages = true;
            options.EnableIntrospection = true;
        });
}

/// <summary>
/// Validates GraphQL engine configuration options
/// </summary>
internal sealed class GraphQLEngineOptionsValidator : IValidateOptions<GraphQLEngineOptions>
{
    private readonly IOptions<GraphQLEngineOptions> _options;

    public GraphQLEngineOptionsValidator(IOptions<GraphQLEngineOptions> options) => _options = options;

    public ValidateOptionsResult Validate(string? name, GraphQLEngineOptions options)
    {
        var errors = options.Validate();
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}

/// <summary>
/// Validates the alternative options class (DotnetGraphqlEngineOptions)
/// </summary>
internal sealed class DotnetGraphqlEngineOptionsValidator : IValidateOptions<DotnetGraphqlEngineOptions>
{
    private readonly IOptions<DotnetGraphqlEngineOptions> _options;

    public DotnetGraphqlEngineOptionsValidator(IOptions<DotnetGraphqlEngineOptions> options) => _options = options;

    public ValidateOptionsResult Validate(string? name, DotnetGraphqlEngineOptions options)
    {
        var errors = options.Validate();
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
