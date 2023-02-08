#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using GraphQLEngine.Configuration;

namespace GraphQLEngine.Configuration;

/// <summary>
/// Provides JSON serialization and deserialization extensions for the <see cref="PersistedQueryExtensions"/> type.
/// </summary>
public static class PersistedQueryExtensionsJsonExtensions
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
    /// Serializes a <see cref="PersistedQueryOptions"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="PersistedQueryOptions"/> to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="PersistedQueryOptions"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this PersistedQueryOptions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(JsonSerializerOptions)
        {
            WriteIndented = indented,
        };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PersistedQueryOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="PersistedQueryOptions"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
    public static PersistedQueryOptions? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<PersistedQueryOptions>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a <see cref="PersistedQueryOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="PersistedQueryOptions"/> if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    public static bool TryFromJson(string json, out PersistedQueryOptions? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<PersistedQueryOptions>(json, JsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
