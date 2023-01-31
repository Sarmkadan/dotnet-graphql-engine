#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Services.Caching;
using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Services.BackgroundServices;

/// <summary>
/// Background service for cache maintenance and optimization
/// Handles cache invalidation, cleanup, and statistics collection
/// </summary>
sealed public class CacheMaintenanceBackgroundService : IDisposable
{
    private readonly DistributedCacheService _cacheService;
    private readonly ILogger<CacheMaintenanceBackgroundService> _logger;
    private readonly CacheMaintenanceOptions _options;
    private Timer? _maintenanceTimer;
    private readonly List<CacheMaintenanceLog> _logs;

    public CacheMaintenanceBackgroundService(
        DistributedCacheService cacheService,
        ILogger<CacheMaintenanceBackgroundService> logger,
        CacheMaintenanceOptions? options = null)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new CacheMaintenanceOptions();
        _logs = new List<CacheMaintenanceLog>();
    }

    /// <summary>
    /// Starts the cache maintenance background service
    /// </summary>
    public void Start()
    {
        if (_maintenanceTimer is not null)
        {
            _logger.LogWarning("Cache maintenance service is already running");
            return;
        }

        _logger.LogInformation("Starting cache maintenance background service (interval: {IntervalMinutes}m)",
            _options.IntervalMinutes);

        // Start periodic timer
        _maintenanceTimer = new Timer(
            _ => RunMaintenance(),
            null,
            TimeSpan.FromMinutes(_options.IntervalMinutes),
            TimeSpan.FromMinutes(_options.IntervalMinutes));
    }

    /// <summary>
    /// Stops the cache maintenance service
    /// </summary>
    public void Stop()
    {
        if (_maintenanceTimer is not null)
        {
            _maintenanceTimer.Dispose();
            _maintenanceTimer = null;
            _logger.LogInformation("Cache maintenance background service stopped");
        }
    }

    /// <summary>
    /// Runs cache maintenance tasks
    /// </summary>
    private void RunMaintenance()
    {
        var startTime = DateTime.UtcNow;
        _logger.LogDebug("Running cache maintenance at {Time}", startTime);

        try
        {
            var log = new CacheMaintenanceLog
            {
                StartTime = startTime,
                Tasks = new Dictionary<string, MaintenanceTaskResult>()
            };

            // Get pre-maintenance stats
            var statsBefore = _cacheService.GetStatistics();

            // Run maintenance tasks
            if (_options.EnableStatisticsCollection)
                CollectStatistics(log);

            if (_options.EnableCacheWarming)
                WarmCache(log);

            if (_options.EnableInvalidationCheck)
                CheckForInvalidations(log);

            // Get post-maintenance stats
            var statsAfter = _cacheService.GetStatistics();

            log.EndTime = DateTime.UtcNow;
            log.Duration = log.EndTime.Value - startTime;
            log.StatisticsBefore = statsBefore;
            log.StatisticsAfter = statsAfter;

            _logs.Add(log);

            // Keep only recent logs
            if (_logs.Count > _options.MaxLogsToKeep)
            {
                _logs.RemoveAt(0);
            }

            _logger.LogInformation("Cache maintenance completed ({Duration}ms). " +
                "Hit Rate: {HitRate:P}, Cache Size: {CacheSize}",
                log.Duration?.TotalMilliseconds ?? 0,
                statsAfter.HitRate,
                statsAfter.TotalEntries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache maintenance");
        }
    }

    /// <summary>
    /// Collects cache statistics
    /// </summary>
    private void CollectStatistics(CacheMaintenanceLog log)
    {
        try
        {
            var stats = _cacheService.GetStatistics();

            log.Tasks["Statistics"] = new MaintenanceTaskResult
            {
                TaskName = "Collect Statistics",
                Success = true,
                Details = new Dictionary<string, object>
                {
                    { "totalEntries", stats.TotalEntries },
                    { "hitRate", stats.HitRate },
                    { "hits", stats.TotalHits },
                    { "misses", stats.TotalMisses }
                }
            };

            _logger.LogDebug("Cache statistics collected: {Entries} entries, {HitRate:P} hit rate",
                stats.TotalEntries, stats.HitRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting cache statistics");
            log.Tasks["Statistics"] = new MaintenanceTaskResult
            {
                TaskName = "Collect Statistics",
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Warms up cache by preloading common queries
    /// </summary>
    private void WarmCache(CacheMaintenanceLog log)
    {
        try
        {
            var count = 0;

            // Refresh the TTL of configured hot keys so they survive between
            // maintenance cycles instead of expiring under load
            if (_options.WarmCommonPatterns && _options.WarmupKeys.Count > 0)
            {
                var extension = TimeSpan.FromMinutes(_options.WarmupTtlMinutes);
                foreach (var key in _options.WarmupKeys)
                {
                    if (!_cacheService.Exists(key))
                        continue;

                    var remaining = _cacheService.GetTimeToLive(key);
                    if (remaining is null || remaining < extension)
                    {
                        _cacheService.SetExpiration(key, extension);
                        count++;
                    }
                }

                _logger.LogDebug("Cache warming refreshed {Count} of {Total} configured keys",
                    count, _options.WarmupKeys.Count);
            }
            else
            {
                _logger.LogDebug("Cache warming skipped (no patterns configured)");
            }

            log.Tasks["CacheWarming"] = new MaintenanceTaskResult
            {
                TaskName = "Cache Warming",
                Success = true,
                Details = new Dictionary<string, object>
                {
                    { "preloadedEntries", count }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error warming cache");
            log.Tasks["CacheWarming"] = new MaintenanceTaskResult
            {
                TaskName = "Cache Warming",
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Checks for cache invalidations
    /// </summary>
    private void CheckForInvalidations(CacheMaintenanceLog log)
    {
        try
        {
            // Evict entries matching the configured invalidation patterns
            var invalidated = 0;
            foreach (var pattern in _options.InvalidationPatterns)
            {
                invalidated += _cacheService.RemovePattern(pattern);
            }

            log.Tasks["InvalidationCheck"] = new MaintenanceTaskResult
            {
                TaskName = "Check Invalidations",
                Success = true,
                Details = new Dictionary<string, object>
                {
                    { "invalidatedEntries", invalidated }
                }
            };

            _logger.LogDebug("Invalidation check completed: {Count} entries evicted", invalidated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache invalidations");
            log.Tasks["InvalidationCheck"] = new MaintenanceTaskResult
            {
                TaskName = "Check Invalidations",
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Gets maintenance logs
    /// </summary>
    public List<CacheMaintenanceLog> GetLogs(int limit = 10)
    {
        return _logs.TakeLast(limit).ToList();
    }

    /// <summary>
    /// Gets the latest maintenance log
    /// </summary>
    public CacheMaintenanceLog? GetLatestLog()
    {
        return _logs.LastOrDefault();
    }

    public void Dispose()
    {
        Stop();
        _logs.Clear();
    }
}

/// <summary>
/// Cache maintenance log entry
/// </summary>
sealed public class CacheMaintenanceLog
{
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public Dictionary<string, MaintenanceTaskResult> Tasks { get; set; } = new();
    public CacheStatistics? StatisticsBefore { get; set; }
    public CacheStatistics? StatisticsAfter { get; set; }
}

/// <summary>
/// Maintenance task result
/// </summary>
sealed public class MaintenanceTaskResult
{
    public string TaskName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public Dictionary<string, object>? Details { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Cache maintenance options
/// </summary>
sealed public class CacheMaintenanceOptions
{
    public int IntervalMinutes { get; set; } = 30;
    public bool EnableStatisticsCollection { get; set; } = true;
    public bool EnableCacheWarming { get; set; } = false;
    public bool EnableInvalidationCheck { get; set; } = true;
    public bool WarmCommonPatterns { get; set; } = false;
    public int MaxLogsToKeep { get; set; } = 50;
    public List<string> WarmupKeys { get; set; } = new();
    public int WarmupTtlMinutes { get; set; } = 60;
    public List<string> InvalidationPatterns { get; set; } = new();
}
