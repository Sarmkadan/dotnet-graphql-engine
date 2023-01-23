#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for TypeConverter operations
/// </summary>
public static class TypeConverterJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    /// <summary>
    /// Serializes a value to JSON string using TypeConverter.ToJsonCompatible
    /// </summary>
    /// <param name="value">Value to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/> and the method is called as an extension on a null instance.</exception>
    public static string ToJson(this object? value, bool indented = false)
    {
        var jsonCompatible = TypeConverter.ToJsonCompatible(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(jsonCompatible, options);
    }

    /// <summary>
    /// Deserializes a value from JSON string using TypeConverter.Convert
    /// </summary>
    /// <typeparam name="T">Target type to convert to</typeparam>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized value or default if failed</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static T? FromJson<T>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return default;

        try
        {
            var value = JsonSerializer.Deserialize<object>(json, _jsonOptions);
            return TypeConverter.Convert<T>(value);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Attempts to deserialize a value from JSON string using TypeConverter.Convert
    /// </summary>
    /// <typeparam name="T">Target type to convert to</typeparam>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for deserialized value</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson<T>(string json, out T? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = FromJson<T>(json);
            return value is not null || typeof(T).IsValueType;
        }
        catch
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Deserializes a value to a specific target type from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="targetType">Target type to convert to</param>
    /// <returns>Deserialized value or null if failed</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="targetType"/> is <see langword="null"/>.</exception>
    public static object? FromJson(string json, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(targetType);

        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return TypeConverter.GetDefaultValue(targetType);

        try
        {
            var value = JsonSerializer.Deserialize<object>(json, _jsonOptions);
            return TypeConverter.Convert(value, targetType);
        }
        catch
        {
            return TypeConverter.GetDefaultValue(targetType);
        }
    }

    /// <summary>
    /// Attempts to deserialize a value to a specific target type from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="targetType">Target type to convert to</param>
    /// <param name="value">Output parameter for deserialized value</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="targetType"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, Type targetType, out object? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(targetType);

        try
        {
            value = FromJson(json, targetType);
            return value is not null || targetType.IsValueType;
        }
        catch
        {
            value = TypeConverter.GetDefaultValue(targetType);
            return false;
        }
    }
}