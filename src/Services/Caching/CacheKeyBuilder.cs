// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace GraphQLEngine.Services.Caching;

/// <summary>
/// Builder for generating cache keys from GraphQL queries and contexts
/// Ensures consistent and collision-free key generation
/// </summary>
public class CacheKeyBuilder
{
    private const string QueryPrefix = "gql:query:";
    private const string SchemaPrefix = "gql:schema:";
    private const string ExecutionPrefix = "gql:exec:";
    private const string MetadataPrefix = "gql:meta:";

    /// <summary>
    /// Builds a cache key for a GraphQL query
    /// </summary>
    public static string BuildQueryKey(string schemaName, string query, Dictionary<string, object>? variables = null)
    {
        if (string.IsNullOrWhiteSpace(schemaName))
            throw new ArgumentException("Schema name cannot be empty", nameof(schemaName));

        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty", nameof(query));

        var normalized = NormalizeQuery(query);
        var hash = HashContent(normalized);

        var key = $"{QueryPrefix}{schemaName}:{hash}";

        // Add variable hash if present
        if (variables != null && variables.Count > 0)
        {
            var variablesHash = HashVariables(variables);
            key = $"{key}:vars:{variablesHash}";
        }

        return key;
    }

    /// <summary>
    /// Builds a cache key for schema metadata
    /// </summary>
    public static string BuildSchemaKey(string schemaName)
    {
        if (string.IsNullOrWhiteSpace(schemaName))
            throw new ArgumentException("Schema name cannot be empty", nameof(schemaName));

        return $"{SchemaPrefix}{schemaName}";
    }

    /// <summary>
    /// Builds a cache key for schema types
    /// </summary>
    public static string BuildSchemaTypesKey(string schemaName)
    {
        return $"{SchemaPrefix}{schemaName}:types";
    }

    /// <summary>
    /// Builds a cache key for a specific type
    /// </summary>
    public static string BuildTypeKey(string schemaName, string typeName)
    {
        if (string.IsNullOrWhiteSpace(schemaName))
            throw new ArgumentException("Schema name cannot be empty", nameof(schemaName));

        if (string.IsNullOrWhiteSpace(typeName))
            throw new ArgumentException("Type name cannot be empty", nameof(typeName));

        return $"{SchemaPrefix}{schemaName}:type:{typeName}";
    }

    /// <summary>
    /// Builds a cache key for query execution results
    /// </summary>
    public static string BuildExecutionKey(string operationId, string operationName)
    {
        if (string.IsNullOrWhiteSpace(operationId))
            throw new ArgumentException("Operation ID cannot be empty", nameof(operationId));

        return $"{ExecutionPrefix}{operationId}:{operationName ?? "anonymous"}";
    }

    /// <summary>
    /// Builds a cache key for metadata
    /// </summary>
    public static string BuildMetadataKey(string identifier, string metaType)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));

        if (string.IsNullOrWhiteSpace(metaType))
            throw new ArgumentException("Metadata type cannot be empty", nameof(metaType));

        return $"{MetadataPrefix}{metaType}:{identifier}";
    }

    /// <summary>
    /// Builds a pattern for wildcard cache invalidation
    /// </summary>
    public static string BuildPatternKey(string prefix, string? pattern = null)
    {
        if (pattern == null)
            return $"^{prefix}.*";

        return $"^{prefix}{System.Text.RegularExpressions.Regex.Escape(pattern)}.*";
    }

    /// <summary>
    /// Normalizes a GraphQL query for consistent caching
    /// </summary>
    private static string NormalizeQuery(string query)
    {
        // Remove extra whitespace
        var normalized = System.Text.RegularExpressions.Regex.Replace(query, @"\s+", " ");

        // Remove comments
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"#.*$", "", System.Text.RegularExpressions.RegexOptions.Multiline);

        // Trim
        return normalized.Trim();
    }

    /// <summary>
    /// Computes a hash of query content
    /// </summary>
    private static string HashContent(string content)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Computes a hash of variables
    /// </summary>
    private static string HashVariables(Dictionary<string, object> variables)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(
            variables.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value));

        return HashContent(json);
    }

    /// <summary>
    /// Extracts schema name from a cache key
    /// </summary>
    public static string? ExtractSchemaName(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey))
            return null;

        var parts = cacheKey.Split(':');
        if (parts.Length >= 3)
            return parts[2];

        return null;
    }

    /// <summary>
    /// Checks if a cache key matches a pattern
    /// </summary>
    public static bool MatchesPattern(string cacheKey, string pattern)
    {
        if (string.IsNullOrEmpty(cacheKey) || string.IsNullOrEmpty(pattern))
            return false;

        try
        {
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            return regex.IsMatch(cacheKey);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a unique key for a request context
    /// </summary>
    public static string BuildContextKey(string userId, string operationId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            userId = "anonymous";

        var contextData = $"{userId}:{operationId}";
        var hash = HashContent(contextData);

        return $"ctx:{hash}";
    }

    /// <summary>
    /// Builds a cache key for rate limiting
    /// </summary>
    public static string BuildRateLimitKey(string clientId, string endpoint)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID cannot be empty", nameof(clientId));

        return $"ratelimit:{clientId}:{endpoint}";
    }

    /// <summary>
    /// Validates if a string is a valid cache key format
    /// </summary>
    public static bool IsValidKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        if (key.Length > 512)
            return false;

        // Check for valid characters
        return System.Text.RegularExpressions.Regex.IsMatch(key, @"^[a-zA-Z0-9:_\-]+$");
    }

    /// <summary>
    /// Gets the key type from a cache key
    /// </summary>
    public static string? GetKeyType(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey))
            return null;

        return cacheKey switch
        {
            _ when cacheKey.StartsWith(QueryPrefix) => "query",
            _ when cacheKey.StartsWith(SchemaPrefix) => "schema",
            _ when cacheKey.StartsWith(ExecutionPrefix) => "execution",
            _ when cacheKey.StartsWith(MetadataPrefix) => "metadata",
            _ when cacheKey.StartsWith("ratelimit:") => "ratelimit",
            _ when cacheKey.StartsWith("ctx:") => "context",
            _ => "unknown"
        };
    }
}
