#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLEngine.Common.Utilities;

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Provides validation helpers for <see cref="QueryArgument"/> instances.
/// </summary>
public static class QueryArgumentValidation
{
    /// <summary>
    /// Validates a <see cref="QueryArgument"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The QueryArgument to validate.</param>
    /// <returns>A list of validation problems; empty if the argument is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QueryArgument? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("QueryArgument.Name cannot be null, empty, or whitespace.");
        }
        else if (value.Name.Any(char.IsWhiteSpace))
        {
            errors.Add("QueryArgument.Name cannot contain whitespace characters.");
        }
        else if (!value.Name.IsValidGraphQLName())
        {
            errors.Add("QueryArgument.Name must be a valid GraphQL name (start with letter/underscore, followed by letters/digits/underscores).");
        }

        // Validate Value (type compatibility is checked at schema level, but we can check for basic issues)
        if (value.Value != null)
        {
            // Check for common problematic types that might indicate schema mismatch
            var valueType = value.Value.GetType();
            if (valueType == typeof(object) || valueType == typeof(Type))
            {
                errors.Add("QueryArgument.Value has an invalid type that cannot be serialized to GraphQL.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="QueryArgument"/> instance is valid.
    /// </summary>
    /// <param name="value">The QueryArgument to check.</param>
    /// <returns>True if the argument is valid; otherwise, false.</returns>
    public static bool IsValid(this QueryArgument? value)
        => value is not null && value.Validate().Count == 0;

    /// <summary>
    /// Ensures that a <see cref="QueryArgument"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The QueryArgument to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the argument is invalid.</exception>
    public static void EnsureValid(this QueryArgument? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"QueryArgument is invalid. Validation errors:\n- {
                    string.Join("\n- ", errors)
                }");
        }
    }

    /// <summary>
    /// Validates a collection of QueryArguments and returns a list of all validation problems.
    /// </summary>
    /// <param name="arguments">The collection of QueryArguments to validate.</param>
    /// <returns>A list of validation problems; empty if all arguments are valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/> is null.</exception>
    public static IReadOnlyList<string> ValidateAll(this IEnumerable<QueryArgument?>? arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        var errors = new List<string>();
        var index = 0;

        foreach (var arg in arguments)
        {
            if (arg == null)
            {
                errors.Add($"QueryArgument collection contains a null element at index {index}.");
            }
            else
            {
                var argErrors = arg.Validate();
                if (argErrors.Count > 0)
                {
                    errors.AddRange(argErrors.Select(e => $"QueryArgument[{index}]: {e}"));
                }
            }
            index++;
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether all QueryArguments in a collection are valid.
    /// </summary>
    /// <param name="arguments">The collection of QueryArguments to check.</param>
    /// <returns>True if all arguments are valid; otherwise, false.</returns>
    public static bool AllValid(this IEnumerable<QueryArgument?>? arguments)
        => arguments is not null && !arguments.Any(a => a is null || !a.IsValid());

    /// <summary>
    /// Ensures that all QueryArguments in a collection are valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if any are invalid.
    /// </summary>
    /// <param name="arguments">The collection of QueryArguments to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when any argument is invalid.</exception>
    public static void EnsureAllValid(this IEnumerable<QueryArgument?>? arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        var errors = arguments.ValidateAll();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"QueryArgument collection contains invalid elements:\n- {
                    string.Join("\n- ", errors)
                }");
        }
    }
}