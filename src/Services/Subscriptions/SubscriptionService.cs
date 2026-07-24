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
    private readonly SubscriptionResilienceOptions _resilienceOptions;
    private readonly ConcurrentDictionary<string, SubscriptionConnection> _connections = new();
    private readonly ConcurrentDictionary<string, AsyncEventHandler<SubscriptionUpdate>> _handlers = new();
    private readonly ConcurrentDictionary<string, BoundedSubscriberBuffer<SubscriptionUpdate>> _buffers = new();

    /// <summary>
    /// Creates the subscription service using the default resilience settings
    /// (256-item bounded buffer per subscriber, drop-oldest overflow policy).
    /// </summary>
    /// <param name="logger">Logger for connection and delivery diagnostics.</param>
    /// <param name="config">Subscription connection limits and timeouts.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> or <paramref name="config"/> is null.</exception>
    public SubscriptionService(ILogger<SubscriptionService> logger, SubscriptionConfig config)
        : this(logger, config, new SubscriptionResilienceOptions())
    {
    }

    /// <summary>
    /// Creates the subscription service with explicit per-subscriber buffering and
    /// overflow resilience settings.
    /// </summary>
    /// <param name="logger">Logger for connection and delivery diagnostics.</param>
    /// <param name="config">Subscription connection limits and timeouts.</param>
    /// <param name="resilienceOptions">
    /// Per-subscriber buffer capacity and overflow policy applied to every subscription.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/>, <paramref name="config"/> or <paramref name="resilienceOptions"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="resilienceOptions"/> contains invalid values.</exception>
    public SubscriptionService(ILogger<SubscriptionService> logger, SubscriptionConfig config, SubscriptionResilienceOptions resilienceOptions)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(resilienceOptions);
        resilienceOptions.Validate();

        _logger = logger;
        _config = config;
        _resilienceOptions = resilienceOptions;
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

            foreach (var key in _buffers.Keys.Where(k => k.StartsWith($"{clientId}:", StringComparison.Ordinal)).ToList())
            {
                if (_buffers.TryRemove(key, out var buffer))
                    buffer.Dispose();
                _handlers.TryRemove(key, out _);
            }

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

        // A bounded per-subscriber buffer decouples the producer (PublishAsync) from
        // this subscriber's handler, so a slow or stalled client cannot make the
        // buffered items - and therefore process memory - grow without limit.
        var buffer = new BoundedSubscriberBuffer<SubscriptionUpdate>(_resilienceOptions.BufferCapacity, _resilienceOptions.OverflowPolicy);
        _buffers[key] = buffer;
        _ = PumpBufferAsync(key, clientId, eventName, buffer, eventHandler);

        connection.State = SubscriptionState.Active;

        _logger.LogInformation("Client subscribed to event: {ClientId} -> {EventName} with filter: {Filter}",
            clientId, eventName, filterExpression ?? "None");
    }

    /// <summary>
    /// Drains a subscriber's bounded buffer, invoking the subscriber's handler for
    /// each buffered update in order until the buffer completes or is terminated.
    /// </summary>
    /// <param name="key">The internal "{clientId}:{eventName}" subscription key.</param>
    /// <param name="clientId">The subscribing client's identifier.</param>
    /// <param name="eventName">The subscribed event name.</param>
    /// <param name="buffer">The bounded buffer to drain.</param>
    /// <param name="eventHandler">The subscriber's handler to invoke for each item.</param>
    private async Task PumpBufferAsync(
        string key,
        string clientId,
        string eventName,
        BoundedSubscriberBuffer<SubscriptionUpdate> buffer,
        AsyncEventHandler<SubscriptionUpdate> eventHandler)
    {
        try
        {
            await foreach (var update in buffer.ReadAllAsync())
            {
                await eventHandler.InvokeAsync(update).ConfigureAwait(false);
            }
        }
        catch (SubscriptionTerminatedException ex)
        {
            _logger.LogWarning(ex,
                "Subscription stream terminated for {ClientId} -> {EventName}: {ErrorCode}",
                clientId, eventName, ex.ErrorCode);
            CloseConnection(clientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled error delivering subscription updates for {ClientId} -> {EventName}",
                clientId, eventName);
        }
        finally
        {
            _handlers.TryRemove(key, out _);
            _buffers.TryRemove(key, out _);
        }
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

        var subscribersCount = 0;

        foreach (var handlerEntry in _handlers.Where(h => h.Key.EndsWith($":{eventName}")))
        {
            var clientId = handlerEntry.Key.Split(':')[0];
            if (_connections.TryGetValue(clientId, out var connection))
            {
                // Evaluate filter if present
                if (connection.Filter == null || connection.Filter.Evaluate(update.Data))
                {
                    // Publishing writes into the subscriber's bounded buffer rather than
                    // invoking the handler inline: a slow consumer's handler can never
                    // block the producer, and the buffer's overflow policy - not
                    // unbounded queuing - decides what happens when it falls behind.
                    if (_buffers.TryGetValue(handlerEntry.Key, out var buffer))
                        buffer.TryPublish(update);

                    subscribersCount++;
                }
            }
        }

        _logger.LogInformation("Update published to {Count} filtered subscribers: {EventName}",
            subscribersCount, eventName);

        await Task.CompletedTask;
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
