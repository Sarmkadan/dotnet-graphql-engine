# CachingBenchmarks

The `CachingBenchmarks` type is a performance testing harness designed to measure and compare the execution times of GraphQL operations with and without caching enabled. It evaluates various caching scenarios including initial execution, cached execution, cache expiration, and hit ratio metrics to validate the efficiency and correctness of the caching layer in `dotnet-graphql-engine`.

## API

### `public void Setup()`
Initializes the benchmark environment before each test execution. This method prepares the necessary resources, such as configuring the GraphQL engine, seeding test data, or resetting cache states. It does not accept parameters and does not return a value. Throws if initialization fails (e.g., due to misconfigured dependencies or unavailable services).

### `public void Cleanup()`
Releases resources and resets the benchmark environment after each test execution. This method ensures no state leaks between benchmark iterations. It does not accept parameters and does not return a value. Throws if resource cleanup fails (e.g., due to locked files or active connections).

### `public async Task FirstExecution()`
Measures the execution time of a GraphQL operation **without** leveraging any cached results. This benchmark establishes a baseline for uncached performance. Returns a `Task` representing the asynchronous operation. Throws if the GraphQL operation fails (e.g., due to query errors or service unavailability).

### `public async Task CachedExecution()`
Measures the execution time of a GraphQL operation **with** caching enabled, where the result is expected to be served from the cache. This benchmark validates the performance improvement of cached responses. Returns a `Task` representing the asynchronous operation. Throws if the GraphQL operation fails or if the cache is unexpectedly bypassed.

### `public async Task CacheMissAfterExpiration()`
Measures the execution time of a GraphQL operation after the cache entry has expired. This benchmark verifies that expired cache entries are correctly invalidated and recomputed. Returns a `Task` representing the asynchronous operation. Throws if the operation fails or if the cache behaves unexpectedly (e.g., serving stale data).

### `public async Task CacheHitRatio()`
Evaluates the effectiveness of the caching layer by measuring the ratio of cache hits to total executions over multiple iterations. This benchmark provides insights into cache utilization and efficiency. Returns a `Task` representing the asynchronous operation. Throws if the benchmark cannot complete (e.g., due to service interruptions or misconfigured cache policies).

### `public int Iterations { get; }`
Gets the number of iterations configured for benchmark execution. This property is read-only and does not accept parameters. It returns the count of iterations as an `int`. Throws if accessed before the benchmark is initialized.

### `public async Task Cached_Iterations()`
Executes a GraphQL operation **with** caching enabled across multiple iterations, aggregating performance metrics for cached responses. Returns a `Task` representing the asynchronous operation. Throws if the operation fails during any iteration.

### `public async Task NonCached_Iterations()`
Executes a GraphQL operation **without** caching enabled across multiple iterations, aggregating performance metrics for uncached responses. Returns a `Task` representing the asynchronous operation. Throws if the operation fails during any iteration.

## Usage

### Example 1: Benchmarking Cached vs. Uncached Execution
