#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Represents the execution context for a GraphQL operation
/// </summary>
sealed public class ExecutionContext
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string QueryId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public ExecutionState State { get; set; } = ExecutionState.Pending;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public long DurationMs { get; set; } = 0;
    public int RequestedFieldCount { get; set; } = 0;
    public int ResolvedFieldCount { get; set; } = 0;

    private readonly Dictionary<string, object?> _contextData = new();
    public IReadOnlyDictionary<string, object?> ContextData => _contextData.AsReadOnly();

    private readonly List<string> _executedResolvers = new();
    public IReadOnlyList<string> ExecutedResolvers => _executedResolvers.AsReadOnly();

    private readonly List<ExecutionError> _errors = new();
    public IReadOnlyList<ExecutionError> Errors => _errors.AsReadOnly();

    private readonly Dictionary<string, object> _cache = new();
    public IReadOnlyDictionary<string, object> Cache => _cache.AsReadOnly();

    public ExecutionContext()
    {
    }

    public ExecutionContext(string queryId)
    {
        QueryId = queryId ?? throw new ArgumentNullException(nameof(queryId));
    }

    /// <summary>
    /// Sets a context value that resolvers can access
    /// </summary>
    public void SetContextValue(string key, object? value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        _contextData[key] = value;
    }

    /// <summary>
    /// Gets a context value
    /// </summary>
    public object? GetContextValue(string key)
    {
        _contextData.TryGetValue(key, out var value);
        return value;
    }

    /// <summary>
    /// Records a resolver execution
    /// </summary>
    public void RecordResolverExecution(string resolverName)
    {
        if (string.IsNullOrEmpty(resolverName)) return;

        _executedResolvers.Add(resolverName);
        ResolvedFieldCount++;
    }

    /// <summary>
    /// Adds an execution error
    /// </summary>
    public void AddError(ExecutionError error)
    {
        if (error is null) throw new ArgumentNullException(nameof(error));

        _errors.Add(error);
        State = ExecutionState.Failed;
    }

    /// <summary>
    /// Adds an execution error with message
    /// </summary>
    public void AddError(string message, string? field = null, int? line = null)
    {
        var error = new ExecutionError
        {
            Message = message,
            Field = field,
            LineNumber = line
        };

        AddError(error);
    }

    /// <summary>
    /// Caches a resolved value
    /// </summary>
    public void CacheValue(string key, object value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        _cache[key] = value;
    }

    /// <summary>
    /// Retrieves a cached value
    /// </summary>
    public bool TryGetCachedValue(string key, out object? value)
    {
        return _cache.TryGetValue(key, out value);
    }

    /// <summary>
    /// Marks the execution as completed. Errors may still be present (partial failures
    /// are normal in GraphQL – the response carries both data and errors).
    /// </summary>
    public void Complete()
    {
        CompletedAt = DateTime.UtcNow;
        DurationMs = (long)(CompletedAt.Value - StartedAt).TotalMilliseconds;
        State = ExecutionState.Completed;
    }

    /// <summary>
    /// Marks the execution as failed due to a catastrophic / unrecoverable error.
    /// </summary>
    public void Fail(string reason)
    {
        AddError(reason);
        CompletedAt = DateTime.UtcNow;
        DurationMs = (long)(CompletedAt.Value - StartedAt).TotalMilliseconds;
        State = ExecutionState.Failed;
    }

    /// <summary>
    /// Cancels the execution
    /// </summary>
    public void Cancel()
    {
        State = ExecutionState.Cancelled;
        CompletedAt = DateTime.UtcNow;
        DurationMs = (long)(CompletedAt.Value - StartedAt).TotalMilliseconds;
    }

    /// <summary>
    /// Gets execution summary
    /// </summary>
    public string GetSummary()
    {
        return $"Execution {Id}: {State}, Duration: {DurationMs}ms, " +
               $"Resolved: {ResolvedFieldCount}/{RequestedFieldCount} fields, " +
               $"Errors: {_errors.Count}";
    }
}

/// <summary>
/// Represents an error that occurred during execution
/// </summary>
sealed public class ExecutionError
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }
    public int? LineNumber { get; set; }
    public string? StackTrace { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    private readonly Dictionary<string, object> _extensions = new();
    public IReadOnlyDictionary<string, object> Extensions => _extensions.AsReadOnly();

    public void AddExtension(string key, object value)
    {
        if (string.IsNullOrEmpty(key)) return;

        _extensions[key] = value;
    }
}

/// <summary>
/// Enumeration of execution states
/// </summary>
public enum ExecutionState
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
