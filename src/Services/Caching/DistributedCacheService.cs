// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace GraphQLEngine.Services.Caching;

/// <summary>
/// Distributed caching service with support for cache invalidation and TTL
/// Uses in-memory implementation with distributed interface
/// </summary>
public class DistributedCacheService : IDisposable
{
    private readonly ILogger<DistributedCacheService> _logger;
    private readonly CacheOptions _options;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;
    private readonly Timer? _cleanupTimer;
    private long _hits = 0;
    private long _misses = 0;

    public DistributedCacheService(
        ILogger<DistributedCacheService> logger,
        CacheOptions? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new CacheOptions();
        _cache = new ConcurrentDictionary<string, CacheEntry>();

        // Start cleanup timer
        if (_options.EnableCleanup)
        {
            _cleanupTimer = new Timer(CleanupExpiredEntries, null,
                TimeSpan.FromMinutes(_options.CleanupIntervalMinutes),
                TimeSpan.FromMinutes(_options.CleanupIntervalMinutes));
        }
    }

    /// <summary>
    /// Gets a value from the cache
    /// </summary>
    public T? Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return default;

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(key, out _);
                _misses++;
                _logger.LogDebug("Cache miss (expired): {Key}", key);
                return default;
            }

            entry.LastAccessedAt = DateTime.UtcNow;
            entry.AccessCount++;
            _hits++;
            _logger.LogDebug("Cache hit: {Key}", key);
            return (T?)entry.Value;
        }

        _misses++;
        _logger.LogDebug("Cache miss: {Key}", key);
        return default;
    }

    /// <summary>
    /// Sets a value in the cache
    /// </summary>
    public void Set<T>(string key, T? value, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        var ttl = expiration ?? TimeSpan.FromMinutes(_options.DefaultTTLMinutes);
        var entry = new CacheEntry
        {
            Key = key,
            Value = value,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(ttl),
            AccessCount = 0
        };

        _cache.AddOrUpdate(key, entry, (_, _) => entry);
        _logger.LogDebug("Cache set: {Key} (TTL: {TTLSeconds}s)", key, ttl.TotalSeconds);
    }

    /// <summary>
    /// Tries to get a value and sets it if not found
    /// </summary>
    public T? GetOrSet<T>(string key, Func<T?> factory, TimeSpan? expiration = null)
    {
        var cached = Get<T>(key);
        if (cached != null)
            return cached;

        var value = factory();
        Set(key, value, expiration);
        return value;
    }

    /// <summary>
    /// Asynchronously gets or sets a value
    /// </summary>
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null)
    {
        var cached = Get<T>(key);
        if (cached != null)
            return cached;

        var value = await factory();
        Set(key, value, expiration);
        return value;
    }

    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    public bool Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var removed = _cache.TryRemove(key, out _);
        if (removed)
            _logger.LogDebug("Cache removed: {Key}", key);

        return removed;
    }

    /// <summary>
    /// Removes multiple values matching a pattern
    /// </summary>
    public int RemovePattern(string pattern)
    {
        var regex = new System.Text.RegularExpressions.Regex(pattern);
        var keysToRemove = _cache.Keys.Where(k => regex.IsMatch(k)).ToList();

        foreach (var key in keysToRemove)
            _cache.TryRemove(key, out _);

        _logger.LogInformation("Cache pattern removed: {Pattern} ({Count} keys)", pattern, keysToRemove.Count);
        return keysToRemove.Count;
    }

    /// <summary>
    /// Clears all cache entries
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        _logger.LogInformation("Cache cleared");
    }

    /// <summary>
    /// Checks if a key exists in cache
    /// </summary>
    public bool Exists(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        if (!_cache.TryGetValue(key, out var entry))
            return false;

        if (entry.IsExpired)
        {
            _cache.TryRemove(key, out _);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        var total = _hits + _misses;
        var hitRate = total > 0 ? (double)_hits / total : 0;

        return new CacheStatistics
        {
            TotalEntries = _cache.Count,
            TotalHits = _hits,
            TotalMisses = _misses,
            HitRate = hitRate,
            AverageAccessCount = _cache.Count > 0
                ? _cache.Values.Average(e => e.AccessCount)
                : 0
        };
    }

    /// <summary>
    /// Sets cache expiration time
    /// </summary>
    public void SetExpiration(string key, TimeSpan expiration)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            entry.ExpiresAt = DateTime.UtcNow.Add(expiration);
            _logger.LogDebug("Cache expiration updated: {Key}", key);
        }
    }

    /// <summary>
    /// Gets remaining TTL for a key
    /// </summary>
    public TimeSpan? GetTimeToLive(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
                return null;

            return entry.ExpiresAt - DateTime.UtcNow;
        }

        return null;
    }

    /// <summary>
    /// Cleans up expired cache entries
    /// </summary>
    private void CleanupExpiredEntries(object? state)
    {
        try
        {
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.IsExpired)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
                _cache.TryRemove(key, out _);

            if (expiredKeys.Count > 0)
                _logger.LogDebug("Cache cleanup: removed {Count} expired entries", expiredKeys.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache cleanup");
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

/// <summary>
/// Cache entry with metadata
/// </summary>
public class CacheEntry
{
    public string Key { get; set; } = string.Empty;
    public object? Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    public long AccessCount { get; set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public TimeSpan Age => DateTime.UtcNow - CreatedAt;
}

/// <summary>
/// Cache options
/// </summary>
public class CacheOptions
{
    public int DefaultTTLMinutes { get; set; } = 30;
    public int MaxEntriesBefore Cleanup { get; set; } = 10000;
    public bool EnableCleanup { get; set; } = true;
    public int CleanupIntervalMinutes { get; set; } = 15;
}

/// <summary>
/// Cache statistics
/// </summary>
public class CacheStatistics
{
    public int TotalEntries { get; set; }
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public double HitRate { get; set; }
    public double AverageAccessCount { get; set; }
}
