#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Provides validation helpers for <see cref="QueryField"/> instances.
/// </summary>
public static class QueryFieldValidation
{
    /// <summary>
    /// Validates a <see cref="QueryField"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The QueryField to validate.</param>
    /// <returns>A list of validation problems; empty if the field is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this QueryField? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("QueryField.Name cannot be null, empty, or whitespace.");
        }
        else if (value.Name.Any(char.IsWhiteSpace))
        {
            errors.Add("QueryField.Name cannot contain whitespace characters.");
        }

        // Validate Alias
        if (!string.IsNullOrEmpty(value.Alias) && string.IsNullOrWhiteSpace(value.Alias))
        {
            errors.Add("QueryField.Alias cannot be whitespace.");
        }
        else if (value.Alias?.Any(char.IsWhiteSpace) == true)
        {
            errors.Add("QueryField.Alias cannot contain whitespace characters.");
        }

        // Validate TypeCondition
        if (!string.IsNullOrEmpty(value.TypeCondition) && string.IsNullOrWhiteSpace(value.TypeCondition))
        {
            errors.Add("QueryField.TypeCondition cannot be whitespace.");
        }
        else if (value.TypeCondition?.Any(char.IsWhiteSpace) == true)
        {
            errors.Add("QueryField.TypeCondition cannot contain whitespace characters.");
        }

        // Validate Arguments
        if (value.Arguments == null)
        {
            errors.Add("QueryField.Arguments cannot be null.");
        }
        else
        {
            foreach (var arg in value.Arguments)
            {
                if (arg == null)
                {
                    errors.Add("QueryField.Arguments contains a null element.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(arg.Name))
                {
                    errors.Add("QueryField.Arguments[].Name cannot be null, empty, or whitespace.");
                }
                else if (arg.Name.Any(char.IsWhiteSpace))
                {
                    errors.Add("QueryField.Arguments[].Name cannot contain whitespace characters.");
                }
            }
        }

        // Validate Fields (nested selections)
        if (value.Fields == null)
        {
            errors.Add("QueryField.Fields cannot be null.");
        }
        else
        {
            foreach (var field in value.Fields)
            {
                if (field == null)
                {
                    errors.Add("QueryField.Fields contains a null element.");
                    continue;
                }

                // Recursively validate nested fields
                errors.AddRange(field.Validate().Select(e => $"Nested field validation: {e}"));
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="QueryField"/> instance is valid.
    /// </summary>
    /// <param name="value">The QueryField to check.</param>
    /// <returns>True if the field is valid; otherwise, false.</returns>
    public static bool IsValid(this QueryField? value)
        => value is not null && value.Validate().Count == 0;

    /// <summary>
    /// Ensures that a <see cref="QueryField"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The QueryField to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the field is invalid.</exception>
    public static void EnsureValid(this QueryField? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"QueryField is invalid. Validation errors:\n- {
                    string.Join("\n- ", errors)
                }");
        }
    }
}