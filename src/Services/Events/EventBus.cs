#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Services.Events;

/// <summary>
/// Event bus for pub/sub pattern
/// Enables loosely coupled event publishing and subscription
/// </summary>
sealed public class EventBus : IDisposable
{
    private readonly Dictionary<string, List<Delegate>> _subscriptions;
    private readonly ILogger<EventBus> _logger;
    private readonly List<EventLog> _eventLog;
    private readonly int _maxLogSize;

    public EventBus(ILogger<EventBus> logger, int maxLogSize = 1000)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subscriptions = new Dictionary<string, List<Delegate>>();
        _eventLog = new List<EventLog>();
        _maxLogSize = maxLogSize;
    }

    /// <summary>
    /// Subscribes to an event type
    /// </summary>
    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        var eventType = typeof(TEvent).Name;

        if (!_subscriptions.ContainsKey(eventType))
            _subscriptions[eventType] = new List<Delegate>();

        _subscriptions[eventType].Add(handler);
        _logger.LogDebug("Subscribed to event {EventType}", eventType);
    }

    /// <summary>
    /// Subscribes to an event with async handler
    /// </summary>
    public void SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        var eventType = typeof(TEvent).Name;

        if (!_subscriptions.ContainsKey(eventType))
            _subscriptions[eventType] = new List<Delegate>();

        _subscriptions[eventType].Add(handler);
        _logger.LogDebug("Subscribed to async event {EventType}", eventType);
    }

    /// <summary>
    /// Publishes an event to all subscribers
    /// </summary>
    public void Publish<TEvent>(TEvent @event) where TEvent : class
    {
        var eventType = typeof(TEvent).Name;
        _logger.LogInformation("Publishing event {EventType}", eventType);

        LogEvent(@event);

        if (!_subscriptions.ContainsKey(eventType))
        {
            _logger.LogDebug("No subscribers for event {EventType}", eventType);
            return;
        }

        var handlers = _subscriptions[eventType];
        var exceptions = new List<Exception>();

        foreach (var handler in handlers)
        {
            try
            {
                if (handler is Action<TEvent> action)
                {
                    action(@event);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in event handler for {EventType}", eventType);
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException($"Errors occurred while publishing event {eventType}", exceptions);
        }
    }

    /// <summary>
    /// Publishes an event asynchronously to all subscribers
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
    {
        var eventType = typeof(TEvent).Name;
        _logger.LogInformation("Publishing async event {EventType}", eventType);

        LogEvent(@event);

        if (!_subscriptions.ContainsKey(eventType))
        {
            _logger.LogDebug("No subscribers for event {EventType}", eventType);
            return;
        }

        var handlers = _subscriptions[eventType];
        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            try
            {
                if (handler is Func<TEvent, Task> asyncHandler)
                {
                    tasks.Add(asyncHandler(@event));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to async event {EventType}", eventType);
            }
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while publishing async event {EventType}", eventType);
        }
    }

    /// <summary>
    /// Unsubscribes all handlers for an event type
    /// </summary>
    public void Unsubscribe<TEvent>() where TEvent : class
    {
        var eventType = typeof(TEvent).Name;
        _subscriptions.Remove(eventType);
        _logger.LogDebug("Unsubscribed from event {EventType}", eventType);
    }

    /// <summary>
    /// Gets the number of subscribers for an event type
    /// </summary>
    public int GetSubscriberCount<TEvent>() where TEvent : class
    {
        var eventType = typeof(TEvent).Name;
        return _subscriptions.ContainsKey(eventType)
            ? _subscriptions[eventType].Count
            : 0;
    }

    /// <summary>
    /// Gets event log entries
    /// </summary>
    public List<EventLog> GetEventLog(int? limit = null)
    {
        var logs = _eventLog.OrderByDescending(l => l.Timestamp).ToList();
        return limit.HasValue ? logs.Take(limit.Value).ToList() : logs;
    }

    /// <summary>
    /// Clears the event log
    /// </summary>
    public void ClearEventLog()
    {
        _eventLog.Clear();
        _logger.LogDebug("Event log cleared");
    }

    /// <summary>
    /// Gets event statistics
    /// </summary>
    public EventBusStatistics GetStatistics()
    {
        var stats = new EventBusStatistics
        {
            TotalSubscriptions = _subscriptions.Values.Sum(list => list.Count),
            SubscribedEventTypes = _subscriptions.Keys.ToList(),
            TotalEventsLogged = _eventLog.Count,
            MostRecentEvent = _eventLog.OrderByDescending(e => e.Timestamp).FirstOrDefault()
        };

        return stats;
    }

    /// <summary>
    /// Logs an event in the event log
    /// </summary>
    private void LogEvent<TEvent>(TEvent @event) where TEvent : class
    {
        var log = new EventLog
        {
            EventType = typeof(TEvent).Name,
            Timestamp = DateTime.UtcNow,
            Data = @event
        };

        _eventLog.Add(log);

        // Keep log size under control
        if (_eventLog.Count > _maxLogSize)
        {
            _eventLog.RemoveRange(0, _eventLog.Count - _maxLogSize);
        }
    }

    public void Dispose()
    {
        _subscriptions.Clear();
        _eventLog.Clear();
    }
}

/// <summary>
/// Base class for events
/// </summary>
public abstract class Event
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Event log entry
/// </summary>
sealed public class EventLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public object? Data { get; set; }
}

/// <summary>
/// Event bus statistics
/// </summary>
sealed public class EventBusStatistics
{
    public int TotalSubscriptions { get; set; }
    public List<string> SubscribedEventTypes { get; set; } = new();
    public int TotalEventsLogged { get; set; }
    public EventLog? MostRecentEvent { get; set; }
}

/// <summary>
/// Query execution event
/// </summary>
sealed public class QueryExecutedEvent : Event
{
    public string QueryId { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public int ErrorCount { get; set; }
    public bool Success { get; set; }
}

/// <summary>
/// Schema modification event
/// </summary>
sealed public class SchemaModifiedEvent : Event
{
    public string SchemaName { get; set; } = string.Empty;
    public string ModificationType { get; set; } = string.Empty;
    public string? Details { get; set; }
}

/// <summary>
/// Cache event
/// </summary>
sealed public class CacheEvent : Event
{
    public string Key { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public bool Hit { get; set; }
}
