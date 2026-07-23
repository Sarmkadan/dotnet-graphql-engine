#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Common.Constants;
using GraphQLEngine.Configuration;
using GraphQLEngine.Services.Caching;
using GraphQLEngine.Data.Repositories;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Exceptions;
using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Services.GraphQL;

/// <summary>
/// Implements the Automatic Persisted Queries (APQ) protocol.
/// On first contact a client sends the full query document together with its SHA-256 hash;
/// the service stores it and future requests may send only the hash, saving bandwidth.
/// An in-process hash index provides O(1) lookups; the repository is the durable store and
/// the authoritative fallback for warm-start / multi-instance deployments.
/// When <see cref="PersistedQueryOptions.AllowlistOnly"/> is enabled, ad-hoc query documents
/// are rejected — only queries registered in advance are permitted.
/// </summary>
sealed public class PersistedQueryService
{
    private readonly IRepository<PersistedQuery> _repository;
    private readonly ILogger<PersistedQueryService> _logger;
    private readonly PersistedQueryOptions _options;

    // Fast bounded hash→entity index; the repository remains source of truth.
    // LRU eviction caps memory even when clients register unlimited unique queries.
    private readonly ICacheStore<string, PersistedQuery> _hashIndex;

    /// <summary>
    /// Initialises the service with the required repository and logger.
    /// </summary>
    /// <param name="repository">Durable store for persisted queries.</param>
    /// <param name="logger">Diagnostic logger.</param>
    /// <param name="options">Optional behaviour options; defaults are used when omitted.</param>
    /// <param name="hashIndex">
    /// Optional cache store for the in-process hash index. When omitted, a
    /// <see cref="LruCacheStore{TKey, TValue}"/> bounded by
    /// <see cref="PersistedQueryOptions.MaxIndexSize"/> (falling back to
    /// <see cref="GraphQLConstants.PersistedQueryMaxEntries"/>) is used.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="repository"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    public PersistedQueryService(
        IRepository<PersistedQuery> repository,
        ILogger<PersistedQueryService> logger,
        PersistedQueryOptions? options = null,
        ICacheStore<string, PersistedQuery>? hashIndex = null)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);

        _repository = repository;
        _logger = logger;
        _options = options ?? new PersistedQueryOptions();

        var maxEntries = _options.MaxIndexSize > 0
            ? _options.MaxIndexSize
            : GraphQLConstants.PersistedQueryMaxEntries;

        _hashIndex = hashIndex ?? new LruCacheStore<string, PersistedQuery>(
            maxEntries, keyComparer: StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Registers a query document for APQ. If a record with the same hash already exists the
    /// existing entry is returned unchanged — registration is idempotent.
    /// </summary>
    /// <param name="queryString">Full GraphQL document (query / mutation / subscription).</param>
    /// <param name="schemaName">Target schema name; defaults to <c>"default"</c>.</param>
    /// <param name="operationName">Optional operation name embedded in the document.</param>
    /// <param name="cancellationToken">Propagates cancellation to async operations.</param>
    /// <returns>The newly registered or pre-existing <see cref="PersistedQuery"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="queryString"/> is empty.</exception>
    public async Task<PersistedQuery> RegisterAsync(
        string queryString,
        string schemaName = "default",
        string? operationName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queryString))
            throw new ArgumentException("Query string cannot be empty", nameof(queryString));

        var hash = PersistedQuery.ComputeHash(queryString);

        if (_hashIndex.TryGetValue(hash, out var cached))
        {
            _logger.LogDebug("Persisted query already registered: {Hash}", hash);
            return cached;
        }

        var persisted = new PersistedQuery
        {
            Hash = hash,
            QueryString = queryString,
            SchemaName = string.IsNullOrWhiteSpace(schemaName) ? "default" : schemaName,
            OperationName = operationName
        };

        cancellationToken.ThrowIfCancellationRequested();
        await _repository.AddAsync(persisted);
        _hashIndex[hash] = persisted;

        _logger.LogInformation(
            "Registered persisted query {Hash} on schema {SchemaName}",
            hash, persisted.SchemaName);

        return persisted;
    }

    /// <summary>
    /// Retrieves a persisted query by its APQ hash.
    /// Returns <c>null</c> when no matching record exists — callers should respond to the
    /// client with a <c>PERSISTED_QUERY_NOT_FOUND</c> error and instruct it to resend
    /// the full document.
    /// </summary>
    /// <param name="hash">Lowercase hex SHA-256 hash supplied by the client.</param>
    /// <param name="cancellationToken">Propagates cancellation to async operations.</param>
    /// <returns>The matching <see cref="PersistedQuery"/>, or <c>null</c> if not found.</returns>
    public async Task<PersistedQuery?> GetByHashAsync(
        string hash,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return null;

        if (_hashIndex.TryGetValue(hash, out var cached))
        {
            _logger.LogDebug("APQ cache hit: {Hash}", hash);
            cached.RecordExecution();
            return cached;
        }

        // Repository fallback covers warm-start and multi-instance scenarios
        cancellationToken.ThrowIfCancellationRequested();
        var all = await _repository.GetAllAsync();
        var found = all.FirstOrDefault(q =>
            string.Equals(q.Hash, hash, StringComparison.OrdinalIgnoreCase));

        if (found is null)
        {
            _logger.LogDebug("APQ cache miss — query not found: {Hash}", hash);
            return null;
        }

        _hashIndex[found.Hash] = found;
        found.RecordExecution();

        _logger.LogDebug("APQ cache miss — populated from repository: {Hash}", hash);
        return found;
    }

    /// <summary>
    /// Verifies that <paramref name="hash"/> is the correct SHA-256 digest of
    /// <paramref name="queryString"/>. Throws when they diverge so callers can reject
    /// tampered or corrupted APQ payloads before storing them.
    /// </summary>
    /// <param name="hash">Hash value asserted by the client.</param>
    /// <param name="queryString">Full query document to verify.</param>
    /// <exception cref="ArgumentException">Thrown when either argument is empty.</exception>
    /// <exception cref="ValidationException">Thrown when the computed hash does not match.</exception>
    public void ValidateHash(string hash, string queryString)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be empty", nameof(hash));

        if (string.IsNullOrWhiteSpace(queryString))
            throw new ArgumentException("Query string cannot be empty", nameof(queryString));

        var computed = PersistedQuery.ComputeHash(queryString);

        if (!string.Equals(computed, hash, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "APQ hash mismatch — client sent {ClientHash}, computed {ComputedHash}",
                hash, computed);

            throw new ValidationException(
                $"Persisted query hash mismatch: expected {hash}, computed {computed}",
                new List<string>
                {
                    $"Provided hash '{hash}' does not match the SHA-256 digest of the supplied query document."
                });
        }
    }

    /// <summary>
    /// Returns all registered persisted queries, optionally filtered to a specific schema.
    /// </summary>
    /// <param name="schemaName">
    /// When provided, only queries registered against this schema are returned.
    /// Pass <c>null</c> or an empty string to retrieve all schemas.
    /// </param>
    /// <param name="cancellationToken">Propagates cancellation to async operations.</param>
    /// <returns>Read-only list of matching <see cref="PersistedQuery"/> records.</returns>
    public async Task<IReadOnlyList<PersistedQuery>> ListAsync(
        string? schemaName = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var all = await _repository.GetAllAsync();

        return string.IsNullOrWhiteSpace(schemaName)
            ? all.ToList()
            : all.Where(q => string.Equals(q.SchemaName, schemaName, StringComparison.OrdinalIgnoreCase))
                 .ToList();
    }

    /// <summary>
    /// Removes the persisted query identified by <paramref name="hash"/>.
    /// </summary>
    /// <param name="hash">SHA-256 hash of the query to remove.</param>
    /// <param name="cancellationToken">Propagates cancellation to async operations.</param>
    /// <returns><c>true</c> if the entry was found and deleted; <c>false</c> otherwise.</returns>
    public async Task<bool> RemoveAsync(string hash, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return false;

        _hashIndex.TryRemove(hash, out _);

        cancellationToken.ThrowIfCancellationRequested();
        var all = await _repository.GetAllAsync();
        var target = all.FirstOrDefault(q =>
            string.Equals(q.Hash, hash, StringComparison.OrdinalIgnoreCase));

        if (target is null)
            return false;

        await _repository.DeleteAsync(target.Id);

        _logger.LogInformation("Removed persisted query {Hash}", hash);
        return true;
    }

    /// <summary>
    /// Returns the total number of registered persisted queries across all schemas.
    /// </summary>
    /// <param name="cancellationToken">Propagates cancellation to async operations.</param>
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _repository.CountAsync();
    }

    /// <summary>
    /// Determines whether a query document is allowed to execute.
    /// When <see cref="PersistedQueryOptions.AllowlistOnly"/> is <c>false</c> (the default),
    /// every query is allowed. When <c>true</c>, the document must already be registered as
    /// a persisted query; otherwise execution is denied.
    /// </summary>
    /// <param name="queryString">The full GraphQL document submitted by the client.</param>
    /// <param name="cancellationToken">Propagates cancellation to async operations.</param>
    /// <returns>
    /// <c>true</c> when the query is permitted to run; <c>false</c> when allowlist mode is
    /// active and the document is not in the persisted-query store.
    /// </returns>
    public async Task<bool> IsAllowedAsync(
        string queryString,
        CancellationToken cancellationToken = default)
    {
        if (!_options.AllowlistOnly)
            return true;

        if (string.IsNullOrWhiteSpace(queryString))
            return false;

        var hash = PersistedQuery.ComputeHash(queryString);

        if (_hashIndex.ContainsKey(hash))
            return true;

        cancellationToken.ThrowIfCancellationRequested();
        var all = await _repository.GetAllAsync();
        var found = all.Any(q =>
            string.Equals(q.Hash, hash, StringComparison.OrdinalIgnoreCase));

        if (!found)
        {
            _logger.LogWarning(
                "Allowlist rejection — query with hash {Hash} is not a registered persisted query",
                hash);
        }

        return found;
    }
}
