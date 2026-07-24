using System;
using System.Collections.Generic;

namespace dotnet_graphql_engine.Common.Utilities;

/// <summary>
/// Provides validation helpers for reflection-related operations.
/// </summary>
public static class ReflectionHelperValidation
{
    /// <summary>
    /// Validates the supplied <see cref="Type"/> for basic reflection requirements.
    /// Currently the validation only checks that the type is not <c>null</c>.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of validation error messages.
    /// The list is empty when the type is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        // Placeholder for future validation logic.
        // At the moment there are no validation errors for a non‑null type.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the supplied <see cref="Type"/> passes validation.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>
    /// <c>true</c> if the type is non‑null and has no validation errors; otherwise <c>false</c>.
    /// </returns>
    public static bool IsValid(Type? type)
    {
        if (type is null)
        {
            return false;
        }

        return Validate(type).Count == 0;
    }

    /// <summary>
    /// Ensures that the supplied <see cref="Type"/> is valid.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when validation errors are present.</exception>
    public static void EnsureValid(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var errors = Validate(type);
        if (errors.Count > 0)
        {
            // Combine all error messages into a single exception message.
            throw new ArgumentException(string.Join("; ", errors), nameof(type));
        }
    }
}
