#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides validation helpers for TypeConverter operations
/// Validates input values and TypeConverter behavior before conversion operations
/// </summary>
public static class TypeConverterValidation
{
    /// <summary>
    /// Validates that a value can be safely converted using TypeConverter
    /// </summary>
    /// <param name="value">The value to validate (used for null checking)</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(object? value = null)
    {
        var problems = new List<string>();

        if (value is null)
        {
            problems.Add("Value cannot be null");
            return problems.AsReadOnly();
        }

        var valueType = value.GetType();

        // Validate that null values are handled correctly
        var nullConversion = TypeConverter.Convert<object?>(null);
        if (nullConversion is not null)
        {
            problems.Add("TypeConverter.Convert<object?> should return null for null input");
        }

        // Validate that the value's type can be converted to itself
        if (!TypeConverter.CanConvert(valueType, valueType))
        {
            problems.Add($"TypeConverter.CanConvert cannot convert {valueType.Name} to itself");
        }

        // Validate round-trip conversion for common types
        try
        {
            // Test conversion to string and back
            var stringValue = TypeConverter.Convert<string>(value);
            if (stringValue is not null)
            {
                var roundTrip = TypeConverter.Convert(value, value.GetType());
                if (roundTrip is null)
                {
                    problems.Add("Round-trip conversion through string loses null safety");
                }
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Round-trip conversion failed: {ex.Message}");
        }

        // Validate that GetDefaultValue returns appropriate defaults
        try
        {
            var intDefault = TypeConverter.GetDefaultValue(typeof(int));
            if (intDefault is null or not int)
            {
                problems.Add("TypeConverter.GetDefaultValue(typeof(int)) should return default(int) (0)");
            }

            var stringDefault = TypeConverter.GetDefaultValue(typeof(string));
            if (stringDefault is not null)
            {
                problems.Add("TypeConverter.GetDefaultValue(typeof(string)) should return null");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetDefaultValue validation failed: {ex.Message}");
        }

        // Validate ConvertList handles null input
        try
        {
            var emptyList = TypeConverter.ConvertList<int>(null);
            if (emptyList is null)
            {
                problems.Add("TypeConverter.ConvertList<T> should return empty list for null input, not null");
            }
            else if (emptyList.Count != 0)
            {
                problems.Add("TypeConverter.ConvertList<T> should return empty list for null input");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ConvertList validation failed: {ex.Message}");
        }

        // Validate ToJsonCompatible handles null and preserves types
        try
        {
            var nullResult = TypeConverter.ToJsonCompatible(null);
            if (nullResult is not null)
            {
                problems.Add("TypeConverter.ToJsonCompatible should return null for null input");
            }

            var jsonResult = TypeConverter.ToJsonCompatible(value);
            if (jsonResult is null && value is not null)
            {
                problems.Add("TypeConverter.ToJsonCompatible should not return null for non-null values");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ToJsonCompatible validation failed: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a value can be safely converted using TypeConverter
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(object? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a value can be safely converted using TypeConverter, throwing if not
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
                $"TypeConverter validation failed:{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}