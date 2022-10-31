// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Services.BackgroundServices;

/// <summary>
/// Background service for periodic health checks
/// Monitors system health and logs status updates
/// </summary>
public class HealthCheckBackgroundService : IDisposable
{
    private readonly ILogger<HealthCheckBackgroundService> _logger;
    private readonly HealthCheckOptions _options;
    private Timer? _healthCheckTimer;
    private readonly List<HealthCheckResult> _results;
    private DateTime _lastRunTime = DateTime.UtcNow;

    public HealthCheckBackgroundService(
        ILogger<HealthCheckBackgroundService> logger,
        HealthCheckOptions? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new HealthCheckOptions();
        _results = new List<HealthCheckResult>();
    }

    /// <summary>
    /// Starts the background health check service
    /// </summary>
    public void Start()
    {
        if (_healthCheckTimer != null)
        {
            _logger.LogWarning("Health check service is already running");
            return;
        }

        _logger.LogInformation("Starting health check background service (interval: {IntervalSeconds}s)",
            _options.IntervalSeconds);

        // Run first check immediately
        RunHealthCheck();

        // Start periodic timer
        _healthCheckTimer = new Timer(
            _ => RunHealthCheck(),
            null,
            TimeSpan.FromSeconds(_options.IntervalSeconds),
            TimeSpan.FromSeconds(_options.IntervalSeconds));
    }

    /// <summary>
    /// Stops the background health check service
    /// </summary>
    public void Stop()
    {
        if (_healthCheckTimer != null)
        {
            _healthCheckTimer.Dispose();
            _healthCheckTimer = null;
            _logger.LogInformation("Health check background service stopped");
        }
    }

    /// <summary>
    /// Runs a health check
    /// </summary>
    private void RunHealthCheck()
    {
        var startTime = DateTime.UtcNow;
        _logger.LogDebug("Running health check at {Time}", startTime);

        try
        {
            var result = new HealthCheckResult
            {
                CheckedAt = startTime,
                IsHealthy = true,
                Checks = new Dictionary<string, HealthCheckItem>()
            };

            // Check system resources
            CheckSystemResources(result);

            // Check memory usage
            CheckMemoryUsage(result);

            // Check process health
            CheckProcessHealth(result);

            result.Duration = DateTime.UtcNow - startTime;
            _results.Add(result);

            // Keep only recent results
            if (_results.Count > _options.MaxResultsToKeep)
            {
                _results.RemoveAt(0);
            }

            _lastRunTime = DateTime.UtcNow;

            // Log summary
            var failedChecks = result.Checks.Values.Count(c => !c.IsHealthy);
            if (failedChecks == 0)
            {
                _logger.LogInformation("Health check passed ({Duration}ms)", result.Duration.TotalMilliseconds);
            }
            else
            {
                _logger.LogWarning("Health check completed with {FailedCount} failures ({Duration}ms)",
                    failedChecks, result.Duration.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
        }
    }

    /// <summary>
    /// Checks system resource availability
    /// </summary>
    private void CheckSystemResources(HealthCheckResult result)
    {
        try
        {
            var processorCount = Environment.ProcessorCount;
            var isHealthy = processorCount > 0;

            result.Checks["SystemResources"] = new HealthCheckItem
            {
                Name = "System Resources",
                IsHealthy = isHealthy,
                Details = $"Processors: {processorCount}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking system resources");
            result.Checks["SystemResources"] = new HealthCheckItem
            {
                Name = "System Resources",
                IsHealthy = false,
                Error = ex.Message
            };
            result.IsHealthy = false;
        }
    }

    /// <summary>
    /// Checks memory usage
    /// </summary>
    private void CheckMemoryUsage(HealthCheckResult result)
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var peakWorkingSet = process.PeakWorkingSet64;

            // Check if memory usage is reasonable (less than 2GB)
            var isHealthy = workingSet < 2 * 1024 * 1024 * 1024;

            result.Checks["Memory"] = new HealthCheckItem
            {
                Name = "Memory Usage",
                IsHealthy = isHealthy,
                Details = $"Current: {FormatBytes(workingSet)}, Peak: {FormatBytes(peakWorkingSet)}"
            };

            if (!isHealthy)
                result.IsHealthy = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking memory usage");
            result.Checks["Memory"] = new HealthCheckItem
            {
                Name = "Memory Usage",
                IsHealthy = false,
                Error = ex.Message
            };
            result.IsHealthy = false;
        }
    }

    /// <summary>
    /// Checks process health
    /// </summary>
    private void CheckProcessHealth(HealthCheckResult result)
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var threadCount = process.Threads.Count;
            var isResponding = !process.HasExited;

            // Check thread count (warn if > 1000)
            var isHealthy = isResponding && threadCount < 1000;

            result.Checks["Process"] = new HealthCheckItem
            {
                Name = "Process Health",
                IsHealthy = isHealthy,
                Details = $"Threads: {threadCount}, Responding: {isResponding}"
            };

            if (!isHealthy)
                result.IsHealthy = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking process health");
            result.Checks["Process"] = new HealthCheckItem
            {
                Name = "Process Health",
                IsHealthy = false,
                Error = ex.Message
            };
            result.IsHealthy = false;
        }
    }

    /// <summary>
    /// Formats bytes to human-readable string
    /// </summary>
    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##}{sizes[order]}";
    }

    /// <summary>
    /// Gets the latest health check results
    /// </summary>
    public List<HealthCheckResult> GetResults(int limit = 10)
    {
        return _results.TakeLast(limit).ToList();
    }

    /// <summary>
    /// Gets the latest health check result
    /// </summary>
    public HealthCheckResult? GetLatestResult()
    {
        return _results.LastOrDefault();
    }

    /// <summary>
    /// Gets overall health statistics
    /// </summary>
    public HealthCheckStatistics GetStatistics()
    {
        var allHealthy = _results.Count(r => r.IsHealthy);
        var allFailed = _results.Count(r => !r.IsHealthy);

        return new HealthCheckStatistics
        {
            TotalChecks = _results.Count,
            HealthyChecks = allHealthy,
            FailedChecks = allFailed,
            SuccessRate = _results.Count > 0 ? (double)allHealthy / _results.Count : 0,
            LastCheckTime = _lastRunTime
        };
    }

    public void Dispose()
    {
        Stop();
        _results.Clear();
    }
}

/// <summary>
/// Health check result
/// </summary>
public class HealthCheckResult
{
    public DateTime CheckedAt { get; set; }
    public bool IsHealthy { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, HealthCheckItem> Checks { get; set; } = new();
}

/// <summary>
/// Individual health check item
/// </summary>
public class HealthCheckItem
{
    public string Name { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string? Details { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Health check options
/// </summary>
public class HealthCheckOptions
{
    public int IntervalSeconds { get; set; } = 60;
    public int MaxResultsToKeep { get; set; } = 100;
}

/// <summary>
/// Health check statistics
/// </summary>
public class HealthCheckStatistics
{
    public int TotalChecks { get; set; }
    public int HealthyChecks { get; set; }
    public int FailedChecks { get; set; }
    public double SuccessRate { get; set; }
    public DateTime LastCheckTime { get; set; }
}
