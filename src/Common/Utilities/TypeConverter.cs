#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Generic type conversion utility with support for various data types
/// Handles nullable types, enums, and custom conversions
/// </summary>
public static class TypeConverter
{
    /// <summary>
    /// Converts a value to the specified target type
    /// </summary>
    public static T? Convert<T>(object? value)
    {
        if (value is null)
            return default;

        try
        {
            var targetType = typeof(T);

            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // If value is already the correct type, return it
            if (underlyingType.IsAssignableFrom(value.GetType()))
                return (T)value;

            // Handle string conversions
            if (underlyingType == typeof(string))
                return (T)(object)value.ToString()!;

            // Handle numeric conversions
            if (underlyingType == typeof(int))
                return (T)(object)System.Convert.ToInt32(value, CultureInfo.InvariantCulture);

            if (underlyingType == typeof(long))
                return (T)(object)System.Convert.ToInt64(value, CultureInfo.InvariantCulture);

            if (underlyingType == typeof(float))
                return (T)(object)System.Convert.ToSingle(value, CultureInfo.InvariantCulture);

            if (underlyingType == typeof(double))
                return (T)(object)System.Convert.ToDouble(value, CultureInfo.InvariantCulture);

            if (underlyingType == typeof(decimal))
                return (T)(object)System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);

            // Handle boolean conversions
            if (underlyingType == typeof(bool))
                return (T)(object)ConvertToBoolean(value);

            // Handle date/time conversions
            if (underlyingType == typeof(DateTime))
                return (T)(object)System.Convert.ToDateTime(value, CultureInfo.InvariantCulture);

            if (underlyingType == typeof(DateTimeOffset))
                return (T)(object)DateTimeOffset.Parse(value.ToString()!, CultureInfo.InvariantCulture);

            if (underlyingType == typeof(TimeSpan))
                return (T)(object)TimeSpan.Parse(value.ToString()!, CultureInfo.InvariantCulture);

            // Handle GUID
            if (underlyingType == typeof(Guid))
                return (T)(object)Guid.Parse(value.ToString()!);

            // Handle enums
            if (underlyingType.IsEnum)
                return (T)Enum.Parse(underlyingType, value.ToString()!);

            // Fallback: use standard Convert.ChangeType
            return (T)System.Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture)!;
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Converts a value to the specified target type (non-generic)
    /// </summary>
    public static object? Convert(object? value, Type targetType)
    {
        if (value is null)
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        try
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType.IsAssignableFrom(value.GetType()))
                return value;

            if (underlyingType.IsEnum)
                return Enum.Parse(underlyingType, value.ToString()!);

            return System.Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Tries to convert a value and returns success flag
    /// </summary>
    public static bool TryConvert<T>(object? value, out T? result)
    {
        try
        {
            result = Convert<T>(value);
            return result is not null || value is null;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Converts a boolean-like value to bool
    /// </summary>
    private static bool ConvertToBoolean(object value)
    {
        if (value is null)
            return false;

        var strValue = value.ToString()?.ToLowerInvariant();
        return strValue switch
        {
            "true" or "1" or "yes" or "on" => true,
            "false" or "0" or "no" or "off" => false,
            _ => bool.Parse(strValue!)
        };
    }

    /// <summary>
    /// Converts between compatible types with proper type checking
    /// </summary>
    public static bool CanConvert(Type from, Type to)
    {
        if (from is null || to is null)
            return false;

        var underlyingTarget = Nullable.GetUnderlyingType(to) ?? to;

        // Direct assignment
        if (underlyingTarget.IsAssignableFrom(from))
            return true;

        // Standard conversions
        if (underlyingTarget.IsPrimitive || underlyingTarget == typeof(string) ||
            underlyingTarget == typeof(decimal) || underlyingTarget == typeof(DateTime))
            return true;

        // Enum conversions
        if (underlyingTarget.IsEnum)
            return from == typeof(string) || from.IsEnum || from.IsPrimitive;

        return false;
    }

    /// <summary>
    /// Gets the default value for a type
    /// </summary>
    public static object? GetDefaultValue(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    /// <summary>
    /// Converts a collection of values to a target type
    /// </summary>
    public static List<T?> ConvertList<T>(IEnumerable<object?>? values)
    {
        if (values is null)
            return new List<T?>();

        return values.Select(v => Convert<T>(v)).ToList();
    }

    /// <summary>
    /// Converts a value to JSON-compatible representation
    /// </summary>
    public static object? ToJsonCompatible(object? value)
    {
        if (value is null)
            return null;

        var type = value.GetType();

        // Already JSON compatible
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            return value;

        // Handle collections
        if (value is System.Collections.IEnumerable enumerable && type != typeof(string))
        {
            var list = new List<object?>();
            foreach (var item in enumerable)
                list.Add(ToJsonCompatible(item));
            return list;
        }

        // Handle objects by converting to dictionary
        if (type.IsClass)
        {
            var dict = new Dictionary<string, object?>();
            var properties = ReflectionHelper.GetPublicProperties(type);
            foreach (var prop in properties)
            {
                try
                {
                    var propValue = ReflectionHelper.GetPropertyValue(value, prop.Name);
                    dict[prop.Name] = ToJsonCompatible(propValue);
                }
                catch { }
            }
            return dict;
        }

        return value.ToString();
    }
}
