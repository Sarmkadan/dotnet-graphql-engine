#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQLEngine.Common.Utilities;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides JSON serialization and deserialization extensions for working with <see cref="Enum"/> types.
/// </summary>
public static class EnumHelperJsonExtensions
{
    /// <summary>
    /// Private static readonly JSON serializer options with camelCase naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Serializes an <see cref="Enum"/> instance to a JSON string.
    /// </summary>
    /// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
    /// <param name="value">The <see cref="Enum"/> to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="Enum"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson<T>(this T value, bool indented = false) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializerOptions options = new(JsonSerializerOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="Enum"/> instance.
    /// </summary>
    /// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An <see cref="Enum"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
    public static T? FromJson<T>(string json) where T : Enum
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to an <see cref="Enum"/> instance.
    /// </summary>
    /// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="Enum"/> if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    public static bool TryFromJson<T>(string json, out T? value) where T : Enum
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }
}
