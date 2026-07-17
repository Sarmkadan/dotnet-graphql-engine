#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQLEngine.Examples;

public static class UserJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Converts a User object to a JSON string.
    /// </summary>
    /// <param name="value">The User object to convert.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the User object.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this User value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(
            value,
            indented
                ? _jsonSerializerOptions with { WriteIndented = true }
                : _jsonSerializerOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a User object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized User object or <c>null</c> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
    public static User? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<User>(
                _jsonSerializerOptions with { PropertyNameCaseInsensitive = true },
                json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a User object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized User object or <c>null</c> if deserialization fails.</param>
    /// <returns><c>true</c> if deserialization succeeds, <c>false</c> otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out User? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<User>(
                _jsonSerializerOptions with { PropertyNameCaseInsensitive = true },
                json);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}