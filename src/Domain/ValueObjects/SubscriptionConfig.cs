// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Domain.ValueObjects;

/// <summary>
/// Configuration for GraphQL subscriptions
/// </summary>
public class SubscriptionConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public bool Enabled { get; set; } = true;
    public int MaxConnections { get; set; } = 1000;
    public int ConnectionTimeoutMs { get; set; } = 30000;
    public int HeartbeatIntervalMs { get; set; } = 5000;
    public int MaxMessageQueueSize { get; set; } = 1000;
    public bool AllowMultipleSubscriptions { get; set; } = true;

    private readonly Dictionary<string, SubscriptionFilter> _filters = new();
    public IReadOnlyDictionary<string, SubscriptionFilter> Filters => _filters.AsReadOnly();

    /// <summary>
    /// Adds a subscription filter
    /// </summary>
    public void AddFilter(string name, SubscriptionFilter filter)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Filter name cannot be empty", nameof(name));

        if (filter == null) throw new ArgumentNullException(nameof(filter));

        _filters[name] = filter;
    }

    /// <summary>
    /// Removes a subscription filter
    /// </summary>
    public bool RemoveFilter(string name)
    {
        return _filters.Remove(name);
    }

    /// <summary>
    /// Gets a subscription filter
    /// </summary>
    public SubscriptionFilter? GetFilter(string name)
    {
        _filters.TryGetValue(name, out var filter);
        return filter;
    }

    /// <summary>
    /// Validates the subscription configuration
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (MaxConnections <= 0)
            errors.Add("Max connections must be greater than 0");

        if (ConnectionTimeoutMs < 1000)
            errors.Add("Connection timeout must be at least 1000ms");

        if (HeartbeatIntervalMs < 100)
            errors.Add("Heartbeat interval must be at least 100ms");

        if (MaxMessageQueueSize <= 0)
            errors.Add("Max message queue size must be greater than 0");

        return errors.Count == 0;
    }
}

/// <summary>
/// Represents a filter for subscriptions
/// </summary>
public class SubscriptionFilter
{
    public string Name { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
    public FilterType Type { get; set; } = FilterType.Include;
    public bool IsActive { get; set; } = true;

    public SubscriptionFilter()
    {
    }

    public SubscriptionFilter(string name, string expression, FilterType type = FilterType.Include)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        Type = type;
    }

    /// <summary>
    /// Evaluates the filter against a subscription update
    /// </summary>
    public bool Evaluate(Dictionary<string, object> updateData)
    {
        if (!IsActive) return true;
        if (string.IsNullOrEmpty(Expression)) return Type == FilterType.Include;

        // Simple key presence check for demonstration
        var keys = Expression.Split(',').Select(k => k.Trim()).ToList();
        var matches = keys.All(k => updateData.ContainsKey(k));

        return Type == FilterType.Include ? matches : !matches;
    }
}

/// <summary>
/// Enumeration of subscription filter types
/// </summary>
public enum FilterType
{
    Include = 0,
    Exclude = 1
}
