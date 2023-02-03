#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GraphQLEngine.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="DependencyInjectionJsonExtensions"/>.
/// </summary>
public static class DependencyInjectionJsonExtensionsValidation
{
    /// <summary>
    /// Validates a <see cref="DependencyInjectionJsonExtensions"/> instance.
    /// </summary>
    /// <param name="value">The <see cref="DependencyInjectionJsonExtensions"/> instance to validate.</param>
    /// <returns>A list of human-readable problems with the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this DependencyInjectionJsonExtensions? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // No validation logic needed here as DependencyInjectionJsonExtensions is a static class
        // and does not have any instance state. However, we can validate its methods.

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="DependencyInjectionJsonExtensions"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="DependencyInjectionJsonExtensions"/> instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this DependencyInjectionJsonExtensions? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a <see cref="DependencyInjectionJsonExtensions"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="DependencyInjectionJsonExtensions"/> instance to ensure.</param>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this DependencyInjectionJsonExtensions? value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid DependencyInjectionJsonExtensions instance: {string.Join(", ", problems)}", nameof(value));
        }
    }
}
