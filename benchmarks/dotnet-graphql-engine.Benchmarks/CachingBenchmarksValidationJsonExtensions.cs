namespace benchmarks.dotnet_graphql_engine.Benchmarks;

public static class CachingBenchmarksValidationJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Converts a <see cref="CachingBenchmarksValidation"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="CachingBenchmarksValidation"/> to convert.</param>
    /// <param name="indented">Whether to indent the JSON.</param>
    /// <returns>The JSON string.</returns>
    public static string ToJson(this CachingBenchmarksValidation value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented ? JsonOptions : new JsonSerializerOptions(JsonOptions)
        {
            WriteIndented = false,
        };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="CachingBenchmarksValidation"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="CachingBenchmarksValidation"/>.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static CachingBenchmarksValidation? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<CachingBenchmarksValidation>(json, JsonOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a <see cref="CachingBenchmarksValidation"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="CachingBenchmarksValidation"/>.</param>
    /// <returns>Whether the deserialization was successful.</returns>
    public static bool TryFromJson(string json, out CachingBenchmarksValidation? value)
    {
        try
        {
            value = FromJson(json);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
