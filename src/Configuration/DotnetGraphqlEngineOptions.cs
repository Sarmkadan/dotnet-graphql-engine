#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using GraphQLEngine.Common.Constants;

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
}
