#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides JSON serialization extension methods compatible with JsonHelper's serialization approach.
/// These methods offer a fluent API for JSON serialization and deserialization using JsonHelper's conventions.
/// </summary>
public static class JsonHelperJsonExtensions
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes an object to a JSON string using JsonHelper-compatible serialization options.
    /// </summary>
    /// <param name="value">The object to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the object</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }
            : CamelCaseOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an object using JsonHelper-compatible options.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>An instance of type T if successful; otherwise, null</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or whitespace</exception>
    public static T? FromJson<T>(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<T>(json, CamelCaseOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an object of type T.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Output parameter that receives the deserialized object if successful</param>
    /// <returns>true if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="ArgumentException">Thrown when json is null or whitespace</exception>
    public static bool TryFromJson<T>(string json, out T? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<T>(json, CamelCaseOptions);
            return true;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }
}