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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GraphQLEngine.Configuration;

/// <summary>
/// Extension methods for configuring GraphQL engine services with various customization options.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds GraphQL engine services with custom logging configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureLogging">Optional logging configuration action. If <see langword="null"/>, console logging with Information level is configured.</param>
    /// <param name="configureOptions">Optional options configuration action for GraphQL engine.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    /// <example>
    /// <code>
    /// services.AddGraphQLEngineWithLogging();
    /// </code>
    /// </example>
    public static IServiceCollection AddGraphQLEngineWithLogging(
        this IServiceCollection services,
        Action<ILoggingBuilder>? configureLogging = null,
        Action<GraphQLEngineOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Configure logging if provided
        if (configureLogging != null)
        {
            services.AddLogging(configureLogging);
        }
        else
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
        }

        // Add GraphQL engine services
        services.AddGraphQLEngine(configureOptions);

        return services;
    }

    /// <summary>
    /// Adds GraphQL engine services with custom validation rules.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="validationAction">Optional action to configure validation rules. Default values are:
    /// <list type="bullet">
    /// <item><description><see cref="GraphQLEngineOptions.MaxQueryComplexity"/> = 1000</description></item>
    /// <item><description><see cref="GraphQLEngineOptions.MaxQueryDepth"/> = 5</description></item>
    /// <item><description><see cref="GraphQLEngineOptions.QueryTimeoutMs"/> = 10000</description></item>
    /// <item><description><see cref="GraphQLEngineOptions.EnableDetailedErrorMessages"/> = false</description></item>
    /// </list>
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    /// <example>
    /// <code>
    /// services.AddGraphQLEngineWithValidation(options => {
    ///     options.MaxQueryComplexity = 2000;
    ///     options.EnableDetailedErrorMessages = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddGraphQLEngineWithValidation(
        this IServiceCollection services,
        Action<GraphQLEngineOptions>? validationAction = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddGraphQLEngine(options =>
        {
            // Apply default validation rules
            options.MaxQueryComplexity = 1000;
            options.MaxQueryDepth = 5;
            options.QueryTimeoutMs = 10000;
            options.EnableDetailedErrorMessages = false;

            // Apply custom validation rules if provided
            validationAction?.Invoke(options);
        });

        return services;
    }

    /// <summary>
    /// Adds GraphQL engine services with custom repository configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="repositoryLifetime">The <see cref="ServiceLifetime"/> to use for repository registrations. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    /// <example>
    /// <code>
    /// services.AddGraphQLEngineWithRepositoryLifetime(ServiceLifetime.Scoped);
    /// </code>
    /// </example>
    public static IServiceCollection AddGraphQLEngineWithRepositoryLifetime(
        this IServiceCollection services,
        ServiceLifetime repositoryLifetime = ServiceLifetime.Singleton)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddGraphQLEngine();

        // Re-register repositories with specified lifetime
        switch (repositoryLifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));
                services.AddSingleton<IRepository<GraphQLSchema>, InMemoryRepository<GraphQLSchema>>();
                services.AddSingleton<IRepository<GraphQLType>, InMemoryRepository<GraphQLType>>();
                services.AddSingleton<IRepository<GraphQLQuery>, InMemoryRepository<GraphQLQuery>>();
                services.AddSingleton<IRepository<ExecutionContext>, InMemoryRepository<ExecutionContext>>();
                services.AddSingleton<IRepository<DataLoaderRequest>, InMemoryRepository<DataLoaderRequest>>();
                break;

            case ServiceLifetime.Scoped:
                services.AddScoped(typeof(IRepository<>), typeof(InMemoryRepository<>));
                services.AddScoped<IRepository<GraphQLSchema>, InMemoryRepository<GraphQLSchema>>();
                services.AddScoped<IRepository<GraphQLType>, InMemoryRepository<GraphQLType>>();
                services.AddScoped<IRepository<GraphQLQuery>, InMemoryRepository<GraphQLQuery>>();
                services.AddScoped<IRepository<ExecutionContext>, InMemoryRepository<ExecutionContext>>();
                services.AddScoped<IRepository<DataLoaderRequest>, InMemoryRepository<DataLoaderRequest>>();
                break;

            case ServiceLifetime.Transient:
                services.AddTransient(typeof(IRepository<>), typeof(InMemoryRepository<>));
                services.AddTransient<IRepository<GraphQLSchema>, InMemoryRepository<GraphQLSchema>>();
                services.AddTransient<IRepository<GraphQLType>, InMemoryRepository<GraphQLType>>();
                services.AddTransient<IRepository<GraphQLQuery>, InMemoryRepository<GraphQLQuery>>();
                services.AddTransient<IRepository<ExecutionContext>, InMemoryRepository<ExecutionContext>>();
                services.AddTransient<IRepository<DataLoaderRequest>, InMemoryRepository<DataLoaderRequest>>();
                break;
        }

        return services;
    }

    /// <summary>
    /// Adds GraphQL engine services with custom schema stitching configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="schemaName">Name for the schema stitching configuration. Defaults to "default".</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="schemaName"/> is <see langword="null"/></exception>
    /// <example>
    /// <code>
    /// services.AddGraphQLEngineWithSchemaStitching("my-schema");
    /// </code>
    /// </example>
    public static IServiceCollection AddGraphQLEngineWithSchemaStitching(
        this IServiceCollection services,
        string schemaName = "default")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(schemaName);

        services.AddGraphQLEngine();

        // Re-register schema stitching with custom name
        services.AddSingleton<SchemaStitchingConfig>(_ => new SchemaStitchingConfig(schemaName));

        return services;
    }
}