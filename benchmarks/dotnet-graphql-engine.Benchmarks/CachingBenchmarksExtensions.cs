using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace dotnet_graphql_engine.Benchmarks
{
    /// <summary>
    /// Extension methods that make it easier to work with <see cref="CachingBenchmarks"/> in
    /// ad‑hoc scenarios or from other benchmark suites.
    /// </summary>
    public static class CachingBenchmarksExtensions
    {
        /// <summary>
        /// Executes the <c>FirstExecution</c> benchmark and returns the elapsed time.
        /// </summary>
        /// <param name="bench">The benchmark instance to execute against. Cannot be null.</param>
        /// <returns>The elapsed time for the benchmark execution.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bench"/> is null.</exception>
        public static async Task<TimeSpan> MeasureFirstExecutionAsync(this CachingBenchmarks bench)
        {
            ArgumentNullException.ThrowIfNull(bench);

            bench.Setup();
            var sw = Stopwatch.StartNew();
            await bench.FirstExecution();
            sw.Stop();
            bench.Cleanup();
            return sw.Elapsed;
        }

        /// <summary>
        /// Executes the <c>CachedExecution</c> benchmark and returns the elapsed time.
        /// </summary>
        /// <param name="bench">The benchmark instance to execute against. Cannot be null.</param>
        /// <returns>The elapsed time for the benchmark execution.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bench"/> is null.</exception>
        public static async Task<TimeSpan> MeasureCachedExecutionAsync(this CachingBenchmarks bench)
        {
            ArgumentNullException.ThrowIfNull(bench);

            bench.Setup();
            var sw = Stopwatch.StartNew();
            await bench.CachedExecution();
            sw.Stop();
            bench.Cleanup();
            return sw.Elapsed;
        }

        /// <summary>
        /// Runs the full suite of caching benchmarks in a deterministic order.
        /// The method performs the required <c>Setup</c> and <c>Cleanup</c> calls.
        /// </summary>
        /// <param name="bench">The benchmark instance to execute against. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bench"/> is null.</exception>
        public static async Task RunAllAsync(this CachingBenchmarks bench)
        {
            ArgumentNullException.ThrowIfNull(bench);

            bench.Setup();

            await bench.FirstExecution();
            await bench.CachedExecution();
            await bench.CacheMissAfterExpiration();
            await bench.CacheHitRatio();

            bench.Cleanup();
        }

        /// <summary>
        /// Returns the configured number of iterations for the benchmark.
        /// </summary>
        /// <param name="bench">The benchmark instance to get iterations from. Cannot be null.</param>
        /// <returns>The configured number of iterations.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bench"/> is null.</exception>
        public static int GetIterations(this CachingBenchmarks bench)
        {
            ArgumentNullException.ThrowIfNull(bench);
            return bench.Iterations;
        }
    }
}
