#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Represents a persisted GraphQL query registered via the Automatic Persisted Queries (APQ) protocol.
/// Clients send only a SHA-256 hash on repeat requests; the full document is stored on first
/// registration and looked up by hash on subsequent calls.
/// </summary>
sealed public class PersistedQuery
{
    /// <summary>
    /// Unique identifier for this persisted-query record.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Lowercase hex SHA-256 hash of the query document — the APQ lookup key.
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Full GraphQL query document (query / mutation / subscription).
    /// </summary>
    public string QueryString { get; set; } = string.Empty;

    /// <summary>
    /// Name of the schema this query targets; defaults to <c>"default"</c>.
    /// </summary>
    public string SchemaName { get; set; } = "default";

    /// <summary>
    /// Optional operation name embedded in the query document.
    /// </summary>
    public string? OperationName { get; set; }

    /// <summary>
    /// UTC timestamp of first registration.
    /// </summary>
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp of the most recent execution, or <c>null</c> if never executed.
    /// </summary>
    public DateTime? LastExecutedAt { get; private set; }

    /// <summary>
    /// Total number of times this persisted query has been executed.
    /// </summary>
    public long ExecutionCount { get; private set; }

    /// <summary>
    /// Records a single execution, incrementing the counter and updating the timestamp.
    /// </summary>
    public void RecordExecution()
    {
        ExecutionCount++;
        LastExecutedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifies that the stored <see cref="Hash"/> matches the current <see cref="QueryString"/>.
    /// Returns <c>false</c> if either property is empty or the hashes diverge.
    /// </summary>
    public bool ValidateHash()
    {
        if (string.IsNullOrEmpty(QueryString) || string.IsNullOrEmpty(Hash))
            return false;

        var computed = ComputeHash(QueryString);
        return string.Equals(computed, Hash, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Computes the APQ SHA-256 hash for a query document.
    /// The hash is a lowercase hex string, matching the format expected by Apollo-compatible clients.
    /// </summary>
    /// <param name="query">The raw GraphQL document string.</param>
    /// <returns>64-character lowercase hex SHA-256 digest.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> is null or whitespace.</exception>
    public static string ComputeHash(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty", nameof(query));

        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(query));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
