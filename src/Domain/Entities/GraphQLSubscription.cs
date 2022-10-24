// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Represents a GraphQL subscription operation
/// </summary>
public class GraphQLSubscription
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SubscriptionQuery { get; set; } = string.Empty;
    public string? ClientId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdatedAt { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
    public int TotalUpdatesReceived { get; set; } = 0;

    private readonly Dictionary<string, object?> _variables = new();
    public IReadOnlyDictionary<string, object?> Variables => _variables.AsReadOnly();

    private readonly List<string> _subscribedEvents = new();
    public IReadOnlyList<string> SubscribedEvents => _subscribedEvents.AsReadOnly();

    private readonly List<SubscriptionUpdate> _updates = new();
    public IReadOnlyList<SubscriptionUpdate> Updates => _updates.AsReadOnly();

    private readonly List<string> _errors = new();
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    public GraphQLSubscription()
    {
    }

    public GraphQLSubscription(string name, string subscriptionQuery, string? clientId = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        SubscriptionQuery = subscriptionQuery ?? throw new ArgumentNullException(nameof(subscriptionQuery));
        ClientId = clientId;
    }

    /// <summary>
    /// Sets a variable for subscription
    /// </summary>
    public void SetVariable(string name, object? value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be empty", nameof(name));

        _variables[name] = value;
    }

    /// <summary>
    /// Gets a variable value
    /// </summary>
    public object? GetVariable(string name)
    {
        _variables.TryGetValue(name, out var value);
        return value;
    }

    /// <summary>
    /// Adds an event that this subscription listens to
    /// </summary>
    public void AddSubscribedEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName)) return;

        if (!_subscribedEvents.Contains(eventName))
            _subscribedEvents.Add(eventName);
    }

    /// <summary>
    /// Removes a subscribed event
    /// </summary>
    public bool RemoveSubscribedEvent(string eventName)
    {
        return _subscribedEvents.Remove(eventName);
    }

    /// <summary>
    /// Records an update received from the subscription
    /// </summary>
    public void RecordUpdate(SubscriptionUpdate update)
    {
        if (update == null) throw new ArgumentNullException(nameof(update));

        _updates.Add(update);
        LastUpdatedAt = DateTime.UtcNow;
        TotalUpdatesReceived++;

        if (Status == SubscriptionStatus.Pending)
            Status = SubscriptionStatus.Active;
    }

    /// <summary>
    /// Gets updates received since a specific time
    /// </summary>
    public IEnumerable<SubscriptionUpdate> GetUpdatesAfter(DateTime timestamp)
    {
        return _updates.Where(u => u.PublishedAt > timestamp).ToList();
    }

    /// <summary>
    /// Adds an error
    /// </summary>
    public void AddError(string error)
    {
        if (string.IsNullOrEmpty(error)) return;

        _errors.Add(error);
        Status = SubscriptionStatus.Failed;
    }

    /// <summary>
    /// Completes the subscription
    /// </summary>
    public void Complete()
    {
        Status = SubscriptionStatus.Completed;
    }

    /// <summary>
    /// Cancels the subscription
    /// </summary>
    public void Cancel()
    {
        Status = SubscriptionStatus.Cancelled;
    }

    /// <summary>
    /// Pauses the subscription
    /// </summary>
    public void Pause()
    {
        Status = SubscriptionStatus.Paused;
    }

    /// <summary>
    /// Resumes a paused subscription
    /// </summary>
    public void Resume()
    {
        if (Status == SubscriptionStatus.Paused)
            Status = SubscriptionStatus.Active;
    }

    /// <summary>
    /// Validates the subscription
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Subscription name is required");

        if (string.IsNullOrWhiteSpace(SubscriptionQuery))
            errors.Add("Subscription query is required");

        if (_subscribedEvents.Count == 0)
            errors.Add("At least one subscribed event is required");

        return errors.Count == 0;
    }

    /// <summary>
    /// Clears all recorded updates
    /// </summary>
    public void ClearUpdates()
    {
        _updates.Clear();
        TotalUpdatesReceived = 0;
    }

    /// <summary>
    /// Gets a summary of the subscription
    /// </summary>
    public string GetSummary()
    {
        return $"Subscription '{Name}' (ID: {Id}): " +
               $"Status: {Status}, Client: {ClientId}, " +
               $"Events: {_subscribedEvents.Count}, Updates: {TotalUpdatesReceived}";
    }
}

/// <summary>
/// Represents an update received from a subscription
/// </summary>
public class SubscriptionUpdate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EventName { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    public SubscriptionUpdate()
    {
    }

    public SubscriptionUpdate(string eventName, Dictionary<string, object> data)
    {
        EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
        Data = data ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Enumeration of subscription statuses
/// </summary>
public enum SubscriptionStatus
{
    Pending = 0,
    Active = 1,
    Paused = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}
