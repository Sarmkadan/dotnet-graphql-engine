#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides JSON serialization and deserialization extension methods for DateTime values
/// with validation through the <see cref="DateTimeExtensionsValidation"/> class.
/// </summary>
public static class DateTimeExtensionsValidationJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a DateTime value to its JSON string representation with validation.
    /// </summary>
    /// <param name="dateTime">The DateTime value to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>The JSON string representation of the value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is MinValue or MaxValue.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static string ToJson(this DateTime dateTime, bool indented = false)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfEqual(dateTime, DateTime.MaxValue, nameof(dateTime));

        dateTime.EnsureValid();

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(dateTime, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a DateTime value.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>The deserialized DateTime value, or null if parsing fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static DateTime? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<DateTime>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a DateTime value.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output parameter for the deserialized value.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out DateTime? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            value = JsonSerializer.Deserialize<DateTime>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}