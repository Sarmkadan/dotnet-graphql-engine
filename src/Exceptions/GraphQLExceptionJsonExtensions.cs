#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQLEngine.Exceptions;

namespace GraphQLEngine.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="GraphQLException"/>.
/// </summary>
public static class GraphQLExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes the <paramref name="value"/> to a JSON string, optionally with indentation.
    /// </summary>
    /// <param name="value">The <see cref="GraphQLException"/> to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>The JSON string representation of the <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this GraphQLException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = JsonOptions;
        options.WriteIndented = indented;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="GraphQLException"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="GraphQLException"/> or <c>null</c> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
    public static GraphQLException? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<GraphQLException>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="GraphQLException"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="GraphQLException"/> or <c>null</c> if deserialization fails.</param>
    /// <returns><c>true</c> if deserialization succeeds and produces a non-null value, <c>false</c> otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out GraphQLException? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<GraphQLException>(json, JsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
