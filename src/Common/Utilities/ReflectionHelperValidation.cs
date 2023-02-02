#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;
using System.Globalization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Validation utilities for <see cref="ReflectionHelper"/> static class.
/// </summary>
public static class ReflectionHelperValidation
{
    /// <summary>
    /// Validates various methods of <see cref="ReflectionHelper"/> and returns human-readable problems.
    /// </summary>
    /// <returns>List of validation problems, empty if valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        // Validate IsNullableType
        if (ReflectionHelper.IsNullableType(typeof(int)))
        {
            errors.Add("IsNullableType failed for int");
        }

        if (!ReflectionHelper.IsNullableType(typeof(int?)))
        {
            errors.Add("IsNullableType failed for int?");
        }

        // Validate GetNullableUnderlyingType
        if (ReflectionHelper.GetNullableUnderlyingType(typeof(int?)) != typeof(int))
        {
            errors.Add("GetNullableUnderlyingType failed for int?");
        }

        // Validate IsGeneric
        if (ReflectionHelper.IsGeneric(typeof(int)))
        {
            errors.Add("IsGeneric failed for int");
        }

        if (!ReflectionHelper.IsGeneric(typeof(List<int>)))
        {
            errors.Add("IsGeneric failed for List<int>");
        }

        // Validate GetReadableTypeName
        var typeName = ReflectionHelper.GetReadableTypeName(typeof(List<int>));
        if (typeName != "List<Int32>")
        {
            errors.Add($"GetReadableTypeName failed for List<int>, got: {typeName}");
        }

        // Validate ImplementsInterface
        if (!ReflectionHelper.ImplementsInterface<System.Collections.IEnumerable>(typeof(List<int>)))
        {
            errors.Add("ImplementsInterface failed for List<int> implementing IEnumerable");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the ReflectionHelper is valid (all validation methods pass).
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the ReflectionHelper is valid, throwing ArgumentException if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems.</exception>
    public static void EnsureValid()
    {
        var errors = Validate();

        if (errors.Count > 0)
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine("ReflectionHelper validation failed:");

            for (int i = 0; i < errors.Count; i++)
            {
                errorMessage.AppendLine($" {i + 1}. {errors[i]}");
            }

            throw new ArgumentException(errorMessage.ToString());
        }
    }
}
