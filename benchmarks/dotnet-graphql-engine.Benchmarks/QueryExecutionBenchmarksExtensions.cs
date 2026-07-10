using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.GraphQL;

namespace GraphQLEngine.Benchmarks;

/// <summary>
/// Extension methods for QueryExecutionBenchmarks providing additional benchmarking utilities
/// </summary>
public static class QueryExecutionBenchmarksExtensions
{
    /// <summary>
    /// Executes a query with warmup iterations to ensure JIT compilation and caching effects are measured
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance</param>
    /// <param name="query">The query to execute</param>
    /// <param name="warmupIterations">Number of warmup iterations (default: 5)</param>
    /// <param name="measureIterations">Number of measurement iterations (default: 10)</param>
    /// <returns>Average execution time in milliseconds</returns>
    public static async Task<double> ExecuteWithWarmupAsync(this QueryExecutionBenchmarks benchmarks, GraphQLQuery query, int warmupIterations = 5, int measureIterations = 10)
    {
        if (benchmarks == null)
            throw new ArgumentNullException(nameof(benchmarks));
        if (query == null)
            throw new ArgumentNullException(nameof(query));
        if (warmupIterations < 1)
            throw new ArgumentOutOfRangeException(nameof(warmupIterations), "Must be at least 1");
        if (measureIterations < 1)
            throw new ArgumentOutOfRangeException(nameof(measureIterations), "Must be at least 1");

        var executionService = benchmarks.GetExecutionService();
        var executionTimes = new List<double>();

        // Warmup phase - ensure JIT compilation and caching effects
        for (int i = 0; i < warmupIterations; i++)
        {
            var warmupResult = await executionService.ExecuteAsync(query);
            if (warmupResult.HasErrors)
            {
                throw new InvalidOperationException($"Warmup query execution failed: {string.Join(", ", warmupResult.Errors)}");
            }
        }

        // Measurement phase
        var totalTicks = 0L;
        for (int i = 0; i < measureIterations; i++)
        {
            var startTime = DateTime.UtcNow;
            var result = await executionService.ExecuteAsync(query);
            var endTime = DateTime.UtcNow;

            if (result.HasErrors)
            {
                throw new InvalidOperationException($"Measurement query execution failed: {string.Join(", ", result.Errors)}");
            }

            totalTicks += (endTime - startTime).Ticks;
        }

        var averageTicks = totalTicks / (double)measureIterations;
        return TimeSpan.FromTicks((long)averageTicks).TotalMilliseconds;
    }

    /// <summary>
    /// Executes multiple queries in parallel to test concurrent execution performance
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance</param>
    /// <param name="queries">Collection of queries to execute in parallel</param>
    /// <param name="degreeOfParallelism">Number of concurrent executions (default: Environment.ProcessorCount)</param>
    /// <returns>Collection of execution results</returns>
    public static async Task<IReadOnlyList<GraphQLExecutionResult>> ExecuteInParallelAsync(this QueryExecutionBenchmarks benchmarks, IReadOnlyCollection<GraphQLQuery> queries, int degreeOfParallelism = 0)
    {
        if (benchmarks == null)
            throw new ArgumentNullException(nameof(benchmarks));
        if (queries == null)
            throw new ArgumentNullException(nameof(queries));
        if (queries.Count == 0)
            throw new ArgumentException("Queries collection cannot be empty", nameof(queries));

        if (degreeOfParallelism <= 0)
        {
            degreeOfParallelism = Environment.ProcessorCount;
        }

        var executionService = benchmarks.GetExecutionService();
        var results = new List<GraphQLExecutionResult>(queries.Count);
        var semaphore = new SemaphoreSlim(degreeOfParallelism, degreeOfParallelism);

        var tasks = new List<Task>();
        foreach (var query in queries)
        {
            await semaphore.WaitAsync();
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var result = await executionService.ExecuteAsync(query);
                    lock (results)
                    {
                        results.Add(result);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
        return results.AsReadOnly();
    }

    /// <summary>
    /// Creates a custom schema specifically for benchmarking with specific complexity
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance</param>
    /// <param name="typeCount">Number of types to generate (default: 10)</param>
    /// <param name="fieldCountPerType">Number of fields per type (default: 5)</param>
    /// <returns>The created schema name</returns>
    public static string CreateBenchmarkSchema(this QueryExecutionBenchmarks benchmarks, int typeCount = 10, int fieldCountPerType = 5)
    {
        if (benchmarks == null)
            throw new ArgumentNullException(nameof(benchmarks));
        if (typeCount < 1)
            throw new ArgumentOutOfRangeException(nameof(typeCount), "Must be at least 1");
        if (fieldCountPerType < 1)
            throw new ArgumentOutOfRangeException(nameof(fieldCountPerType), "Must be at least 1");

        var schemaName = $"BenchmarkSchema_{Guid.NewGuid()}";
        var schemaService = benchmarks.GetSchemaService();

        // Create base types
        for (int i = 0; i < typeCount; i++)
        {
            var typeName = $"Type{i + 1}";
            var type = new GraphQLType
            {
                Name = typeName,
                Description = $"Benchmark type {i + 1}",
                Kind = GraphQLTypeKind.Object
            };

            // Add fields
            for (int j = 0; j < fieldCountPerType; j++)
            {
                type.AddField(new GraphQLField
                {
                    Name = $"field{j + 1}",
                    ReturnType = "String",
                    Description = $"Field {j + 1} of type {typeName}"
                });
            }

            schemaService.AddType(schemaName, type);
        }

        // Create Query type with all types
        var queryType = new GraphQLType
        {
            Name = "Query",
            Description = "Benchmark query type",
            Kind = GraphQLTypeKind.Object
        };

        for (int i = 0; i < typeCount; i++)
        {
            queryType.AddField(new GraphQLField
            {
                Name = $"type{i + 1}",
                ReturnType = $"Type{i + 1}",
                Description = $"Get Type{i + 1}"
            });
        }

        schemaService.AddType(schemaName, queryType);

        // Register simple resolvers
        var executionService = benchmarks.GetExecutionService();
        for (int i = 0; i < typeCount; i++)
        {
            var typeName = $"Type{i + 1}";
            executionService.RegisterResolver($"{typeName.ToLower()}", async (ExecutionContext ctx) => new { id = "1", name = typeName });
        }

        return schemaName;
    }

    /// <summary>
    /// Measures memory allocation during query execution
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance</param>
    /// <param name="query">The query to execute</param>
    /// <param name="iterations">Number of iterations to measure (default: 100)</param>
    /// <returns>Average memory allocated per iteration in bytes</returns>
    public static async Task<long> MeasureMemoryAllocationAsync(this QueryExecutionBenchmarks benchmarks, GraphQLQuery query, int iterations = 100)
    {
        if (benchmarks == null)
            throw new ArgumentNullException(nameof(benchmarks));
        if (query == null)
            throw new ArgumentNullException(nameof(query));
        if (iterations < 1)
            throw new ArgumentOutOfRangeException(nameof(iterations), "Must be at least 1");

        var executionService = benchmarks.GetExecutionService();
        var gcBeforeCollections = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
        var memoryBefore = GC.GetTotalAllocatedBytes(true);

        for (int i = 0; i < iterations; i++)
        {
            var result = await executionService.ExecuteAsync(query);
            if (result.HasErrors)
            {
                throw new InvalidOperationException($"Memory measurement query failed: {string.Join(", ", result.Errors)}");
            }
        }

        var memoryAfter = GC.GetTotalAllocatedBytes(false);
        var gcAfterCollections = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);

        var totalAllocated = memoryAfter - memoryBefore;
        var averageAllocated = totalAllocated / iterations;

        // Account for GC collections during measurement
        var gcCollections = gcAfterCollections - gcBeforeCollections;
        if (gcCollections > 0)
        {
            // Adjust for GC overhead - each collection can allocate significant memory
            averageAllocated = (long)(averageAllocated * 1.1); // 10% overhead estimate
        }

        return averageAllocated;
    }

    private static SchemaService GetSchemaService(this QueryExecutionBenchmarks benchmarks)
    {
        var field = typeof(QueryExecutionBenchmarks).GetField("_schemaService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (SchemaService)field?.GetValue(benchmarks)!;
    }

    private static GraphQLExecutionService GetExecutionService(this QueryExecutionBenchmarks benchmarks)
    {
        var field = typeof(QueryExecutionBenchmarks).GetField("_executionService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (GraphQLExecutionService)field?.GetValue(benchmarks)!;
    }
}