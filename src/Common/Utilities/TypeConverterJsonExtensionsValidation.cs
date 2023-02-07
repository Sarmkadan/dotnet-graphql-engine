#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Validation helpers for <see cref="TypeConverterJsonExtensions"/>.
/// </summary>
public static class TypeConverterJsonExtensionsValidation
{
    /// <summary>
    /// Validates that <see cref="TypeConverterJsonExtensions"/> contains the expected public static members
    /// with the correct signatures.
    /// </summary>
    /// <returns>
    /// A read‑only list of human‑readable problem descriptions.
    /// The list is empty when the type is valid.
    /// </returns>
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();
        var type = typeof(TypeConverterJsonExtensions);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

        // Expected: string ToJson(object? value, bool indented = false)
        var toJson = methods.FirstOrDefault(m =>
            m.Name == nameof(TypeConverterJsonExtensions.ToJson) &&
            !m.IsGenericMethod &&
            m.ReturnType == typeof(string) &&
            m.GetParameters().Length == 2);
        if (toJson is null)
            problems.Add("Missing or invalid ToJson(object?, bool) method.");

        // Expected: T? FromJson<T>(string json)
        var fromJsonGeneric = methods.FirstOrDefault(m =>
            m.Name == nameof(TypeConverterJsonExtensions.FromJson) &&
            m.IsGenericMethodDefinition &&
            m.GetParameters().Length == 1);
        if (fromJsonGeneric is null)
            problems.Add("Missing or invalid generic FromJson<T>(string) method.");

        // Expected: bool TryFromJson<T>(string json, out T? value)
        var tryFromJsonGeneric = methods.FirstOrDefault(m =>
            m.Name == nameof(TypeConverterJsonExtensions.TryFromJson) &&
            m.IsGenericMethodDefinition &&
            m.GetParameters().Length == 2);
        if (tryFromJsonGeneric is null)
            problems.Add("Missing or invalid generic TryFromJson<T>(string, out T?) method.");

        // Expected: object? FromJson(string json, Type targetType)
        var fromJson = methods.FirstOrDefault(m =>
            m.Name == nameof(TypeConverterJsonExtensions.FromJson) &&
            !m.IsGenericMethod &&
            m.ReturnType == typeof(object) &&
            m.GetParameters().Length == 2);
        if (fromJson is null)
            problems.Add("Missing or invalid non‑generic FromJson(string, Type) method.");

        // Expected: bool TryFromJson(string json, Type targetType, out object? value)
        var tryFromJson = methods.FirstOrDefault(m =>
            m.Name == nameof(TypeConverterJsonExtensions.TryFromJson) &&
            !m.IsGenericMethod &&
            m.ReturnType == typeof(bool) &&
            m.GetParameters().Length == 3);
        if (tryFromJson is null)
            problems.Add("Missing or invalid non‑generic TryFromJson(string, Type, out object?) method.");

        return problems;
    }

    /// <summary>
    /// Determines whether <see cref="TypeConverterJsonExtensions"/> passes validation.
    /// </summary>
    /// <returns><c>true</c> if no validation problems are found; otherwise <c>false</c>.</returns>
    public static bool IsValid() => !Validate().Any();

    /// <summary>
    /// Ensures that <see cref="TypeConverterJsonExtensions"/> is valid.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when validation problems are detected. The exception message contains a
    /// semicolon‑separated list of the problems.
    /// </exception>
    public static void EnsureValid()
    {
        var problems = Validate();
        if (problems.Any())
            throw new ArgumentException($"TypeConverterJsonExtensions validation failed: {string.Join("; ", problems)}");
    }
}
