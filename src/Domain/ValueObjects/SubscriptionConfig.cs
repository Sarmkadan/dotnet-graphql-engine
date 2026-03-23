#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Linq; // Added for LINQ extensions

namespace GraphQLEngine.Domain.ValueObjects;

/// <summary>
/// Configuration for GraphQL subscriptions
/// </summary>
sealed public class SubscriptionConfig
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

        if (filter is null) throw new ArgumentNullException(nameof(filter));

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
sealed public class SubscriptionFilter
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
    /// Supports simple key=value or nested.key=value expressions.
    /// </summary>
    public bool Evaluate(Dictionary<string, object> updateData)
    {
        if (!IsActive) return true;
        if (string.IsNullOrEmpty(Expression)) return Type == FilterType.Include;

        var parts = Expression.Split('=', 2); // Split only on the first '='
        if (parts.Length != 2)
        {
            // Invalid expression format, default to no filter (pass if Include, fail if Exclude)
            return Type == FilterType.Include;
        }

        var propertyPath = parts[0].Trim();
        var expectedValueString = parts[1].Trim();

        object? actualValue = GetValueByPath(updateData, propertyPath);

        bool matches = false;
        if (actualValue != null)
        {
            // Simple string comparison for now.
            // More complex scenarios (e.g., numbers, booleans, case-insensitivity) would require
            // type conversion and more sophisticated comparison logic.
            matches = actualValue.ToString()?.Equals(expectedValueString, StringComparison.OrdinalIgnoreCase) ?? false;
        } else if (expectedValueString.Equals("null", StringComparison.OrdinalIgnoreCase)) {
            matches = actualValue == null;
        }


        return Type == FilterType.Include ? matches : !matches;
    }

    /// <summary>
    /// Helper to get a value from a dictionary by a dot-separated path (e.g., "product.id")
    /// </summary>
    private object? GetValueByPath(Dictionary<string, object> data, string path)
    {
        var pathParts = path.Split('.');
        object? currentValue = data;

        foreach (var part in pathParts)
        {
            if (currentValue is Dictionary<string, object> dict)
            {
                if (dict.TryGetValue(part, out var value))
                {
                    currentValue = value;
                }
                else
                {
                    return null; // Path not found
                }
            }
            else
            {
                return null; // Not a dictionary, cannot traverse further
            }
        }
        return currentValue;
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
