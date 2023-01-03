# CachingBenchmarksExtensions

Provides extension methods for executing and measuring GraphQL query caching benchmarks, enabling comparison of cold-cache versus warm-cache execution performance across configurable iteration counts.

## API

### MeasureFirstExecutionAsync

```csharp
public static async Task<TimeSpan> MeasureFirstExecutionAsync(this IQueryExecutor executor, string query, int iterations = 10)
```

Executes the specified query against a cold cache for the given number of iterations and returns the total elapsed time. This measures baseline performance without any cached results.

**Parameters**
- `executor`: The query executor instance to benchmark.
- `query`: The GraphQL query document to execute.
- `iterations`: Number of executions to perform. Defaults to 10.

**Returns**
A `TimeSpan` representing the cumulative duration of all iterations.

**Exceptions**
- `ArgumentNullException`: Thrown if `executor` or `query` is null.
- `ArgumentOutOfRangeException`: Thrown if `iterations` is less than 1.
- `GraphQLException`: Propagated if query execution fails during any iteration.

---

### MeasureCachedExecutionAsync

```csharp
public static async Task<TimeSpan> MeasureCachedExecutionAsync(this IQueryExecutor executor, string query, int iterations = 10)
```

Executes the specified query against a warm cache for the given number of iterations and returns the total elapsed time. The first execution populates the cache; subsequent iterations benefit from cached results.

**Parameters**
- `executor`: The query executor instance to benchmark.
- `query`: The GraphQL query document to execute.
- `iterations`: Number of executions to perform. Defaults to 10.

**Returns**
A `TimeSpan` representing the cumulative duration of all iterations.

**Exceptions**
- `ArgumentNullException`: Thrown if `executor` or `query` is null.
- `ArgumentOutOfRangeException`: Thrown if `iterations` is less than 1.
- `GraphQLException`: Propagated if query execution fails during any iteration.

---

### RunAllAsync

```csharp
public static async Task RunAllAsync(this IQueryExecutor executor, IEnumerable<string> queries, int iterations = 10, Action<string, TimeSpan, TimeSpan>? onResult = null)
```

Runs both cold-cache and warm-cache benchmarks for each query in the provided collection, optionally invoking a callback with the results.

**Parameters**
- `executor`: The query executor instance to benchmark.
- `queries`: A collection of GraphQL query documents to benchmark.
- `iterations`: Number of executions per query per cache state. Defaults to 10.
- `onResult`: Optional callback invoked per query with the query string, cold-cache duration, and warm-cache duration.

**Returns**
A completed `Task` when all benchmarks finish.

**Exceptions**
- `ArgumentNullException`: Thrown if `executor` or `queries` is null.
- `ArgumentOutOfRangeException`: Thrown if `iterations` is less than 1.
- `GraphQLException`: Propagated if any query execution fails.

---

### GetIterations

```csharp
public static int GetIterations(this BenchmarkConfiguration configuration)
```

Retrieves the configured iteration count from a benchmark configuration object.

**Parameters**
- `configuration`: The benchmark configuration instance.

**Returns**
The integer iteration count configured for benchmark runs.

**Exceptions**
- `ArgumentNullException`: Thrown if `configuration` is null.

## Usage

### Basic cold vs warm cache comparison

```csharp
var executor = serviceProvider.GetRequiredService<IQueryExecutor>();
const string query = @"{ products { id name price } }";

var coldTime = await executor.MeasureFirstExecutionAsync(query, iterations: 20);
var warmTime = await executor.MeasureCachedExecutionAsync(query, iterations: 20);

Console.WriteLine($"Cold cache: {coldTime.TotalMilliseconds:F2} ms");
Console.WriteLine($"Warm cache: {warmTime.TotalMilliseconds:F2} ms");
Console.WriteLine($"Speedup: {coldTime.TotalMilliseconds / warmTime.TotalMilliseconds:F2}x");
```

### Batch benchmarking multiple queries with custom reporting

```csharp
var executor = serviceProvider.GetRequiredService<IQueryExecutor>();
var queries = new[]
{
    "{ products { id name } }",
    "{ users { id email } }",
    "{ orders { id total } }"
};

await executor.RunAllAsync(queries, iterations: 15, (query, cold, warm) =>
{
    var speedup = cold.TotalMilliseconds / warm.TotalMilliseconds;
    Console.WriteLine($"Query: {query[..Math.Min(40, query.Length)]}...");
    Console.WriteLine($"  Cold: {cold.TotalMilliseconds:F1} ms | Warm: {warm.TotalMilliseconds:F1} ms | Speedup: {speedup:F2}x");
});
```

## Notes

- **Thread safety**: The extension methods themselves are stateless and thread-safe. However, the underlying `IQueryExecutor` implementation determines whether concurrent benchmark executions are safe. Prefer sequential execution unless the executor documents concurrent safety.
- **Cache state**: `MeasureCachedExecutionAsync` assumes the executor's cache persists between calls. If the executor uses per-request or scoped caching, warm-cache measurements may not reflect true cached performance.
- **Iteration count**: Low iteration counts (below 5) produce noisy results due to JIT compilation, GC, and OS scheduling. Use at least 10 iterations for stable measurements; 50+ for publication-grade data.
- **Exception handling**: Exceptions during any iteration abort the entire benchmark run. Wrap calls in try/catch if partial results are acceptable.
- **Configuration coupling**: `GetIterations` reads from `BenchmarkConfiguration`; ensure the configuration instance passed matches the one used to construct the executor for consistent behavior.
