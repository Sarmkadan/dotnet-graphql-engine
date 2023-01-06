#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for TypeConverter operations
/// </summary>
public static class TypeConverterJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
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
    public static string ToJson(this object? value, bool indented = false)
    {
        var jsonCompatible = TypeConverter.ToJsonCompatible(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(jsonCompatible, options);
    }

    /// <summary>
    /// Deserializes a value from JSON string using TypeConverter.Convert
    /// </summary>
    /// <typeparam name="T">Target type to convert to</typeparam>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized value or default if failed</returns>
    public static T? FromJson<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return default;

        try
        {
            var value = JsonSerializer.Deserialize<object>(json, _jsonOptions);
            return TypeConverter.Convert<T>(value);
        }
        catch (JsonException)
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
    public static bool TryFromJson<T>(string json, out T? value)
    {
        try
        {
            value = FromJson<T>(json);
            return true;
        }
        catch (JsonException)
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
    public static object? FromJson(string json, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return TypeConverter.GetDefaultValue(targetType);

        try
        {
            var value = JsonSerializer.Deserialize<object>(json, _jsonOptions);
            return TypeConverter.Convert(value, targetType);
        }
        catch (JsonException)
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
    public static bool TryFromJson(string json, Type targetType, out object? value)
    {
        try
        {
            value = FromJson(json, targetType);
            return true;
        }
        catch (JsonException)
        {
            value = TypeConverter.GetDefaultValue(targetType);
            return false;
        }
    }
}