#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Reflection;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides runtime validation for <see cref="ReflectionHelper"/> static class methods.
/// </summary>
public static class ReflectionHelperValidation
{
    /// <summary>
    /// Validates the behavior of <see cref="ReflectionHelper"/> methods against expected invariants.
    /// </summary>
    /// <returns>List of validation problems, empty if all validations pass.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any input parameter is null.</exception>
    public static IReadOnlyList<string> Validate()
    {
        ArgumentNullException.ThrowIfNull(typeof(ReflectionHelper));

        var errors = new List<string>();

        // Validate IsNullableType behavior
        if (ReflectionHelper.IsNullableType(typeof(int)))
        {
            errors.Add("IsNullableType should return false for non-nullable type int");
        }

        if (!ReflectionHelper.IsNullableType(typeof(int?)))
        {
            errors.Add("IsNullableType should return true for nullable type int?");
        }

        if (ReflectionHelper.IsNullableType(null))
        {
            errors.Add("IsNullableType should throw ArgumentNullException for null type");
        }

        // Validate GetNullableUnderlyingType behavior
        var intUnderlying = ReflectionHelper.GetNullableUnderlyingType(typeof(int?));
        if (intUnderlying != typeof(int))
        {
            errors.Add("GetNullableUnderlyingType should return typeof(int) for int?");
        }

        var nonNullableUnderlying = ReflectionHelper.GetNullableUnderlyingType(typeof(int));
        if (nonNullableUnderlying is not null)
        {
            errors.Add("GetNullableUnderlyingType should return null for non-nullable type int");
        }

        if (ReflectionHelper.GetNullableUnderlyingType(null) is not null)
        {
            errors.Add("GetNullableUnderlyingType should throw ArgumentNullException for null type");
        }

        // Validate IsGeneric behavior
        if (ReflectionHelper.IsGeneric(typeof(int)))
        {
            errors.Add("IsGeneric should return false for non-generic type int");
        }

        if (!ReflectionHelper.IsGeneric(typeof(List<int>)))
        {
            errors.Add("IsGeneric should return true for generic type List<int>");
        }

        if (ReflectionHelper.IsGeneric(null))
        {
            errors.Add("IsGeneric should throw ArgumentNullException for null type");
        }

        // Validate GetReadableTypeName behavior
        var listTypeName = ReflectionHelper.GetReadableTypeName(typeof(List<int>));
        if (listTypeName != "List<Int32>")
        {
            errors.Add($"GetReadableTypeName should return 'List<Int32>' for List<int>, got: {listTypeName}");
        }

        var simpleTypeName = ReflectionHelper.GetReadableTypeName(typeof(string));
        if (simpleTypeName != "String")
        {
            errors.Add($"GetReadableTypeName should return 'String' for string, got: {simpleTypeName}");
        }

        if (ReflectionHelper.GetReadableTypeName(null) is not null)
        {
            errors.Add("GetReadableTypeName should throw ArgumentNullException for null type");
        }

        // Validate ImplementsInterface behavior
        if (!ReflectionHelper.ImplementsInterface<System.Collections.IEnumerable>(typeof(List<int>)))
        {
            errors.Add("ImplementsInterface should return true for List<int> implementing IEnumerable");
        }

        if (ReflectionHelper.ImplementsInterface<System.Collections.IEnumerable>(typeof(string)))
        {
            errors.Add("ImplementsInterface should return false for string which doesn't implement IEnumerable<T>");
        }

        if (ReflectionHelper.ImplementsInterface<IDisposable>(null))
        {
            errors.Add("ImplementsInterface should throw ArgumentNullException for null type");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether all <see cref="ReflectionHelper"/> validations pass.
    /// </summary>
    /// <returns>True if all validations pass; otherwise, false.</returns>
    public static bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Validates <see cref="ReflectionHelper"/> and throws an exception if any validation fails.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when one or more validations fail.
    /// The exception message contains a numbered list of all validation failures.</exception>
    public static void EnsureValid()
    {
        var errors = Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ReflectionHelper validation failed:{Environment.NewLine}  {string.Join($"{Environment.NewLine}  ", errors)}");
        }
    }
}