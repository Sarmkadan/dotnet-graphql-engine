// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Helper utilities for working with enumerations
/// Provides reflection-based enum operations
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// Gets all values of an enum type
    /// </summary>
    public static List<T> GetEnumValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }

    /// <summary>
    /// Gets all names of an enum type
    /// </summary>
    public static List<string> GetEnumNames<T>() where T : Enum
    {
        return Enum.GetNames(typeof(T)).ToList();
    }

    /// <summary>
    /// Converts a string to an enum value
    /// </summary>
    public static T? Parse<T>(string? value, bool ignoreCase = true) where T : Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return default;

        try
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Tries to parse a string to an enum value
    /// </summary>
    public static bool TryParse<T>(string? value, out T? result, bool ignoreCase = true) where T : Enum
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            result = (T)Enum.Parse(typeof(T), value, ignoreCase);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the display name of an enum value
    /// </summary>
    public static string? GetDisplayName<T>(T value) where T : Enum
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
            return value.ToString();

        var attr = field.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
        return attr?.Name ?? value.ToString();
    }

    /// <summary>
    /// Gets the description of an enum value
    /// </summary>
    public static string? GetDescription<T>(T value) where T : Enum
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
            return null;

        var attr = field.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
        return attr?.Description;
    }

    /// <summary>
    /// Checks if an enum value has a specific attribute
    /// </summary>
    public static bool HasAttribute<T, TAttr>(T value) where T : Enum where TAttr : Attribute
    {
        var field = value.GetType().GetField(value.ToString());
        return field?.GetCustomAttribute<TAttr>() != null;
    }

    /// <summary>
    /// Gets all attributes of an enum value
    /// </summary>
    public static List<TAttr> GetAttributes<T, TAttr>(T value) where T : Enum where TAttr : Attribute
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
            return new List<TAttr>();

        return field.GetCustomAttributes(typeof(TAttr), false).Cast<TAttr>().ToList();
    }

    /// <summary>
    /// Converts enum values to a dictionary with display names
    /// </summary>
    public static Dictionary<string, string> GetEnumDisplayDictionary<T>() where T : Enum
    {
        var result = new Dictionary<string, string>();

        foreach (var value in GetEnumValues<T>())
        {
            var displayName = GetDisplayName(value) ?? value.ToString();
            result[value.ToString()!] = displayName;
        }

        return result;
    }

    /// <summary>
    /// Gets the next enum value in sequence
    /// </summary>
    public static T? GetNextValue<T>(T current) where T : Enum
    {
        var values = GetEnumValues<T>();
        var currentIndex = values.IndexOf(current);

        if (currentIndex < 0 || currentIndex >= values.Count - 1)
            return default;

        return values[currentIndex + 1];
    }

    /// <summary>
    /// Gets the previous enum value in sequence
    /// </summary>
    public static T? GetPreviousValue<T>(T current) where T : Enum
    {
        var values = GetEnumValues<T>();
        var currentIndex = values.IndexOf(current);

        if (currentIndex <= 0)
            return default;

        return values[currentIndex - 1];
    }

    /// <summary>
    /// Checks if an enum is a flags enum
    /// </summary>
    public static bool IsFlagsEnum<T>() where T : Enum
    {
        return typeof(T).GetCustomAttribute<FlagsAttribute>() != null;
    }

    /// <summary>
    /// Combines flags enum values
    /// </summary>
    public static T CombineFlags<T>(params T[] values) where T : Enum
    {
        if (values == null || values.Length == 0)
            return default!;

        if (!IsFlagsEnum<T>())
            throw new InvalidOperationException("Type is not a Flags enum");

        var result = 0;
        foreach (var value in values)
        {
            result |= (int)(object)value;
        }

        return (T)(object)result;
    }

    /// <summary>
    /// Checks if a flag is set
    /// </summary>
    public static bool HasFlag<T>(T value, T flag) where T : Enum
    {
        var intValue = (int)(object)value;
        var intFlag = (int)(object)flag;

        return (intValue & intFlag) == intFlag;
    }

    /// <summary>
    /// Gets all enum values of a specific type from a flags enum
    /// </summary>
    public static List<T> GetFlags<T>(T combined) where T : Enum
    {
        var result = new List<T>();
        var allValues = GetEnumValues<T>();

        foreach (var value in allValues)
        {
            if (HasFlag(combined, value))
                result.Add(value);
        }

        return result;
    }

    /// <summary>
    /// Converts enum to its underlying value
    /// </summary>
    public static object GetUnderlyingValue<T>(T value) where T : Enum
    {
        var underlyingType = Enum.GetUnderlyingType(typeof(T));
        return Convert.ChangeType(value, underlyingType)!;
    }

    /// <summary>
    /// Checks if a string is a valid enum value
    /// </summary>
    public static bool IsValidEnumValue<T>(string value) where T : Enum
    {
        return GetEnumNames<T>().Contains(value, StringComparer.OrdinalIgnoreCase);
    }
}
