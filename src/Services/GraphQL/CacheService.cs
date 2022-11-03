// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Configuration;
using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Services.GraphQL;

/// <summary>
/// Service for caching query results and schema definitions
/// </summary>
public class CacheService
{
    private readonly ILogger<CacheService> _logger;
    private readonly GraphQLEngineOptions _options;
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly object _lockObject = new();

    public CacheService(ILogger<CacheService> logger, GraphQLEngineOptions options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Gets a value from the cache
    /// </summary>
    public object? Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        if (!_options.EnableCaching) return null;

        lock (_lockObject)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                // Check if expired
                if (entry.IsExpired(_options.CacheTTLSeconds))
                {
                    _cache.Remove(key);
                    return null;
                }

                entry.AccessCount++;
                entry.LastAccessedAt = DateTime.UtcNow;
                return entry.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Sets a value in the cache
    /// </summary>
    public void Set(string key, object value)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (value == null) return;
        if (!_options.EnableCaching) return;

        lock (_lockObject)
        {
            // Check cache size limit
            if (_cache.Count >= _options.CacheMaxSize)
            {
                // Remove least recently used entry
                var lruKey = _cache
                    .OrderBy(kv => kv.Value.LastAccessedAt)
                    .FirstOrDefault().Key;

                if (!string.IsNullOrEmpty(lruKey))
                    _cache.Remove(lruKey);
            }

            _cache[key] = new CacheEntry
            {
                Value = value,
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow
            };

            _logger.LogDebug("Cache entry added: {Key}", key);
        }
    }

    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    public bool Remove(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;

        lock (_lockObject)
        {
            return _cache.Remove(key);
        }
    }

    /// <summary>
    /// Checks if a key exists and is not expired
    /// </summary>
    public bool Contains(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;

        lock (_lockObject)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.IsExpired(_options.CacheTTLSeconds))
                {
                    _cache.Remove(key);
                    return false;
                }

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Clears all cache entries
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _cache.Clear();
            _logger.LogInformation("Cache cleared");
        }
    }

    /// <summary>
    /// Removes expired entries from the cache
    /// </summary>
    public int RemoveExpired()
    {
        lock (_lockObject)
        {
            var keysToRemove = _cache
                .Where(kv => kv.Value.IsExpired(_options.CacheTTLSeconds))
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in keysToRemove)
                _cache.Remove(key);

            _logger.LogInformation("Removed {Count} expired cache entries", keysToRemove.Count);
            return keysToRemove.Count;
        }
    }

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    public Dictionary<string, object> GetStatistics()
    {
        lock (_lockObject)
        {
            var totalAccesses = _cache.Values.Sum(e => e.AccessCount);
            var avgAccesses = _cache.Count > 0 ? totalAccesses / _cache.Count : 0;

            return new Dictionary<string, object>
            {
                { "CacheSize", _cache.Count },
                { "MaxSize", _options.CacheMaxSize },
                { "TotalAccesses", totalAccesses },
                { "AverageAccesses", avgAccesses },
                { "TTL", _options.CacheTTLSeconds },
                { "Enabled", _options.EnableCaching },
                { "Timestamp", DateTime.UtcNow }
            };
        }
    }

    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public int AccessCount { get; set; }

        public bool IsExpired(int ttlSeconds)
        {
            return (DateTime.UtcNow - CreatedAt).TotalSeconds > ttlSeconds;
        }
    }
}
