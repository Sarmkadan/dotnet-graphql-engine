using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQLEngine.Benchmarks;

/// <summary>
/// Validation helpers for CachingBenchmarks to ensure benchmark configuration is valid
/// </summary>
public static class CachingBenchmarksValidation
{
    /// <summary>
    /// Validates a CachingBenchmarks instance and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The CachingBenchmarks instance to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> Validate(this CachingBenchmarks value)
    {
        var problems = new List<string>();

        if (value == null)
        {
            problems.Add("CachingBenchmarks instance is null");
            return problems.AsReadOnly();
        }

        // Validate that required fields are initialized after Setup() is called
        // These are checked via null checks in the benchmark methods themselves
        if (value._serviceProvider == null)
        {
            problems.Add("_serviceProvider is not initialized. Ensure Setup() has been called.");
        }

        if (value._executionService == null)
        {
            problems.Add("_executionService is not initialized. Ensure Setup() has been called.");
        }

        if (value._schemaService == null)
        {
            problems.Add("_schemaService is not initialized. Ensure Setup() has been called.");
        }

        if (value._cachedQuery == null)
        {
            problems.Add("_cachedQuery is not initialized. Ensure Setup() has been called.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a CachingBenchmarks instance is valid
    /// </summary>
    /// <param name="value">The CachingBenchmarks instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this CachingBenchmarks value)
    {
        return !value.Validate().Any();
    }

    /// <summary>
    /// Ensures a CachingBenchmarks instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The CachingBenchmarks instance to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems</exception>
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
