#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Data.Repositories;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.GraphQL;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQLEngine.Configuration;

/// <summary>
/// Extension methods for registering Automatic Persisted Query (APQ) support
/// in the dependency injection container.
/// </summary>
public static class PersistedQueryExtensions
{
    /// <summary>
    /// Registers the APQ repository and <see cref="PersistedQueryService"/> so that
    /// persisted queries can be stored, retrieved by hash, and managed at runtime.
    /// Call this after <see cref="DependencyInjection.AddGraphQLEngine"/> to opt in to
    /// the persisted-query feature.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddGraphQLEngine()
    /// .AddPersistedQueries();
    /// </code>
    /// </example>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddPersistedQueries(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IRepository<PersistedQuery>, InMemoryRepository<PersistedQuery>>();
        services.AddScoped<PersistedQueryService>();

        return services;
    }

    /// <summary>
    /// Registers APQ support with a custom configuration callback, allowing callers
    /// to supply <see cref="PersistedQueryOptions"/> at registration time.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Callback to apply custom options.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the provided options are invalid.</exception>
    public static IServiceCollection AddPersistedQueries(
        this IServiceCollection services,
        Action<PersistedQueryOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new PersistedQueryOptions();
        configure(options);

        if (!options.Validate(out var errors))
            throw new InvalidOperationException(
                $"Invalid persisted query options: {string.Join(", ", errors)}");

        services.AddSingleton(options);
        services.AddSingleton<IRepository<PersistedQuery>, InMemoryRepository<PersistedQuery>>();
        services.AddScoped<PersistedQueryService>();

        return services;
    }
}

/// <summary>
/// Configuration options for the APQ persisted-query feature.
/// </summary>
sealed public class PersistedQueryOptions
{
    /// <summary>
    /// When <c>true</c>, incoming APQ payloads that supply both a hash and a full query
    /// document have the hash verified against the document before storage.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnforceHashVerification { get; set; } = true;

    /// <summary>
    /// Maximum number of persisted queries held in the in-process hash index.
    /// Once reached, new registrations still succeed (they bypass the index and hit the
    /// repository directly) until the process restarts and rebuilds from the store.
    /// Defaults to <c>10 000</c>.
    /// </summary>
    public int MaxIndexSize { get; set; } = 10_000;

    /// <summary>
    /// When <c>true</c>, only pre-registered persisted queries may be executed.
    /// Any request that supplies an inline query document (rather than a recognised hash)
    /// is rejected with a <c>QUERY_NOT_ALLOWED</c> error.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool AllowlistOnly { get; set; } = false;

    /// <summary>
    /// When <c>true</c>, a client that sends only a hash for an unknown query receives a
    /// <c>PERSISTED_QUERY_NOT_FOUND</c> error rather than a generic execution failure.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool ReturnNotFoundError { get; set; } = true;

    /// <summary>
    /// Validates the option values and collects any constraint violations.
    /// </summary>
    /// <param name="errors">Populated with one message per violated constraint.</param>
    /// <returns><c>true</c> when all options are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <c>null</c>.</exception>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (errors is null)
            throw new ArgumentNullException(nameof(errors));

        if (MaxIndexSize <= 0)
            errors.Add("MaxIndexSize must be greater than 0");

        return errors.Count == 0;
    }
}
