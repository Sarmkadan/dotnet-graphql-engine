#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides System.Text.Json serialization helpers for TypeConverterJsonExtensions
/// </summary>
public static class TypeConverterJsonExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes JSON serialization options to JSON string
    /// </summary>
    /// <param name="options">JSON serialization options to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static string ToJson(this JsonSerializerOptions options, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(options);

        var effectiveOptions = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(options, effectiveOptions);
    }

    /// <summary>
    /// Deserializes JSON string to JSON serialization options
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized JSON options or null if failed</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static JsonSerializerOptions? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<JsonSerializerOptions>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON string to JSON serialization options
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="options">Output parameter for deserialized options</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out JsonSerializerOptions? options)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            options = FromJson(json);
            return options is not null;
        }
        catch
        {
            options = null;
            return false;
        }
    }
}
