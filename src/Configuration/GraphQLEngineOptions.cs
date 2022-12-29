#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using GraphQLEngine.Common.Constants;

namespace GraphQLEngine.Configuration;

/// <summary>
/// Configuration options for the GraphQL engine
/// </summary>
sealed public class GraphQLEngineOptions
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
    /// <summary>
    /// Enable GraphQL introspection (allows schema exploration)
    /// </summary>
    public bool EnableIntrospection { get; set; } = true;

    /// <summary>
    /// Enable query result caching
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Enable GraphQL subscriptions
    /// </summary>
    public bool EnableSubscriptions { get; set; } = true;

    /// <summary>
    /// Enable DataLoader for batching and caching
    /// </summary>
    public bool EnableDataLoading { get; set; } = true;

    /// <summary>
    /// Enable schema stitching for combining multiple schemas
    /// </summary>
    public bool EnableSchemaStitching { get; set; } = true;

    /// <summary>
    /// Enable detailed error messages in responses (disable in production for security)
    /// </summary>
    public bool EnableDetailedErrorMessages { get; set; } = false;

    /// <summary>
    /// Enable performance metrics collection
    /// </summary>
    public bool EnablePerformanceMetrics { get; set; } = true;

    // Federation options
    /// <summary>
    /// Enable GraphQL Federation support for cross-service entity resolution
    /// </summary>
    public bool EnableFederation { get; set; } = false;

    /// <summary>
    /// Endpoint used to discover and compose federated schemas
    /// </summary>
    public string FederationDiscoveryEndpoint { get; set; } = "/.well-known/federation";

    /// <summary>
    /// Timeout for federation discovery and entity resolution calls
    /// </summary>
    public TimeSpan FederationTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Entity cache time-to-live in seconds (federation entity resolution)
    /// </summary>
    [Range(0, 86400)]
    public int EntityCacheTtlSeconds { get; set; } = GraphQLConstants.DefaultCacheTTLSeconds;

    /// <summary>
    /// Maximum number of entries allowed in the entity cache (federation entity resolution)
    /// </summary>
    [Range(1, 1000000)]
    public int EntityCacheMaxSize { get; set; } = GraphQLConstants.DefaultCacheMaxSize;

    // Caching options
    /// <summary>
    /// Cache time-to-live in seconds
    /// </summary>
    [Range(0, 86400)]
    public int CacheTTLSeconds { get; set; } = GraphQLConstants.DefaultCacheTTLSeconds;

    /// <summary>
    /// Maximum number of entries allowed in the cache
    /// </summary>
    [Range(1, 1000000)]
    public int CacheMaxSize { get; set; } = GraphQLConstants.DefaultCacheMaxSize;

    /// <summary>
    /// Maximum cache size in bytes
    /// </summary>
    [Range(1024, 1073741824)] // 1KB to 1GB
    public int CacheMaxSizeBytes { get; set; } = 52428800; // 50MB

    // Subscription options
    /// <summary>
    /// Maximum concurrent subscription connections
    /// </summary>
    [Range(1, 10000)]
    public int MaxSubscriptionConnections { get; set; } = GraphQLConstants.DefaultMaxSubscriptionConnections;

    /// <summary>
    /// Subscription connection timeout in milliseconds
    /// </summary>
    [Range(1000, 300000)]
    public int SubscriptionTimeoutMs { get; set; } = GraphQLConstants.DefaultSubscriptionTimeoutMs;

    /// <summary>
    /// Heartbeat interval for subscription connections in milliseconds
    /// </summary>
    [Range(1000, 60000)]
    public int HeartbeatIntervalMs { get; set; } = GraphQLConstants.DefaultHeartbeatIntervalMs;

    // Data loading options
    /// <summary>
    /// DataLoader batch size for batching operations
    /// </summary>
    [Range(1, 1000)]
    public int DataLoaderBatchSize { get; set; } = GraphQLConstants.DefaultDataLoaderBatchSize;

    /// <summary>
    /// DataLoader delay in milliseconds before flushing batches
    /// </summary>
    [Range(0, 1000)]
    public int DataLoaderDelayMs { get; set; } = GraphQLConstants.DefaultDataLoaderDelayMs;

    // Remote schema options
    /// <summary>
    /// Enable remote schema introspection
    /// </summary>
    public bool EnableRemoteSchemaIntrospection { get; set; } = true;

    /// <summary>
    /// Remote schema introspection timeout in milliseconds
    /// </summary>
    [Range(1000, 60000)]
    public int RemoteSchemaTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Enable logging of internal errors
    /// </summary>
    public bool LogInternalErrors { get; set; } = true;

    /// <summary>
    /// Include detailed error messages in GraphQL responses
    /// </summary>
    public bool IncludeDetailedErrorMessages { get; set; } = false;

    /// <summary>
    /// Validates the configuration options using DataAnnotations
    /// </summary>
    /// <returns>List of validation errors, empty if valid</returns>
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

        // Additional custom validation
        if (MaxQueryComplexity <= 0)
            errors.Add("MaxQueryComplexity must be greater than 0");

        if (MaxQueryDepth <= 0)
            errors.Add("MaxQueryDepth must be greater than 0");

        if (QueryTimeoutMs <= 0)
            errors.Add("QueryTimeoutMs must be greater than 0");

        if (MaxSubscriptionConnections <= 0)
            errors.Add("MaxSubscriptionConnections must be greater than 0");

        if (SubscriptionTimeoutMs <= 0)
            errors.Add("SubscriptionTimeoutMs must be greater than 0");

        if (HeartbeatIntervalMs <= 0)
            errors.Add("HeartbeatIntervalMs must be greater than 0");

        if (CacheTTLSeconds < 0)
            errors.Add("CacheTTLSeconds cannot be negative");

        if (CacheMaxSizeBytes <= 0)
            errors.Add("CacheMaxSizeBytes must be greater than 0");

        return errors;
    }

    /// <summary>
    /// Validates the configuration options (backward compatibility)
    /// </summary>
    [Obsolete("Use Validate() instead, this method will be removed in future versions")]
    public bool Validate(out List<string> errors)
    {
        var validationErrors = Validate();
        errors = validationErrors;
        return errors.Count == 0;
    }

    /// <summary>
    /// Creates a copy of the options
    /// </summary>
    public GraphQLEngineOptions Clone()
    {
        return new GraphQLEngineOptions
        {
            ServiceName = ServiceName,
            Version = Version,
            MaxQueryComplexity = MaxQueryComplexity,
            MaxQueryDepth = MaxQueryDepth,
            MaxQueryLength = MaxQueryLength,
            QueryTimeoutMs = QueryTimeoutMs,
            MaxQueryFields = MaxQueryFields,
            MaxBatchSize = MaxBatchSize,
            EnableIntrospection = EnableIntrospection,
            EnableCaching = EnableCaching,
            EnableSubscriptions = EnableSubscriptions,
            EnableDataLoading = EnableDataLoading,
            EnableSchemaStitching = EnableSchemaStitching,
            EnableDetailedErrorMessages = EnableDetailedErrorMessages,
            EnablePerformanceMetrics = EnablePerformanceMetrics,
            EnableFederation = EnableFederation,
            FederationDiscoveryEndpoint = FederationDiscoveryEndpoint,
            FederationTimeout = FederationTimeout,
            EntityCacheTtlSeconds = EntityCacheTtlSeconds,
            EntityCacheMaxSize = EntityCacheMaxSize,
            CacheTTLSeconds = CacheTTLSeconds,
            CacheMaxSize = CacheMaxSize,
            CacheMaxSizeBytes = CacheMaxSizeBytes,
            MaxSubscriptionConnections = MaxSubscriptionConnections,
            SubscriptionTimeoutMs = SubscriptionTimeoutMs,
            HeartbeatIntervalMs = HeartbeatIntervalMs,
            DataLoaderBatchSize = DataLoaderBatchSize,
            DataLoaderDelayMs = DataLoaderDelayMs,
            EnableRemoteSchemaIntrospection = EnableRemoteSchemaIntrospection,
            RemoteSchemaTimeoutMs = RemoteSchemaTimeoutMs,
            LogInternalErrors = LogInternalErrors,
            IncludeDetailedErrorMessages = IncludeDetailedErrorMessages
        };
    }

    /// <summary>
    /// Gets a summary of the configuration
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