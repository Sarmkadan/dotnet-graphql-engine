#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides JSON serialization and deserialization extensions for the <see cref="ValidationHelper"/> type.
/// </summary>
public static class ValidationHelperJsonExtensions
{
    /// <summary>
    /// Private static readonly JSON serializer options with camelCase naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a marker representing the <see cref="ValidationHelper"/> type to a JSON string.
    /// </summary>
    /// <param name="value">A marker object (ValidationHelper type instance is always null for static classes).</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the ValidationHelper type marker.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this object? value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializerOptions options = new(JsonSerializerOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(new { Type = nameof(ValidationHelper) }, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ValidationHelper"/> marker.
    /// Since ValidationHelper is a static class, this returns null if deserialization succeeds.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="ValidationHelper"/> marker (always null for static classes) if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
    public static object? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var result = JsonSerializer.Deserialize<ValidationHelperWrapper>(json, JsonSerializerOptions);
            return result?.Type == nameof(ValidationHelper) ? null : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ValidationHelper"/> marker.
    /// Since ValidationHelper is a static class, this returns false if the JSON represents a ValidationHelper marker.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="ValidationHelper"/> marker (always null for static classes).</param>
    /// <returns>True if deserialization succeeded and represents ValidationHelper; otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var result = JsonSerializer.Deserialize<ValidationHelperWrapper>(json, JsonSerializerOptions);
            if (result?.Type == nameof(ValidationHelper))
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
    /// Internal wrapper type for serializing/deserializing ValidationHelper as a marker type.
    /// </summary>
    private sealed class ValidationHelperWrapper
    {
        public string? Type { get; set; }
    }
}