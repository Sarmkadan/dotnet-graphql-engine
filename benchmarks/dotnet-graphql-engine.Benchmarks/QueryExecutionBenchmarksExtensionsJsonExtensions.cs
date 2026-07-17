using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQLEngine.Benchmarks;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="QueryExecutionBenchmarksExtensions"/>.
/// </summary>
public sealed class QueryExecutionBenchmarksExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes the <see cref="QueryExecutionBenchmarksExtensions"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this QueryExecutionBenchmarksExtensions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="QueryExecutionBenchmarksExtensions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="QueryExecutionBenchmarksExtensions"/> instance, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized into a <see cref="QueryExecutionBenchmarksExtensions"/> instance.</exception>
    public static QueryExecutionBenchmarksExtensions? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<QueryExecutionBenchmarksExtensions>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="QueryExecutionBenchmarksExtensions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized value if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out QueryExecutionBenchmarksExtensions? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<QueryExecutionBenchmarksExtensions>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}