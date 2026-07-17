#nullable enable
using System;
using System.Collections.Generic;

namespace GraphQLEngine.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="GraphQLException"/> and its derived types.
/// </summary>
public static class GraphQLExceptionValidation
{
    /// <summary>
    /// Validates the exception instance and returns a read-only list of problems.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>
    /// A read-only list of validation error messages. The list is empty if the instance is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this GraphQLException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(value.Extensions);

        var problems = new List<string>();

        // Base members
        if (value.ErrorCode is not null && string.IsNullOrWhiteSpace(value.ErrorCode))
        {
            problems.Add("ErrorCode is empty or whitespace.");
        }

        // Extensions: ensure no null values (keys cannot be null by definition)
        foreach (var kvp in value.Extensions)
        {
            if (kvp.Value is null)
            {
                problems.Add($"Extensions entry '{kvp.Key}' has a null value.");
            }
        }

        // Derived-type specific validation
        switch (value)
        {
            case ExecutionException exec when string.IsNullOrWhiteSpace(exec.FieldPath):
                problems.Add("ExecutionException.FieldPath is null or whitespace.");
                break;

            case ExecutionException exec when exec.LineNumber is { } line && line < 1:
                problems.Add($"ExecutionException.LineNumber ({line}) must be greater than zero.");
                break;

            case QueryComplexityException qc:
                if (qc.ActualScore < 0)
                {
                    problems.Add($"QueryComplexityException.ActualScore ({qc.ActualScore}) cannot be negative.");
                }

                if (qc.MaxScore < 0)
                {
                    problems.Add($"QueryComplexityException.MaxScore ({qc.MaxScore}) cannot be negative.");
                }
                break;

            case ValidationException ve:
                if (ve.ValidationErrors is null)
                {
                    problems.Add("ValidationException.ValidationErrors is null.");
                }
                else
                {
                    for (int i = 0; i < ve.ValidationErrors.Count; i++)
                    {
                        var err = ve.ValidationErrors[i];
                        if (string.IsNullOrWhiteSpace(err))
                        {
                            problems.Add($"ValidationException.ValidationErrors[{i}] is null or whitespace.");
                        }
                    }
                }
                break;

            case DataLoaderException dl when string.IsNullOrWhiteSpace(dl.LoaderName):
                problems.Add("DataLoaderException.LoaderName is null or whitespace.");
                break;

            case SubscriptionException sub when sub.ClientId is not null && string.IsNullOrWhiteSpace(sub.ClientId):
                problems.Add("SubscriptionException.ClientId is whitespace.");
                break;
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Indicates whether the exception instance passes validation.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns><c>true</c> if no validation problems are found; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this GraphQLException value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures the exception instance is valid, otherwise throws <see cref="ArgumentException"/>.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when validation problems are found.</exception>
    public static void EnsureValid(this GraphQLException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"GraphQLException validation failed: {string.Join("; ", problems)}",
                nameof(value));
        }
    }
}