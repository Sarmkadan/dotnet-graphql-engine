#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides validation extension methods for Type objects that are serialized/deserialized
/// by ReflectionHelperJsonExtensions
/// </summary>
public static class ReflectionHelperJsonExtensionsValidation
{
    /// <summary>
    /// Validates a Type object that was serialized by ReflectionHelperJsonExtensions
    /// </summary>
    /// <param name="type">The Type to validate</param>
    /// <returns>A list of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> Validate(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var problems = new List<string>();

        // Type name validation
        if (string.IsNullOrEmpty(type.FullName))
        {
            problems.Add("Type FullName cannot be null or empty");
        }

        // Assembly qualified name validation
        if (string.IsNullOrEmpty(type.AssemblyQualifiedName))
        {
            problems.Add("Type AssemblyQualifiedName cannot be null or empty");
        }
        else if (type.AssemblyQualifiedName.Length > 2048)
        {
            problems.Add("Type AssemblyQualifiedName exceeds maximum length of 2048 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a Type object is valid (has no validation problems)
    /// </summary>
    /// <param name="type">The Type to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    public static bool IsValid(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return Validate(type).Count == 0;
    }

    /// <summary>
    /// Ensures a Type object is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="type">The Type to validate</param>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems</exception>
    public static void EnsureValid(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var problems = Validate(type);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Type validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}