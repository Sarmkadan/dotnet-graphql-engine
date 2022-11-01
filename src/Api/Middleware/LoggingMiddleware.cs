// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GraphQLEngine.Api.Middleware;

/// <summary>
/// Logging middleware that tracks and logs detailed request/response information
/// Useful for debugging, monitoring, and audit trails
/// </summary>
public class LoggingMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;
    private readonly LoggingOptions _options;

    public LoggingMiddleware(
        ILogger<LoggingMiddleware> logger,
        LoggingOptions? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new LoggingOptions();
    }

    /// <summary>
    /// Logs a request with all relevant details
    /// </summary>
    public void LogRequest(
        string method,
        string path,
        Dictionary<string, string>? headers = null,
        Dictionary<string, string>? queryParams = null,
        string? body = null)
    {
        if (!_options.Enabled)
            return;

        var sanitizedHeaders = SanitizeHeaders(headers ?? new Dictionary<string, string>());

        if (_options.LogLevel >= LogLevel.Information)
        {
            _logger.LogInformation(
                "Request: {Method} {Path} | Headers: {HeaderCount} | QueryParams: {ParamCount}",
                method, path, sanitizedHeaders.Count, queryParams?.Count ?? 0);
        }

        if (_options.LogLevel >= LogLevel.Debug)
        {
            _logger.LogDebug("Request Headers: {@Headers}", sanitizedHeaders);

            if (queryParams != null && queryParams.Count > 0)
                _logger.LogDebug("Request Query Parameters: {@QueryParams}", queryParams);

            if (_options.LogRequestBody && !string.IsNullOrEmpty(body) && body.Length < 10000)
                _logger.LogDebug("Request Body: {Body}", body);
        }
    }

    /// <summary>
    /// Logs a response with performance metrics
    /// </summary>
    public void LogResponse(
        string method,
        string path,
        int statusCode,
        long durationMs,
        Dictionary<string, string>? headers = null,
        string? body = null)
    {
        if (!_options.Enabled)
            return;

        var level = statusCode >= 500 ? LogLevel.Error
                  : statusCode >= 400 ? LogLevel.Warning
                  : LogLevel.Information;

        if (_options.LogLevel >= level)
        {
            _logger.Log(level,
                "Response: {Method} {Path} -> {StatusCode} ({DurationMs}ms)",
                method, path, statusCode, durationMs);
        }

        if (_options.LogLevel >= LogLevel.Debug)
        {
            var sanitizedHeaders = SanitizeHeaders(headers ?? new Dictionary<string, string>());
            if (sanitizedHeaders.Count > 0)
                _logger.LogDebug("Response Headers: {@Headers}", sanitizedHeaders);

            if (_options.LogResponseBody && !string.IsNullOrEmpty(body) && body.Length < 10000)
                _logger.LogDebug("Response Body: {Body}", body);
        }

        // Alert on slow responses
        if (_options.LogSlowRequests && durationMs > _options.SlowRequestThresholdMs)
        {
            _logger.LogWarning(
                "Slow request detected: {Method} {Path} took {DurationMs}ms (threshold: {Threshold}ms)",
                method, path, durationMs, _options.SlowRequestThresholdMs);
        }
    }

    /// <summary>
    /// Logs GraphQL query execution
    /// </summary>
    public void LogQueryExecution(
        string query,
        string? operationName,
        long durationMs,
        int? errorCount = null)
    {
        if (!_options.Enabled)
            return;

        var truncatedQuery = query.Length > 200
            ? query.Substring(0, 200) + "..."
            : query;

        var message = errorCount.HasValue && errorCount > 0
            ? "Query execution failed: {OperationName} ({ErrorCount} errors) in {DurationMs}ms"
            : "Query execution: {OperationName} in {DurationMs}ms";

        _logger.LogInformation(message, operationName ?? "Anonymous", errorCount ?? 0, durationMs);

        if (_options.LogLevel >= LogLevel.Debug && _options.LogRequestBody)
            _logger.LogDebug("GraphQL Query: {Query}", truncatedQuery);
    }

    /// <summary>
    /// Logs cache operations for debugging
    /// </summary>
    public void LogCacheOperation(string operation, string key, bool hit, long? durationMs = null)
    {
        if (!_options.Enabled || !_options.LogCacheOperations)
            return;

        var message = hit
            ? "Cache HIT: {Operation} key={Key}"
            : "Cache MISS: {Operation} key={Key}";

        if (durationMs.HasValue)
            message += " ({DurationMs}ms)";

        _logger.LogDebug(message, operation, key, durationMs);
    }

    /// <summary>
    /// Logs authentication events
    /// </summary>
    public void LogAuthenticationEvent(string userId, string authType, bool success, string? reason = null)
    {
        if (!_options.Enabled || !_options.LogAuthenticationEvents)
            return;

        var level = success ? LogLevel.Information : LogLevel.Warning;
        var message = success
            ? "Authentication successful: User={UserId} Type={AuthType}"
            : "Authentication failed: User={UserId} Type={AuthType} Reason={Reason}";

        _logger.Log(level, message, userId, authType, reason);
    }

    /// <summary>
    /// Removes sensitive information from headers before logging
    /// </summary>
    private Dictionary<string, string> SanitizeHeaders(Dictionary<string, string> headers)
    {
        var sanitized = new Dictionary<string, string>(headers);

        var sensitiveHeaders = new[] { "Authorization", "X-API-Key", "X-Auth-Token", "Cookie" };
        foreach (var header in sensitiveHeaders)
        {
            if (sanitized.ContainsKey(header))
                sanitized[header] = "[REDACTED]";
        }

        return sanitized;
    }

    /// <summary>
    /// Creates a request context for tracking through the pipeline
    /// </summary>
    public RequestContext CreateRequestContext(string requestId = "")
    {
        return new RequestContext
        {
            RequestId = string.IsNullOrEmpty(requestId) ? Guid.NewGuid().ToString() : requestId,
            StartTime = DateTime.UtcNow,
            StopWatch = Stopwatch.StartNew()
        };
    }
}

/// <summary>
/// Logging configuration options
/// </summary>
public class LoggingOptions
{
    public bool Enabled { get; set; } = true;
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public bool LogRequestBody { get; set; } = false;
    public bool LogResponseBody { get; set; } = false;
    public bool LogSlowRequests { get; set; } = true;
    public int SlowRequestThresholdMs { get; set; } = 1000;
    public bool LogCacheOperations { get; set; } = true;
    public bool LogAuthenticationEvents { get; set; } = true;
    public bool IncludeSensitiveData { get; set; } = false;
}

/// <summary>
/// Request context for tracking requests through the pipeline
/// </summary>
public class RequestContext
{
    public string RequestId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public Stopwatch? StopWatch { get; set; }
    public string? UserId { get; set; }
    public string? ClientId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    public long ElapsedMs => StopWatch?.ElapsedMilliseconds ?? 0;
}

/// <summary>
/// Audit log entry for important operations
/// </summary>
public class AuditLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Action { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Resource { get; set; }
    public string? Details { get; set; }
    public string Status { get; set; } = "success";
    public string? ErrorMessage { get; set; }
}
