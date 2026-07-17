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
    };

    /// <summary>
    /// Serializes the <paramref name="value"/> to a JSON string, optionally with indentation.
    /// </summary>
    /// <param name="value">The <see cref="GraphQLException"/> to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>The JSON string representation of the <paramref name="value"/>.</returns>
    public static string ToJson(this GraphQLException value, bool indented = false)
    {
        if (indented)
        {
            JsonOptions.WriteIndented = true;
        }

        return JsonSerializer.Serialize(value, JsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="GraphQLException"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="GraphQLException"/> or <c>null</c> if deserialization fails.</returns>
    public static GraphQLException? FromJson(string json)
    {
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
    /// <returns><c>true</c> if deserialization succeeds, <c>false</c> otherwise.</returns>
    public static bool TryFromJson(string json, out GraphQLException? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<GraphQLException>(json, JsonOptions);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
