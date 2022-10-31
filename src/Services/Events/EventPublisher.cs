// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Services.Events;

/// <summary>
/// Event publisher that wraps the event bus and provides domain-specific event publishing
/// Coordinates with logging and telemetry systems
/// </summary>
public class EventPublisher : IDisposable
{
    private readonly EventBus _eventBus;
    private readonly ILogger<EventPublisher> _logger;
    private readonly List<PublishedEventRecord> _publishedEvents;

    public EventPublisher(EventBus eventBus, ILogger<EventPublisher> logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishedEvents = new List<PublishedEventRecord>();
    }

    /// <summary>
    /// Publishes a query execution event
    /// </summary>
    public void PublishQueryExecuted(string queryId, string operationName, long durationMs, bool success, int errorCount = 0)
    {
        var @event = new QueryExecutedEvent
        {
            QueryId = queryId,
            OperationName = operationName ?? "Anonymous",
            DurationMs = durationMs,
            Success = success,
            ErrorCount = errorCount
        };

        _logger.LogInformation("Publishing query executed event: {QueryId} ({OperationName}) - {Duration}ms",
            queryId, @event.OperationName, durationMs);

        _eventBus.Publish(@event);
        RecordPublishedEvent(@event.GetType().Name, @event);
    }

    /// <summary>
    /// Publishes a schema modification event
    /// </summary>
    public void PublishSchemaModified(string schemaName, string modificationType, string? details = null)
    {
        var @event = new SchemaModifiedEvent
        {
            SchemaName = schemaName,
            ModificationType = modificationType,
            Details = details
        };

        _logger.LogInformation("Publishing schema modified event: {SchemaName} ({ModificationType})",
            schemaName, modificationType);

        _eventBus.Publish(@event);
        RecordPublishedEvent(@event.GetType().Name, @event);
    }

    /// <summary>
    /// Publishes a cache event
    /// </summary>
    public void PublishCacheEvent(string key, string operation, bool hit)
    {
        var @event = new CacheEvent
        {
            Key = key,
            Operation = operation,
            Hit = hit
        };

        _eventBus.Publish(@event);
        RecordPublishedEvent(@event.GetType().Name, @event);
    }

    /// <summary>
    /// Publishes a custom event
    /// </summary>
    public void Publish<TEvent>(TEvent @event) where TEvent : Event
    {
        _logger.LogInformation("Publishing custom event: {EventType}", typeof(TEvent).Name);
        _eventBus.Publish(@event);
        RecordPublishedEvent(@event.GetType().Name, @event);
    }

    /// <summary>
    /// Publishes an event asynchronously
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : Event
    {
        _logger.LogInformation("Publishing async event: {EventType}", typeof(TEvent).Name);
        await _eventBus.PublishAsync(@event);
        RecordPublishedEvent(@event.GetType().Name, @event);
    }

    /// <summary>
    /// Records a published event for audit trail
    /// </summary>
    private void RecordPublishedEvent(string eventType, object @event)
    {
        var record = new PublishedEventRecord
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = eventType,
            PublishedAt = DateTime.UtcNow,
            EventData = @event
        };

        _publishedEvents.Add(record);

        // Keep only recent events
        if (_publishedEvents.Count > 1000)
        {
            _publishedEvents.RemoveRange(0, _publishedEvents.Count - 1000);
        }
    }

    /// <summary>
    /// Gets published event history
    /// </summary>
    public List<PublishedEventRecord> GetEventHistory(string? eventType = null, int limit = 100)
    {
        var query = _publishedEvents.AsEnumerable();

        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(e => e.EventType == eventType);

        return query.OrderByDescending(e => e.PublishedAt).Take(limit).ToList();
    }

    /// <summary>
    /// Gets event publishing statistics
    /// </summary>
    public EventPublishingStatistics GetStatistics()
    {
        var groupedByType = _publishedEvents.GroupBy(e => e.EventType);

        return new EventPublishingStatistics
        {
            TotalEventsPublished = _publishedEvents.Count,
            EventTypeBreakdown = groupedByType.ToDictionary(g => g.Key, g => g.Count()),
            LastEventPublishedAt = _publishedEvents.OrderByDescending(e => e.PublishedAt).FirstOrDefault()?.PublishedAt,
            SubscriberCounts = new Dictionary<string, int>
            {
                { "QueryExecuted", _eventBus.GetSubscriberCount<QueryExecutedEvent>() },
                { "SchemaModified", _eventBus.GetSubscriberCount<SchemaModifiedEvent>() },
                { "CacheEvent", _eventBus.GetSubscriberCount<CacheEvent>() }
            }
        };
    }

    /// <summary>
    /// Clears the event history
    /// </summary>
    public void ClearEventHistory()
    {
        _publishedEvents.Clear();
        _logger.LogInformation("Event history cleared");
    }

    public void Dispose()
    {
        _publishedEvents.Clear();
    }
}

/// <summary>
/// Published event record for audit trail
/// </summary>
public class PublishedEventRecord
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public object? EventData { get; set; }
}

/// <summary>
/// Event publishing statistics
/// </summary>
public class EventPublishingStatistics
{
    public int TotalEventsPublished { get; set; }
    public Dictionary<string, int> EventTypeBreakdown { get; set; } = new();
    public DateTime? LastEventPublishedAt { get; set; }
    public Dictionary<string, int> SubscriberCounts { get; set; } = new();
}
