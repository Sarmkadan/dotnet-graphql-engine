// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Represents a batch data loading request to prevent N+1 queries
/// </summary>
public class DataLoaderRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string LoaderName { get; set; } = string.Empty;
    public string ExecutionContextId { get; set; } = string.Empty;
    public DataLoaderState State { get; set; } = DataLoaderState.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedAt { get; set; }
    public int BatchSize { get; set; } = 0;
    public long ExecutionTimeMs { get; set; } = 0;

    private readonly List<object> _keys = new();
    public IReadOnlyList<object> Keys => _keys.AsReadOnly();

    private readonly Dictionary<object, object?> _results = new();
    public IReadOnlyDictionary<object, object?> Results => _results.AsReadOnly();

    private readonly List<string> _errors = new();
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    public DataLoaderRequest()
    {
    }

    public DataLoaderRequest(string loaderName, string executionContextId)
    {
        LoaderName = loaderName ?? throw new ArgumentNullException(nameof(loaderName));
        ExecutionContextId = executionContextId ?? throw new ArgumentNullException(nameof(executionContextId));
    }

    /// <summary>
    /// Adds a key to load in this batch
    /// </summary>
    public void AddKey(object key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        if (!_keys.Contains(key))
        {
            _keys.Add(key);
            BatchSize++;
        }
    }

    /// <summary>
    /// Removes a key from the batch
    /// </summary>
    public bool RemoveKey(object key)
    {
        if (key == null) return false;

        if (_keys.Remove(key))
        {
            BatchSize--;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets the result for a key
    /// </summary>
    public void SetResult(object key, object? result)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        _results[key] = result;
    }

    /// <summary>
    /// Gets the result for a key
    /// </summary>
    public bool TryGetResult(object key, out object? result)
    {
        return _results.TryGetValue(key, out result);
    }

    /// <summary>
    /// Gets results in the order of keys
    /// </summary>
    public List<object?> GetOrderedResults()
    {
        return _keys.Select(k => _results.TryGetValue(k, out var v) ? v : null).ToList();
    }

    /// <summary>
    /// Adds an error that occurred during loading
    /// </summary>
    public void AddError(string error)
    {
        if (string.IsNullOrEmpty(error)) return;

        _errors.Add(error);
        State = DataLoaderState.Failed;
    }

    /// <summary>
    /// Marks the request as executed
    /// </summary>
    public void MarkExecuted()
    {
        ExecutedAt = DateTime.UtcNow;
        ExecutionTimeMs = (long)(ExecutedAt.Value - CreatedAt).TotalMilliseconds;
        State = _errors.Count == 0 ? DataLoaderState.Completed : DataLoaderState.Failed;
    }

    /// <summary>
    /// Clears all results
    /// </summary>
    public void ClearResults()
    {
        _results.Clear();
        _errors.Clear();
        State = DataLoaderState.Pending;
    }

    /// <summary>
    /// Checks if all keys have been resolved
    /// </summary>
    public bool AllKeysResolved()
    {
        return _keys.Count > 0 && _keys.All(k => _results.ContainsKey(k));
    }

    /// <summary>
    /// Gets the missing keys that haven't been resolved yet
    /// </summary>
    public IEnumerable<object> GetMissingKeys()
    {
        return _keys.Where(k => !_results.ContainsKey(k));
    }

    /// <summary>
    /// Gets a summary of the data loader request
    /// </summary>
    public string GetSummary()
    {
        return $"DataLoader '{LoaderName}' (ID: {Id}): " +
               $"Batch size: {BatchSize}, State: {State}, " +
               $"Resolved: {_results.Count}, Errors: {_errors.Count}";
    }
}

/// <summary>
/// Enumeration of data loader states
/// </summary>
public enum DataLoaderState
{
    Pending = 0,
    Executing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
