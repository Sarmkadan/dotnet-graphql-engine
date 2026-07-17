#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides validation helpers for JSON strings.
/// </summary>
public static class JsonHelperValidation
{
    /// <summary>
    /// Validates a JSON string and returns any identified problems.
    /// </summary>
    /// <param name="json">The JSON string to validate.</param>
    /// <returns>A read-only list of validation problems; an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(string? json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            problems.Add("JSON string cannot be null, empty, or whitespace.");
            return problems;
        }

        if (!JsonHelper.IsValidJson(json))
        {
            problems.Add("The provided string is not a valid JSON.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified JSON string is valid.
    /// </summary>
    /// <param name="json">The JSON string to check.</param>
    /// <returns>true if the JSON is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool IsValid(string? json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return Validate(json).Count == 0;
    }

    /// <summary>
    /// Ensures the specified JSON string is valid.
    /// </summary>
    /// <param name="json">The JSON string to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the JSON string is invalid.</exception>
    public static void EnsureValid(string? json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var problems = Validate(json);
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid JSON: {string.Join(", ", problems)}", nameof(json));
        }
    }
}