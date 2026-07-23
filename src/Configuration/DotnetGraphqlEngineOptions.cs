#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using GraphQLEngine.Common.Constants;
using Microsoft.Extensions.Configuration;

namespace GraphQLEngine.Configuration;

/// <summary>
/// Configuration options for the GraphQL engine (alternative name for IOptions pattern)
/// </summary>
sealed public class DotnetGraphqlEngineOptions
{
    /// <summary>
    /// Service name for identification and logging
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string ServiceName { get; set; } = "dotnet-graphql-engine";

    /// <summary>
    /// Service version
    /// </summary>
    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Maximum query complexity allowed (prevents expensive queries)
    /// </summary>
    [Range(1, 100000)]
    public int MaxQueryComplexity { get; set; } = GraphQLConstants.DefaultMaxQueryComplexity;

    /// <summary>
    /// Maximum query depth allowed (prevents deeply nested queries)
    /// </summary>
    [Range(1, 100)]
    public int MaxQueryDepth { get; set; } = GraphQLConstants.DefaultMaxQueryDepth;

    /// <summary>
    /// Maximum query length in characters
    /// </summary>
    [Range(100, 100000)]
    public int MaxQueryLength { get; set; } = GraphQLConstants.DefaultMaxQueryLength;

    /// <summary>
    /// Query execution timeout in milliseconds
    /// </summary>
    [Range(1000, 300000)]
    public int QueryTimeoutMs { get; set; } = GraphQLConstants.DefaultQueryTimeoutMs;

    /// <summary>
    /// Maximum number of fields allowed in a single query
    /// </summary>
    [Range(1, 1000)]
    public int MaxQueryFields { get; set; } = 200;

    /// <summary>
    /// Maximum batch size for batched queries
    /// </summary>
    [Range(1, 1000)]
    public int MaxBatchSize { get; set; } = 100;

    // Feature flags
    public bool EnableIntrospection { get; set; } = true;
    public bool EnableCaching { get; set; } = true;
    public bool EnableSubscriptions { get; set; } = true;
    public bool EnableDataLoading { get; set; } = true;
    public bool EnableSchemaStitching { get; set; } = true;
    public bool EnableDetailedErrorMessages { get; set; } = false;
    public bool EnablePerformanceMetrics { get; set; } = true;

    // Federation options
    public bool EnableFederation { get; set; } = false;
    public string FederationDiscoveryEndpoint { get; set; } = "/.well-known/federation";
    public TimeSpan FederationTimeout { get; set; } = TimeSpan.FromSeconds(30);
    [Range(0, 86400)]
    public int EntityCacheTtlSeconds { get; set; } = GraphQLConstants.DefaultCacheTTLSeconds;
    [Range(1, 1000000)]
    public int EntityCacheMaxSize { get; set; } = GraphQLConstants.DefaultCacheMaxSize;

    // Caching options
    [Range(0, 86400)]
    public int CacheTTLSeconds { get; set; } = GraphQLConstants.DefaultCacheTTLSeconds;
    [Range(1, 1000000)]
    public int CacheMaxSize { get; set; } = GraphQLConstants.DefaultCacheMaxSize;
    [Range(1024, 1073741824)] // 1KB to 1GB
    public int CacheMaxSizeBytes { get; set; } = 52428800; // 50MB

    // Subscription options
    [Range(1, 10000)]
    public int MaxSubscriptionConnections { get; set; } = GraphQLConstants.DefaultMaxSubscriptionConnections;
    [Range(1000, 300000)]
    public int SubscriptionTimeoutMs { get; set; } = GraphQLConstants.DefaultSubscriptionTimeoutMs;
    [Range(1000, 60000)]
    public int HeartbeatIntervalMs { get; set; } = GraphQLConstants.DefaultHeartbeatIntervalMs;

    // Data loading options
    [Range(1, 1000)]
    public int DataLoaderBatchSize { get; set; } = GraphQLConstants.DefaultDataLoaderBatchSize;
    [Range(0, 1000)]
    public int DataLoaderDelayMs { get; set; } = GraphQLConstants.DefaultDataLoaderDelayMs;

    // Remote schema options
    public bool EnableRemoteSchemaIntrospection { get; set; } = true;
    [Range(1000, 60000)]
    public int RemoteSchemaTimeoutMs { get; set; } = 30000;

    // Logging / error handling
    public bool LogInternalErrors { get; set; } = true;
    public bool IncludeDetailedErrorMessages { get; set; } = false;

    /// <summary>
    /// Validates the configuration options using DataAnnotations.
    /// </summary>
    /// <returns>List of validation errors, empty if valid.</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();
        var validationContext = new ValidationContext(this);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(this, validationContext, validationResults, true);
        if (!isValid)
        {
            errors.AddRange(validationResults.Select(v => v.ErrorMessage ?? "Validation failed"));
        }

        // Additional custom validation (mirrors GraphQLEngineOptions)
        if (MaxQueryComplexity <= 0) errors.Add("MaxQueryComplexity must be greater than 0");
        if (MaxQueryDepth <= 0) errors.Add("MaxQueryDepth must be greater than 0");
        if (QueryTimeoutMs <= 0) errors.Add("QueryTimeoutMs must be greater than 0");
        if (MaxSubscriptionConnections <= 0) errors.Add("MaxSubscriptionConnections must be greater than 0");
        if (SubscriptionTimeoutMs <= 0) errors.Add("SubscriptionTimeoutMs must be greater than 0");
        if (HeartbeatIntervalMs <= 0) errors.Add("HeartbeatIntervalMs must be greater than 0");
        if (CacheTTLSeconds < 0) errors.Add("CacheTTLSeconds cannot be negative");
        if (CacheMaxSizeBytes <= 0) errors.Add("CacheMaxSizeBytes must be greater than 0");

        return errors;
    }

    /// <summary>
    /// Creates a deep copy of the options.
    /// </summary>
    public DotnetGraphqlEngineOptions Clone()
    {
        return (DotnetGraphqlEngineOptions)MemberwiseClone();
    }

    /// <summary>
    /// Returns a short human‑readable summary of the configuration.
    /// </summary>
    public string GetSummary()
    {
        return $"{ServiceName} v{Version}: " +
               $"MaxComplexity={MaxQueryComplexity}, " +
               $"MaxDepth={MaxQueryDepth}, " +
               $"Timeout={QueryTimeoutMs}ms, " +
               $"Introspection={EnableIntrospection}, " +
               $"Subscriptions={EnableSubscriptions}, " +
               $"Caching={EnableCaching}";
    }

    /// <summary>
    /// Binds configuration from legacy string-based keys for backward compatibility.
    /// This method supports the old ConfigurationKeys format and emits a deprecation warning.
    /// </summary>
    /// <param name="configuration">The configuration to bind from</param>
    /// <returns>A new DotnetGraphqlEngineOptions instance with values from legacy configuration</returns>
    /// <remarks>
    /// This method provides backward compatibility for applications that previously used:
    /// - graphql:maxQueryComplexity
    /// - graphql:maxQueryDepth
    /// - graphql:queryTimeoutMs
    /// - graphql:enableIntrospection
    /// - graphql:enableCaching
    /// - graphql:cacheTTLSeconds
    /// - graphql:enableSubscriptions
    /// - graphql:maxSubscriptionConnections
    ///
    /// New applications should use the typed DotnetGraphqlEngineOptions pattern instead.
    /// </remarks>
    public static DotnetGraphqlEngineOptions BindFromLegacyConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new DotnetGraphqlEngineOptions();

        // Log deprecation warning
        Console.Error.WriteLine("[DEPRECATION WARNING] Legacy string-based configuration keys are deprecated. " +
                            "Use the typed DotnetGraphqlEngineOptions pattern instead.");

        // Bind from legacy configuration keys
        options.MaxQueryComplexity = configuration.GetValue("graphql:maxQueryComplexity", options.MaxQueryComplexity);
        options.MaxQueryDepth = configuration.GetValue("graphql:maxQueryDepth", options.MaxQueryDepth);
        options.QueryTimeoutMs = configuration.GetValue("graphql:queryTimeoutMs", options.QueryTimeoutMs);
        options.EnableIntrospection = configuration.GetValue("graphql:enableIntrospection", options.EnableIntrospection);
        options.EnableCaching = configuration.GetValue("graphql:enableCaching", options.EnableCaching);
        options.CacheTTLSeconds = configuration.GetValue("graphql:cacheTTLSeconds", options.CacheTTLSeconds);
        options.EnableSubscriptions = configuration.GetValue("graphql:enableSubscriptions", options.EnableSubscriptions);
        options.MaxSubscriptionConnections = configuration.GetValue("graphql:maxSubscriptionConnections", options.MaxSubscriptionConnections);

        // Bind from new configuration structure (GraphQL section)
        var graphQLConfig = configuration.GetSection("GraphQL");
        if (graphQLConfig.Exists())
        {
            options.ServiceName = graphQLConfig.GetValue(nameof(options.ServiceName), options.ServiceName);
            options.Version = graphQLConfig.GetValue(nameof(options.Version), options.Version);
            options.MaxQueryComplexity = graphQLConfig.GetValue(nameof(options.MaxQueryComplexity), options.MaxQueryComplexity);
            options.MaxQueryDepth = graphQLConfig.GetValue(nameof(options.MaxQueryDepth), options.MaxQueryDepth);
            options.MaxQueryLength = graphQLConfig.GetValue(nameof(options.MaxQueryLength), options.MaxQueryLength);
            options.QueryTimeoutMs = graphQLConfig.GetValue(nameof(options.QueryTimeoutMs), options.QueryTimeoutMs);
            options.MaxQueryFields = graphQLConfig.GetValue(nameof(options.MaxQueryFields), options.MaxQueryFields);
            options.MaxBatchSize = graphQLConfig.GetValue(nameof(options.MaxBatchSize), options.MaxBatchSize);
            options.EnableIntrospection = graphQLConfig.GetValue(nameof(options.EnableIntrospection), options.EnableIntrospection);
            options.EnableCaching = graphQLConfig.GetValue(nameof(options.EnableCaching), options.EnableCaching);
            options.EnableSubscriptions = graphQLConfig.GetValue(nameof(options.EnableSubscriptions), options.EnableSubscriptions);
            options.EnableDataLoading = graphQLConfig.GetValue(nameof(options.EnableDataLoading), options.EnableDataLoading);
            options.EnableSchemaStitching = graphQLConfig.GetValue(nameof(options.EnableSchemaStitching), options.EnableSchemaStitching);
            options.EnableDetailedErrorMessages = graphQLConfig.GetValue(nameof(options.EnableDetailedErrorMessages), options.EnableDetailedErrorMessages);
            options.EnablePerformanceMetrics = graphQLConfig.GetValue(nameof(options.EnablePerformanceMetrics), options.EnablePerformanceMetrics);
            options.EnableFederation = graphQLConfig.GetValue(nameof(options.EnableFederation), options.EnableFederation);
            options.FederationDiscoveryEndpoint = graphQLConfig.GetValue(nameof(options.FederationDiscoveryEndpoint), options.FederationDiscoveryEndpoint);
            options.FederationTimeout = graphQLConfig.GetValue<TimeSpan>(nameof(options.FederationTimeout), options.FederationTimeout);
            options.EntityCacheTtlSeconds = graphQLConfig.GetValue(nameof(options.EntityCacheTtlSeconds), options.EntityCacheTtlSeconds);
            options.EntityCacheMaxSize = graphQLConfig.GetValue(nameof(options.EntityCacheMaxSize), options.EntityCacheMaxSize);
            options.CacheTTLSeconds = graphQLConfig.GetValue(nameof(options.CacheTTLSeconds), options.CacheTTLSeconds);
            options.CacheMaxSize = graphQLConfig.GetValue(nameof(options.CacheMaxSize), options.CacheMaxSize);
            options.CacheMaxSizeBytes = graphQLConfig.GetValue(nameof(options.CacheMaxSizeBytes), options.CacheMaxSizeBytes);
            options.MaxSubscriptionConnections = graphQLConfig.GetValue(nameof(options.MaxSubscriptionConnections), options.MaxSubscriptionConnections);
            options.SubscriptionTimeoutMs = graphQLConfig.GetValue(nameof(options.SubscriptionTimeoutMs), options.SubscriptionTimeoutMs);
            options.HeartbeatIntervalMs = graphQLConfig.GetValue(nameof(options.HeartbeatIntervalMs), options.HeartbeatIntervalMs);
            options.DataLoaderBatchSize = graphQLConfig.GetValue(nameof(options.DataLoaderBatchSize), options.DataLoaderBatchSize);
            options.DataLoaderDelayMs = graphQLConfig.GetValue(nameof(options.DataLoaderDelayMs), options.DataLoaderDelayMs);
            options.EnableRemoteSchemaIntrospection = graphQLConfig.GetValue(nameof(options.EnableRemoteSchemaIntrospection), options.EnableRemoteSchemaIntrospection);
            options.RemoteSchemaTimeoutMs = graphQLConfig.GetValue(nameof(options.RemoteSchemaTimeoutMs), options.RemoteSchemaTimeoutMs);
            options.LogInternalErrors = graphQLConfig.GetValue(nameof(options.LogInternalErrors), options.LogInternalErrors);
            options.IncludeDetailedErrorMessages = graphQLConfig.GetValue(nameof(options.IncludeDetailedErrorMessages), options.IncludeDetailedErrorMessages);
        }

        return options;
    }
}
