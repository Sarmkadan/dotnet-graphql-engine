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
    /// Validates extensions methods of <see cref="DependencyInjectionJsonExtensions"/> for invalid usage.
    /// </summary>
    /// <returns>A list of human-readable problems with the extensions methods.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        // Validate ToJson methods
        ValidateToJson(problems);

        // Validate FromJson methods
        ValidateFromJson(problems);

        // Validate TryFromJson methods
        ValidateTryFromJson(problems);

        return problems.AsReadOnly();
    }

    private static void ValidateToJson(List<string> problems)
    {
        // No validation rules currently needed for ToJson methods.
    }

    private static void ValidateFromJson(List<string> problems)
    {
        // FromJson methods return null for null or whitespace input; this is valid.
    }

    private static void ValidateTryFromJson(List<string> problems)
    {
        // TryFromJson methods return false for null or whitespace input; this is valid.
    }

    /// <summary>
    /// Checks if extensions methods of <see cref="DependencyInjectionJsonExtensions"/> are valid.
    /// </summary>
    /// <returns><see langword="true"/> if the extensions methods are valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Ensures extensions methods of <see cref="DependencyInjectionJsonExtensions"/> are valid.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when extensions methods are not valid.</exception>
    public static void EnsureValid()
    {
        var problems = Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid usage of DependencyInjectionJsonExtensions: {string.Join(", ", problems)}");
        }
    }
}
