#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Services.DataLoader;

/// <summary>
/// Service for managing batch data loading to prevent N+1 queries
/// </summary>
sealed public class DataLoaderService
{
    private readonly ILogger<DataLoaderService> _logger;
    private readonly Dictionary<string, DataLoaderRequest> _activeLoaders = new();
    private readonly Dictionary<string, Func<List<object>, Task<List<object?>>>> _batchFunctions = new();

    public DataLoaderService(ILogger<DataLoaderService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a batch function for a data loader
    /// </summary>
    public void RegisterBatchFunction(string loaderName,
        Func<List<object>, Task<List<object?>>> batchFunction)
    {
        if (string.IsNullOrEmpty(loaderName))
            throw new ArgumentException("Loader name cannot be empty", nameof(loaderName));

        if (batchFunction is null) throw new ArgumentNullException(nameof(batchFunction));

        _batchFunctions[loaderName] = batchFunction;
        _logger.LogInformation("Batch function registered for loader: {LoaderName}", loaderName);
    }

    /// <summary>
    /// Creates a new data loading request
    /// </summary>
    public DataLoaderRequest CreateRequest(string loaderName, string executionContextId)
    {
        if (string.IsNullOrEmpty(loaderName))
            throw new ArgumentException("Loader name cannot be empty", nameof(loaderName));

        if (string.IsNullOrEmpty(executionContextId))
            throw new ArgumentException("Execution context ID cannot be empty", nameof(executionContextId));

        var request = new DataLoaderRequest(loaderName, executionContextId);
        _activeLoaders[request.Id] = request;

        _logger.LogInformation("Data loader request created: {RequestId} for loader: {LoaderName}",
            request.Id, loaderName);

        return request;
    }

    /// <summary>
    /// Adds a key to be loaded
    /// </summary>
    public void LoadKey(string requestId, object key)
    {
        var request = GetRequest(requestId) ??
            throw new InvalidOperationException($"Request {requestId} not found");

        request.AddKey(key);
    }

    /// <summary>
    /// Executes the batch loading for a request
    /// </summary>
    public async Task<DataLoaderRequest> ExecuteAsync(string requestId)
    {
        var request = GetRequest(requestId) ??
            throw new InvalidOperationException($"Request {requestId} not found");

        try
        {
            if (request.BatchSize == 0)
            {
                request.MarkExecuted();
                return request;
            }

            request.State = DataLoaderState.Executing;

            // Get the batch function
            if (!_batchFunctions.TryGetValue(request.LoaderName, out var batchFunc))
            {
                request.AddError($"No batch function found for loader: {request.LoaderName}");
                request.MarkExecuted();
                return request;
            }

            // Execute the batch function
            var results = await batchFunc(request.Keys.ToList());

            // Map results to keys
            if (results is not null)
            {
                for (int i = 0; i < request.Keys.Count && i < results.Count; i++)
                {
                    request.SetResult(request.Keys[i], results[i]);
                }
            }

            request.MarkExecuted();

            _logger.LogInformation("Data loader executed: {RequestId}, Batch size: {BatchSize}, Duration: {Duration}ms",
                requestId, request.BatchSize, request.ExecutionTimeMs);

            return request;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing data loader: {RequestId}", requestId);
            request.AddError($"Execution failed: {ex.Message}");
            request.MarkExecuted();
            return request;
        }
    }

    /// <summary>
    /// Gets a request by ID
    /// </summary>
    public DataLoaderRequest? GetRequest(string requestId)
    {
        _activeLoaders.TryGetValue(requestId, out var request);
        return request;
    }

    /// <summary>
    /// Gets the result for a specific key from a completed request
    /// </summary>
    public object? GetResult(string requestId, object key)
    {
        var request = GetRequest(requestId);
        if (request is null) return null;

        request.TryGetResult(key, out var result);
        return result;
    }

    /// <summary>
    /// Removes a completed request
    /// </summary>
    public bool RemoveRequest(string requestId)
    {
        return _activeLoaders.Remove(requestId);
    }

    /// <summary>
    /// Gets statistics about active data loaders
    /// </summary>
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "ActiveRequests", _activeLoaders.Count },
            { "RegisteredLoaders", _batchFunctions.Count },
            { "TotalKeysLoaded", _activeLoaders.Values.Sum(r => r.BatchSize) },
            { "Timestamp", DateTime.UtcNow }
        };
    }

    /// <summary>
    /// Clears all completed requests
    /// </summary>
    public int ClearCompletedRequests()
    {
        var keysToRemove = _activeLoaders
            .Where(kv => kv.Value.State == DataLoaderState.Completed || kv.Value.State == DataLoaderState.Failed)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in keysToRemove)
            _activeLoaders.Remove(key);

        _logger.LogInformation("Cleared {Count} completed data loader requests", keysToRemove.Count);
        return keysToRemove.Count;
    }
}
