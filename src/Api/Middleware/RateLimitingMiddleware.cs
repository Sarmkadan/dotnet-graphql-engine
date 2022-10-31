// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace GraphQLEngine.Api.Middleware;

/// <summary>
/// Rate limiting middleware that enforces request quotas
/// Uses token bucket algorithm for fair rate limiting
/// </summary>
public class RateLimitingMiddleware
{
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;
    private readonly ConcurrentDictionary<string, ClientQuotaInfo> _clientQuotas;
    private readonly Timer? _cleanupTimer;

    public RateLimitingMiddleware(
        ILogger<RateLimitingMiddleware> logger,
        RateLimitOptions? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new RateLimitOptions();
        _clientQuotas = new ConcurrentDictionary<string, ClientQuotaInfo>();

        // Start cleanup timer to remove expired quota entries
        if (_options.EnableCleanup)
        {
            _cleanupTimer = new Timer(CleanupExpiredQuotas, null,
                TimeSpan.FromMinutes(_options.CleanupIntervalMinutes),
                TimeSpan.FromMinutes(_options.CleanupIntervalMinutes));
        }
    }

    /// <summary>
    /// Checks if a request from the given client is allowed under the current rate limit
    /// </summary>
    public RateLimitResult CheckRateLimit(string clientId, int requestCost = 1)
    {
        if (!_options.Enabled)
        {
            return new RateLimitResult { Allowed = true };
        }

        try
        {
            var now = DateTime.UtcNow;
            var quota = _clientQuotas.AddOrUpdate(clientId,
                key => new ClientQuotaInfo
                {
                    ClientId = clientId,
                    CreatedAt = now,
                    LastAccessAt = now,
                    TokensRemaining = _options.RequestsPerMinute,
                    TotalRequests = 1
                },
                (key, existing) => UpdateClientQuota(existing, requestCost, now));

            bool allowed = quota.TokensRemaining >= requestCost;

            if (allowed)
            {
                quota.TokensRemaining -= requestCost;
                quota.AllowedRequests++;
                _logger.LogDebug("Rate limit check passed for {ClientId}: {TokensRemaining} tokens remaining",
                    clientId, quota.TokensRemaining);
            }
            else
            {
                quota.RejectedRequests++;
                _logger.LogWarning("Rate limit exceeded for {ClientId}: {TokensRemaining} tokens remaining, {RequestCost} requested",
                    clientId, quota.TokensRemaining, requestCost);
            }

            return new RateLimitResult
            {
                Allowed = allowed,
                ClientId = clientId,
                TokensRemaining = quota.TokensRemaining,
                ResetAt = quota.NextResetAt,
                RequestCost = requestCost,
                TotalRequests = quota.TotalRequests,
                AllowedRequests = quota.AllowedRequests,
                RejectedRequests = quota.RejectedRequests
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during rate limit check for {ClientId}", clientId);
            // On error, allow the request but log it
            return new RateLimitResult { Allowed = true };
        }
    }

    /// <summary>
    /// Gets current quota information for a client
    /// </summary>
    public ClientQuotaInfo? GetClientQuota(string clientId)
    {
        _clientQuotas.TryGetValue(clientId, out var quota);
        return quota;
    }

    /// <summary>
    /// Resets the quota for a specific client
    /// </summary>
    public void ResetClientQuota(string clientId)
    {
        _clientQuotas.TryRemove(clientId, out _);
        _logger.LogInformation("Rate limit quota reset for {ClientId}", clientId);
    }

    /// <summary>
    /// Updates a client's quota with token bucket algorithm
    /// </summary>
    private ClientQuotaInfo UpdateClientQuota(ClientQuotaInfo existing, int cost, DateTime now)
    {
        var timeSinceLastAccess = (now - existing.LastAccessAt).TotalSeconds;
        var tokensToAdd = (int)(timeSinceLastAccess * (_options.RequestsPerMinute / 60.0));
        var newTokens = Math.Min(
            existing.TokensRemaining + tokensToAdd,
            _options.RequestsPerMinute);

        existing.TokensRemaining = newTokens;
        existing.LastAccessAt = now;
        existing.TotalRequests++;

        // Reset window every minute
        if (now - existing.WindowStartAt >= TimeSpan.FromMinutes(1))
        {
            existing.WindowStartAt = now;
            existing.NextResetAt = now.AddMinutes(1);
        }

        return existing;
    }

    /// <summary>
    /// Periodically cleans up quota entries for inactive clients
    /// </summary>
    private void CleanupExpiredQuotas(object? state)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-_options.InactivityTimeoutMinutes);
            var expiredClients = _clientQuotas
                .Where(kvp => kvp.Value.LastAccessAt < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var clientId in expiredClients)
            {
                _clientQuotas.TryRemove(clientId, out _);
            }

            if (expiredClients.Count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired quota entries", expiredClients.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during quota cleanup");
        }
    }

    /// <summary>
    /// Gets overall statistics for all clients
    /// </summary>
    public RateLimitStatistics GetStatistics()
    {
        return new RateLimitStatistics
        {
            TotalClients = _clientQuotas.Count,
            TotalRequests = _clientQuotas.Values.Sum(q => q.TotalRequests),
            TotalAllowedRequests = _clientQuotas.Values.Sum(q => q.AllowedRequests),
            TotalRejectedRequests = _clientQuotas.Values.Sum(q => q.RejectedRequests),
            AverageTokensPerClient = _clientQuotas.Count > 0
                ? _clientQuotas.Values.Average(q => q.TokensRemaining)
                : 0
        };
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

/// <summary>
/// Rate limiting configuration options
/// </summary>
public class RateLimitOptions
{
    public bool Enabled { get; set; } = true;
    public int RequestsPerMinute { get; set; } = 100;
    public int BurstSize { get; set; } = 150;
    public int InactivityTimeoutMinutes { get; set; } = 60;
    public bool EnableCleanup { get; set; } = true;
    public int CleanupIntervalMinutes { get; set; } = 30;
}

/// <summary>
/// Result of a rate limit check
/// </summary>
public class RateLimitResult
{
    public bool Allowed { get; set; }
    public string? ClientId { get; set; }
    public int TokensRemaining { get; set; }
    public DateTime ResetAt { get; set; }
    public int RequestCost { get; set; }
    public long TotalRequests { get; set; }
    public long AllowedRequests { get; set; }
    public long RejectedRequests { get; set; }

    public Dictionary<string, string> GetResponseHeaders()
    {
        return new Dictionary<string, string>
        {
            { "X-RateLimit-Limit", "100" },
            { "X-RateLimit-Remaining", TokensRemaining.ToString() },
            { "X-RateLimit-Reset", ((DateTimeOffset)ResetAt).ToUnixTimeSeconds().ToString() }
        };
    }
}

/// <summary>
/// Client quota information
/// </summary>
public class ClientQuotaInfo
{
    public string ClientId { get; set; } = string.Empty;
    public int TokensRemaining { get; set; }
    public DateTime WindowStartAt { get; set; } = DateTime.UtcNow;
    public DateTime NextResetAt { get; set; } = DateTime.UtcNow.AddMinutes(1);
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessAt { get; set; }
    public long TotalRequests { get; set; }
    public long AllowedRequests { get; set; }
    public long RejectedRequests { get; set; }
}

/// <summary>
/// Overall rate limiting statistics
/// </summary>
public class RateLimitStatistics
{
    public int TotalClients { get; set; }
    public long TotalRequests { get; set; }
    public long TotalAllowedRequests { get; set; }
    public long TotalRejectedRequests { get; set; }
    public double AverageTokensPerClient { get; set; }
    public float RejectionRate => TotalRequests > 0
        ? (float)TotalRejectedRequests / TotalRequests
        : 0;
}
