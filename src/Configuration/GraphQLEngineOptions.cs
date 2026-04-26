#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Common.Constants;

namespace GraphQLEngine.Configuration;

/// <summary>
/// Configuration options for the GraphQL engine
/// </summary>
sealed public class GraphQLEngineOptions
{
    public string? ServiceName { get; set; } = "dotnet-graphql-engine";
    public string? Version { get; set; } = "1.0.0";

    // Query execution options
    public int MaxQueryComplexity { get; set; } = GraphQLConstants.DefaultMaxQueryComplexity;
    public int MaxQueryDepth { get; set; } = GraphQLConstants.DefaultMaxQueryDepth;
    public int MaxQueryLength { get; set; } = GraphQLConstants.DefaultMaxQueryLength;
    public int QueryTimeoutMs { get; set; } = GraphQLConstants.DefaultQueryTimeoutMs;

    // Feature flags
    public bool EnableIntrospection { get; set; } = true;
    public bool EnableCaching { get; set; } = true;
    public bool EnableSubscriptions { get; set; } = true;
    public bool EnableDataLoading { get; set; } = true;
    public bool EnableSchemaStitching { get; set; } = true;

    // Caching options
    public int CacheTTLSeconds { get; set; } = GraphQLConstants.DefaultCacheTTLSeconds;
    public int CacheMaxSize { get; set; } = GraphQLConstants.DefaultCacheMaxSize;

    // Subscription options
    public int MaxSubscriptionConnections { get; set; } = GraphQLConstants.DefaultMaxSubscriptionConnections;
    public int SubscriptionTimeoutMs { get; set; } = GraphQLConstants.DefaultSubscriptionTimeoutMs;
    public int HeartbeatIntervalMs { get; set; } = GraphQLConstants.DefaultHeartbeatIntervalMs;

    // Data loading options
    public int DataLoaderBatchSize { get; set; } = GraphQLConstants.DefaultDataLoaderBatchSize;
    public int DataLoaderDelayMs { get; set; } = GraphQLConstants.DefaultDataLoaderDelayMs;

    // Logging options
    public bool EnableDetailedErrorMessages { get; set; } = false;
    public bool EnablePerformanceMetrics { get; set; } = true;

    /// <summary>
    /// Validates the configuration options
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (MaxQueryComplexity <= 0)
            errors.Add("MaxQueryComplexity must be greater than 0");

        if (MaxQueryDepth <= 0)
            errors.Add("MaxQueryDepth must be greater than 0");

        if (MaxQueryLength <= 0)
            errors.Add("MaxQueryLength must be greater than 0");

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

        if (CacheMaxSize <= 0)
            errors.Add("CacheMaxSize must be greater than 0");

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
            EnableIntrospection = EnableIntrospection,
            EnableCaching = EnableCaching,
            EnableSubscriptions = EnableSubscriptions,
            EnableDataLoading = EnableDataLoading,
            EnableSchemaStitching = EnableSchemaStitching,
            CacheTTLSeconds = CacheTTLSeconds,
            CacheMaxSize = CacheMaxSize,
            MaxSubscriptionConnections = MaxSubscriptionConnections,
            SubscriptionTimeoutMs = SubscriptionTimeoutMs,
            HeartbeatIntervalMs = HeartbeatIntervalMs,
            DataLoaderBatchSize = DataLoaderBatchSize,
            DataLoaderDelayMs = DataLoaderDelayMs,
            EnableDetailedErrorMessages = EnableDetailedErrorMessages,
            EnablePerformanceMetrics = EnablePerformanceMetrics
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
