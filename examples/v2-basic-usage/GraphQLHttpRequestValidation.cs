#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQLEngine.Hosting;

/// <summary>
/// Validation helpers for <see cref="GraphQLHttpRequest"/> instances.
/// </summary>
public static class GraphQLHttpRequestValidation
{
    /// <summary>
    /// Validates a GraphQL HTTP request and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The request to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation messages.</returns>
    public static IReadOnlyList<string> Validate(this GraphQLHttpRequest value)
    {
        if (value is null)
        {
            return new[] { "Request cannot be null." };
        }

        var errors = new List<string>();

        // Validate Query property
        if (string.IsNullOrWhiteSpace(value.Query))
        {
            errors.Add("Query cannot be null or whitespace.");
        }
        else if (value.Query.Length > 1_000_000)
        {
            errors.Add("Query exceeds maximum length of 1,000,000 characters.");
        }

        // Validate OperationName property
        if (!string.IsNullOrEmpty(value.OperationName) && value.OperationName.Length > 100)
        {
            errors.Add("OperationName exceeds maximum length of 100 characters.");
        }

        // Validate Variables property
        if (value.Variables is not null)
        {
            if (value.Variables.Count > 100)
            {
                errors.Add("Variables dictionary exceeds maximum size of 100 entries.");
            }

            foreach (var kvp in value.Variables)
            {
                if (kvp.Key is null)
                {
                    errors.Add("Variables dictionary contains a null key.");
                    continue;
                }

                if (kvp.Key.Length > 100)
                {
                    errors.Add($"Variable key '{kvp.Key}' exceeds maximum length of 100 characters.");
                }

                if (kvp.Value is not null && kvp.Value.ToString()?.Length > 10_000)
                {
                    errors.Add($"Variable value for key '{kvp.Key}' exceeds maximum length of 10,000 characters.");
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a GraphQL HTTP request is valid.
    /// </summary>
    /// <param name="value">The request to check.</param>
    /// <returns>True if the request is valid; otherwise, false.</returns>
    public static bool IsValid(this GraphQLHttpRequest value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a GraphQL HTTP request is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The request to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the request is invalid.</exception>
    public static void EnsureValid(this GraphQLHttpRequest value)
    {
        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"GraphQL HTTP request is invalid. Problems: {string.Join(" ", errors)}");
        }
    }
}
