#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extension methods for the <see cref="ReflectionHelperJsonExtensions"/> type.
/// These methods serialize and deserialize the ReflectionHelperJsonExtensions marker type for use in JSON contexts.
/// </summary>
public static class ReflectionHelperJsonExtensionsJsonExtensions
{
    /// <summary>
    /// Private static readonly JSON serializer options with camelCase naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes a marker representing the <see cref="ReflectionHelperJsonExtensions"/> type to a JSON string.
    /// </summary>
    /// <param name="value">A marker object (ReflectionHelperJsonExtensions type instance is always null for static classes).</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the ReflectionHelperJsonExtensions type marker.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this object? value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializerOptions options = new(JsonSerializerOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(new { Type = nameof(ReflectionHelperJsonExtensions) }, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ReflectionHelperJsonExtensions"/> marker.
    /// Since ReflectionHelperJsonExtensions is a static class, this returns null if deserialization succeeds.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="ReflectionHelperJsonExtensions"/> marker (always null for static classes) if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    public static object? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var result = JsonSerializer.Deserialize<ReflectionHelperWrapper>(json, JsonSerializerOptions);
            return result?.Type == nameof(ReflectionHelperJsonExtensions) ? null : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ReflectionHelperJsonExtensions"/> marker.
    /// Since ReflectionHelperJsonExtensions is a static class, this returns false if the JSON represents a ReflectionHelperJsonExtensions marker.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="ReflectionHelperJsonExtensions"/> marker (always null for static classes).</param>
    /// <returns>True if deserialization succeeded and represents ReflectionHelperJsonExtensions; otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var result = JsonSerializer.Deserialize<ReflectionHelperWrapper>(json, JsonSerializerOptions);
            if (result?.Type == nameof(ReflectionHelperJsonExtensions))
            {
                value = null;
                return true;
            }

            value = null;
            return false;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Internal wrapper type for serializing/deserializing ReflectionHelperJsonExtensions as a marker type.
    /// </summary>
    private sealed class ReflectionHelperWrapper
    {
        public string? Type { get; set; }
    }
}
