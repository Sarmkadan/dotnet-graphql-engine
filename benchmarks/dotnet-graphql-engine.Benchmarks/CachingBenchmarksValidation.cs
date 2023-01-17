using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQLEngine.Benchmarks;

/// <summary>
/// Provides validation methods for <see cref="CachingBenchmarks"/> instances to ensure benchmark configuration is valid.
/// </summary>
public static class CachingBenchmarksValidation
{
    /// <summary>
    /// Validates a <see cref="CachingBenchmarks"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The <see cref="CachingBenchmarks"/> instance to validate.</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this CachingBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that required fields are initialized after Setup() is called
        // These are checked via null checks in the benchmark methods themselves
        if (value._serviceProvider is null)
        {
            problems.Add("_serviceProvider is not initialized. Ensure Setup() has been called.");
        }

        if (value._executionService is null)
        {
            problems.Add("_executionService is not initialized. Ensure Setup() has been called.");
        }

        if (value._schemaService is null)
        {
            problems.Add("_schemaService is not initialized. Ensure Setup() has been called.");
        }

        if (value._cachedQuery is null)
        {
            problems.Add("_cachedQuery is not initialized. Ensure Setup() has been called.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="CachingBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="CachingBenchmarks"/> instance to check.</param>
    /// <returns><see langword="true"/> if valid; <see langword="false"/> otherwise.</returns>
    public static bool IsValid(this CachingBenchmarks value) => !value.Validate().Any();

    /// <summary>
    /// Ensures a <see cref="CachingBenchmarks"/> instance is valid, throwing <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The <see cref="CachingBenchmarks"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems.</exception>
    public static void EnsureValid(this CachingBenchmarks value)
    {
        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"CachingBenchmarks validation failed:{Environment.NewLine}" +
                string.Join(Environment.NewLine, problems.Select(p => $"  - {p}")));
        }
    }
}
