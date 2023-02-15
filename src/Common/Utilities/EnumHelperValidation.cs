#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides validation helpers for EnumHelper operations
/// </summary>
public static class EnumHelperValidation
{
    /// <summary>
    /// Validates an enum value and returns a list of human-readable problems
    /// </summary>
    /// <typeparam name="T">The enum type to validate</typeparam>
    /// <param name="value">The enum value to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate<T>(T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate GetEnumValues returns non-null collection
        try
        {
            var values = EnumHelper.GetEnumValues<T>();
            if (values is null)
                problems.Add("GetEnumValues<T>() returned null");
            else if (values.Count == 0)
                problems.Add("GetEnumValues<T>() returned empty collection");
        }
        catch (Exception ex)
        {
            problems.Add($"GetEnumValues<T>() threw: {ex.Message}");
        }

        // Validate GetEnumNames returns non-null collection
        try
        {
            var names = EnumHelper.GetEnumNames<T>();
            if (names is null)
                problems.Add("GetEnumNames<T>() returned null");
            else if (names.Count == 0)
                problems.Add("GetEnumNames<T>() returned empty collection");
        }
        catch (Exception ex)
        {
            problems.Add($"GetEnumNames<T>() threw: {ex.Message}");
        }

        // Validate Parse behavior
        try
        {
            var parsedNull = EnumHelper.Parse<T>(null);
            if (parsedNull is not null)
                problems.Add("Parse<T>(null) should return null");
        }
        catch (Exception ex)
        {
            problems.Add($"Parse<T>(null) threw: {ex.Message}");
        }

        try
        {
            var parsedEmpty = EnumHelper.Parse<T>("   ");
            if (parsedEmpty is not null)
                problems.Add("Parse<T>(empty string) should return null");
        }
        catch (Exception ex)
        {
            problems.Add($"Parse<T>(empty string) threw: {ex.Message}");
        }

        // Validate TryParse behavior
        try
        {
            var result = EnumHelper.TryParse<T>(null, out _);
            if (result)
                problems.Add("TryParse<T>(null) should return false");
        }
        catch (Exception ex)
        {
            problems.Add($"TryParse<T>(null) threw: {ex.Message}");
        }

        try
        {
            var result = EnumHelper.TryParse<T>("   ", out _);
            if (result)
                problems.Add("TryParse<T>(empty string) should return false");
        }
        catch (Exception ex)
        {
            problems.Add($"TryParse<T>(empty string) threw: {ex.Message}");
        }

        // Validate GetDisplayName returns non-null for valid value
        try
        {
            var displayName = EnumHelper.GetDisplayName(value);
            if (displayName is null)
                problems.Add("GetDisplayName returned null for valid enum value");
        }
        catch (Exception ex)
        {
            problems.Add($"GetDisplayName threw: {ex.Message}");
        }

        // Validate GetDescription doesn't throw for valid value
        try
        {
            var description = EnumHelper.GetDescription(value);
            // Description can be null, that's acceptable
        }
        catch (Exception ex)
        {
            problems.Add($"GetDescription threw: {ex.Message}");
        }

        // Validate HasAttribute and GetAttributes don't throw for valid value
        try
        {
            var hasAttr = EnumHelper.HasAttribute<T, FlagsAttribute>(value);
            var attrs = EnumHelper.GetAttributes<T, FlagsAttribute>(value);
        }
        catch (Exception ex)
        {
            problems.Add($"HasAttribute/GetAttributes threw: {ex.Message}");
        }

        // Validate GetEnumDisplayDictionary returns non-null
        try
        {
            var dict = EnumHelper.GetEnumDisplayDictionary<T>();
            if (dict is null)
                problems.Add("GetEnumDisplayDictionary returned null");
            else if (dict.Count == 0)
                problems.Add("GetEnumDisplayDictionary returned empty dictionary");
        }
        catch (Exception ex)
        {
            problems.Add($"GetEnumDisplayDictionary threw: {ex.Message}");
        }

        // Validate GetNextValue/GetPreviousValue behavior
        try
        {
            var values = EnumHelper.GetEnumValues<T>();
            if (values.Count > 0)
            {
                var first = values.First();
                var next = EnumHelper.GetNextValue(first);
                var prev = EnumHelper.GetPreviousValue(first);

                if (prev is not null)
                    problems.Add("GetPreviousValue(first) should return null for first element");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetNextValue/GetPreviousValue threw: {ex.Message}");
        }

        // Validate IsFlagsEnum doesn't throw
        try
        {
            var isFlags = EnumHelper.IsFlagsEnum<T>();
            // Can be true or false, both are acceptable
        }
        catch (Exception ex)
        {
            problems.Add($"IsFlagsEnum threw: {ex.Message}");
        }

        // Validate CombineFlags behavior
        try
        {
            var combined = EnumHelper.CombineFlags<T>();
            if (combined is not null)
                problems.Add("CombineFlags<T>() with no args should return default");
        }
        catch (Exception ex)
        {
            problems.Add($"CombineFlags<T>() threw: {ex.Message}");
        }

        try
        {
            var values = EnumHelper.GetEnumValues<T>();
            if (values.Count > 0)
            {
                var combined = EnumHelper.CombineFlags(values.First());
                if (combined is null)
                    problems.Add("CombineFlags(single arg) should not return null");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"CombineFlags(single arg) threw: {ex.Message}");
        }

        // Validate HasFlag behavior
        try
        {
            var hasFlag = EnumHelper.HasFlag(value, value);
            // Should not throw
        }
        catch (Exception ex)
        {
            problems.Add($"HasFlag threw: {ex.Message}");
        }

        // Validate GetFlags behavior
        try
        {
            var flags = EnumHelper.GetFlags(value);
            if (flags is null)
                problems.Add("GetFlags returned null");
        }
        catch (Exception ex)
        {
            problems.Add($"GetFlags threw: {ex.Message}");
        }

        // Validate GetUnderlyingValue returns non-null
        try
        {
            var underlying = EnumHelper.GetUnderlyingValue(value);
            if (underlying is null)
                problems.Add("GetUnderlyingValue returned null");
        }
        catch (Exception ex)
        {
            problems.Add($"GetUnderlyingValue threw: {ex.Message}");
        }

        // Validate IsValidEnumValue behavior
        try
        {
            var isValid = EnumHelper.IsValidEnumValue<T>("SomeInvalidName");
            if (isValid)
                problems.Add("IsValidEnumValue should return false for invalid name");
        }
        catch (Exception ex)
        {
            problems.Add($"IsValidEnumValue threw: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an enum value is valid
    /// </summary>
    /// <typeparam name="T">The enum type to validate</typeparam>
    /// <param name="value">The enum value to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid<T>(T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures an enum value is valid, throwing ArgumentException if not
    /// </summary>
    /// <typeparam name="T">The enum type to validate</typeparam>
    /// <param name="value">The enum value to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing problem descriptions</exception>
    public static void EnsureValid<T>(T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Enum validation failed for type {typeof(T).Name}:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }
}