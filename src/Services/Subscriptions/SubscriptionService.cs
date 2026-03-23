#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace GraphQLEngine.Services.Subscriptions;

/// <summary>
/// Service for managing GraphQL subscriptions
/// </summary>
sealed public class SubscriptionService
{
    private readonly ILogger<SubscriptionService> _logger;
    private readonly SubscriptionConfig _config;
    private readonly ConcurrentDictionary<string, SubscriptionConnection> _connections = new();
    private readonly ConcurrentDictionary<string, AsyncEventHandler<SubscriptionUpdate>> _handlers = new();

    public SubscriptionService(ILogger<SubscriptionService> logger, SubscriptionConfig config)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Creates a new subscription connection
    /// </summary>
    public SubscriptionConnection CreateConnection(string clientId, string subscriptionQuery)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentException("Client ID cannot be empty", nameof(clientId));

        if (_connections.Count >= _config.MaxConnections)
        {
            _logger.LogWarning("Maximum subscription connections reached: {MaxConnections}", _config.MaxConnections);
            throw new InvalidOperationException("Maximum subscription connections reached");
        }

        var connection = new SubscriptionConnection
        {
            ClientId = clientId,
            SubscriptionQuery = subscriptionQuery,
            ConnectedAt = DateTime.UtcNow
        };

        if (_connections.TryAdd(clientId, connection))
        {
            _logger.LogInformation("Subscription connection created: {ClientId}", clientId);
            return connection;
        }

        throw new InvalidOperationException($"Connection for client {clientId} already exists");
    }

    /// <summary>
    /// Closes a subscription connection
    /// </summary>
    public bool CloseConnection(string clientId)
    {
        if (string.IsNullOrEmpty(clientId)) return false;

        if (_connections.TryRemove(clientId, out var connection))
        {
            connection.State = SubscriptionState.Closed;
            _logger.LogInformation("Subscription connection closed: {ClientId}", clientId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets a connection by client ID
    /// </summary>
    public SubscriptionConnection? GetConnection(string clientId)
    {
        _connections.TryGetValue(clientId, out var connection);
        return connection;
    }

    /// <summary>
    /// Subscribes to updates for a specific event
    /// </summary>
    public void Subscribe(string clientId, string eventName,
        Func<SubscriptionUpdate, Task> handler, string? filterExpression = null) // Added filterExpression
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentException("Client ID cannot be empty", nameof(clientId));

        if (string.IsNullOrEmpty(eventName))
            throw new ArgumentException("Event name cannot be empty", nameof(eventName));

        var connection = GetConnection(clientId) ??
            throw new InvalidOperationException($"Connection for client {clientId} not found");

        // If a filter expression is provided, create and assign a filter
        if (!string.IsNullOrEmpty(filterExpression))
        {
            connection.Filter = new SubscriptionFilter($"filter_{clientId}_{eventName}", filterExpression);
        }

        var key = $"{clientId}:{eventName}";
        var eventHandler = new AsyncEventHandler<SubscriptionUpdate>(handler);

        _handlers.TryAdd(key, eventHandler);
        connection.State = SubscriptionState.Active;

        _logger.LogInformation("Client subscribed to event: {ClientId} -> {EventName} with filter: {Filter}",
            clientId, eventName, filterExpression ?? "None");
    }

    /// <summary>
    /// Publishes an update to all subscribers of an event
    /// </summary>
    public async Task PublishAsync(string eventName, Dictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(eventName))
            throw new ArgumentException("Event name cannot be empty", nameof(eventName));

        var update = new SubscriptionUpdate
        {
            EventName = eventName,
            Data = new Dictionary<string, object>(data),
            PublishedAt = DateTime.UtcNow
        };

        var tasks = new List<Task>();
        var subscribersCount = 0;

        foreach (var handlerEntry in _handlers.Where(h => h.Key.EndsWith($":{eventName}")))
        {
            var clientId = handlerEntry.Key.Split(':')[0];
            if (_connections.TryGetValue(clientId, out var connection))
            {
                // Evaluate filter if present
                if (connection.Filter == null || connection.Filter.Evaluate(update.Data))
                {
                    tasks.Add(handlerEntry.Value.InvokeAsync(update));
                    subscribersCount++;
                }
            }
        }

        try
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation("Update published to {Count} filtered subscribers: {EventName}",
                subscribersCount, eventName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing subscription update: {EventName}", eventName);
        }
    }

    /// <summary>
    /// Gets all active connections
    /// </summary>
    public IEnumerable<SubscriptionConnection> GetActiveConnections()
    {
        return _connections.Values.Where(c => c.State == SubscriptionState.Active).ToList();
    }

    /// <summary>
    /// Gets subscription statistics
    /// </summary>
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "ActiveConnections", _connections.Count },
            { "MaxConnections", _config.MaxConnections },
            { "RegisteredHandlers", _handlers.Count },
            { "HeartbeatInterval", _config.HeartbeatIntervalMs },
            { "Timestamp", DateTime.UtcNow }
        };
    }

    /// <summary>
    /// Closes idle connections
    /// </summary>
    public int CloseIdleConnections(int idleTimeMs = 300000)
    {
        var cutoffTime = DateTime.UtcNow.AddMilliseconds(-idleTimeMs);
        var idleConnections = _connections
            .Where(kv => kv.Value.LastHeartbeat < cutoffTime)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var clientId in idleConnections)
            CloseConnection(clientId);

        _logger.LogInformation("Closed {Count} idle subscription connections", idleConnections.Count);
        return idleConnections.Count;
    }
}

/// <summary>
/// Represents a subscription connection
/// </summary>
sealed public class SubscriptionConnection
{
    public string ClientId { get; set; } = string.Empty;
    public string? SubscriptionQuery { get; set; }
    public SubscriptionState State { get; set; } = SubscriptionState.Pending;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
    public SubscriptionFilter? Filter { get; set; } // New: Per-connection filter
}

/// <summary>
/// Represents a subscription update event
/// </summary>
sealed public class SubscriptionUpdate
{
    public string EventName { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enumeration of subscription states
/// </summary>
public enum SubscriptionState
{
    Pending = 0,
    Active = 1,
    Paused = 2,
    Closed = 3
}

/// <summary>
/// Async event handler for subscription updates
/// </summary>
sealed public class AsyncEventHandler<T>
{
    private readonly Func<T, Task> _handler;

    public AsyncEventHandler(Func<T, Task> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public Task InvokeAsync(T args) => _handler(args);
}
