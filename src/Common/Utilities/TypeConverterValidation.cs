#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides validation helpers for TypeConverter operations
/// Validates input values before conversion and checks for common data issues
/// </summary>
public static class TypeConverterValidation
{
    /// <summary>
    /// Validates TypeConverter operations and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The value to validate (used for null checking)</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(object? value = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate GetDefaultValue - should not return null for value types
        try
        {
            var defaultValue = TypeConverter.GetDefaultValue(typeof(int));
            if (defaultValue is null)
            {
                problems.Add("GetDefaultValue returns null for value types, which may cause issues during conversion");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetDefaultValue throws exception for value types: {ex.Message}");
        }

        // Validate CanConvert - basic functionality
        if (!TypeConverter.CanConvert(typeof(string), typeof(int)))
            problems.Add("CanConvert incorrectly returns false for string to int conversion");

        if (TypeConverter.CanConvert(typeof(int), typeof(string)))
            problems.Add("CanConvert incorrectly returns true for int to string conversion (should be false)");

        // Validate ConvertList - should handle null input
        try
        {
            var emptyList = TypeConverter.ConvertList<int>(null);
            if (emptyList is null)
            {
                problems.Add("ConvertList returns null for null input instead of empty list");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ConvertList throws exception for null input: {ex.Message}");
        }

        // Validate ToJsonCompatible - should handle null and various types
        try
        {
            var nullResult = TypeConverter.ToJsonCompatible(null);
            if (nullResult is not null)
            {
                problems.Add("ToJsonCompatible should return null for null input");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ToJsonCompatible throws exception for null input: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if TypeConverter operations are valid
    /// </summary>
    /// <param name="value">The value to check (used for null checking)</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(object? value = null)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures TypeConverter operations are valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with a list of problems</exception>
    public static void EnsureValid(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"TypeConverter validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}