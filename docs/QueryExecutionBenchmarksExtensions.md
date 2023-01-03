# QueryExecutionBenchmarksExtensions

Provides extension methods for benchmarking GraphQL query execution scenarios in the `dotnet-graphql-engine` project. This class offers utilities to warm up execution pipelines, run queries concurrently, generate benchmark schemas, and measure memory allocations during query processing.

## API

### ExecuteWithWarmupAsync

```csharp
public static async Task<double> ExecuteWithWarmupAsync(
    this IQueryExecutor executor,
    string query,
    int warmupIterations = 3,
    CancellationToken cancellationToken = default)
```

Performs a specified number of warmup executions before measuring and returning the average execution time of a GraphQL query. This stabilizes JIT compilation and caching effects to produce consistent benchmark timings.

**Parameters:**
- `executor` — the query executor instance to benchmark.
- `query` — the GraphQL query string to execute.
- `warmupIterations` — number of warmup runs to perform before timing (default 3).
- `cancellationToken` — token to cancel the operation.

**Returns:** the average execution duration in milliseconds across the measured runs.

**Throws:** `ArgumentNullException` when `executor` or `query` is null; propagates any exceptions thrown by the underlying executor.

---

### ExecuteInParallelAsync

```csharp
public static async Task<IReadOnlyList<GraphQLExecutionResult>> ExecuteInParallelAsync(
    this IQueryExecutor executor,
    string query,
    int degreeOfParallelism,
    CancellationToken cancellationToken = default)
```

Executes the same GraphQL query concurrently across multiple parallel tasks and collects all results. Useful for measuring throughput and identifying contention or thread-safety issues under load.

**Parameters:**
- `executor` — the query executor instance.
- `query` — the GraphQL query string to execute in each parallel task.
- `degreeOfParallelism` — number of concurrent executions to launch.
- `cancellationToken` — token to cancel all parallel operations.

**Returns:** a read-only list of `GraphQLExecutionResult` instances, one per parallel execution.

**Throws:** `ArgumentNullException` when `executor` or `query` is null; `ArgumentOutOfRangeException` when `degreeOfParallelism` is less than 1; `OperationCanceledException` when the token is signaled; aggregates exceptions from individual tasks into an `AggregateException` when multiple tasks fail.

---

### CreateBenchmarkSchema

```csharp
public static string CreateBenchmarkSchema(
    int typeCount = 10,
    int fieldCountPerType = 5,
    int depth = 3)
```

Generates a synthetic GraphQL schema string suitable for benchmarking. The schema contains configurable numbers of object types, fields per type, and nesting depth to simulate various query complexity levels.

**Parameters:**
- `typeCount` — number of object types to generate (default 10).
- `fieldCountPerType` — number of fields on each generated type (default 5).
- `depth` — maximum nesting depth for field type references (default 3).

**Returns:** a string containing the complete SDL schema definition.

**Throws:** `ArgumentOutOfRangeException` when any parameter is less than 1.

---

### MeasureMemoryAllocationAsync

```csharp
public static async Task<long> MeasureMemoryAllocationAsync(
    this IQueryExecutor executor,
    string query,
    CancellationToken cancellationToken = default)
```

Measures the total managed memory allocated during the execution of a single GraphQL query. Forces garbage collection before and after execution to isolate the allocation delta attributable to the query.

**Parameters:**
- `executor` — the query executor instance.
- `query` — the GraphQL query string to measure.
- `cancellationToken` — token to cancel the operation.

**Returns:** the number of bytes allocated during query execution.

**Throws:** `ArgumentNullException` when `executor` or `query` is null; propagates any exceptions thrown by the executor.

## Usage

### Example 1: Measuring Average Execution Time with Warmup

```csharp
using GraphQLBenchmarks;

IQueryExecutor executor = BuildExecutor(schema);
string query = QueryExecutionBenchmarksExtensions.CreateBenchmarkSchema(
    typeCount: 20,
    fieldCountPerType: 8,
    depth: 4);

double avgMs = await executor.ExecuteWithWarmupAsync(
    query,
    warmupIterations: 5);

Console.WriteLine($"Average execution time: {avgMs:F2} ms");
```

### Example 2: Parallel Execution and Memory Measurement

```csharp
using GraphQLBenchmarks;

IQueryExecutor executor = BuildExecutor(schema);
string query = @"
    query {
        users { id name posts { title } }
    }";

// Measure memory for a single execution
long allocatedBytes = await executor.MeasureMemoryAllocationAsync(query);
Console.WriteLine($"Allocated: {allocatedBytes} bytes");

// Run 8 concurrent executions and inspect results
IReadOnlyList<GraphQLExecutionResult> results =
    await executor.ExecuteInParallelAsync(query, degreeOfParallelism: 8);

int successCount = results.Count(r => r.Errors == null || r.Errors.Count == 0);
Console.WriteLine($"Successful executions: {successCount}/{results.Count}");
```

## Notes

- **Thread safety:** `ExecuteInParallelAsync` launches multiple tasks against the same executor instance. The executor must be thread-safe for concurrent use; otherwise, results may be corrupted or exceptions may surface as `AggregateException`.
- **Memory measurement accuracy:** `MeasureMemoryAllocationAsync` triggers `GC.Collect` and `GC.WaitForPendingFinalizers` aggressively. This can interfere with other concurrent operations in the process and should be used in isolated benchmark runs.
- **Warmup side effects:** `ExecuteWithWarmupAsync` discards results from warmup iterations. If the executor maintains mutable internal state across executions, warmup runs may alter the state seen by the timed run.
- **Schema generation:** `CreateBenchmarkSchema` produces deterministic output for given parameter values. The generated schema includes only object types and scalar fields; it does not include interfaces, unions, or input types.
- **Cancellation behavior:** All async methods respect the provided `CancellationToken`. When canceled during parallel execution, already-running tasks may complete or be left in an indeterminate state depending on the executor's own cancellation support.
