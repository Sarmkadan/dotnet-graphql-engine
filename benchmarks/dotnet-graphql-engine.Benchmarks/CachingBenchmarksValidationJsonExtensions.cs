namespace benchmarks.dotnet_graphql_engine.Benchmarks;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="CachingBenchmarksValidation"/>.
/// </summary>
public static class CachingBenchmarksValidationJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this CachingBenchmarksValidation value, bool indented = false)
        => JsonSerializer.Serialize(value, indented ? JsonOptions : new JsonSerializerOptions(JsonOptions) { WriteIndented = false });

    /// <summary>
    /// Deserializes a JSON string to a <see cref="CachingBenchmarksValidation"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="CachingBenchmarksValidation"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static CachingBenchmarksValidation? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<CachingBenchmarksValidation>(json, JsonOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a <see cref="CachingBenchmarksValidation"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="CachingBenchmarksValidation"/>.</param>
    /// <returns>Whether the deserialization was successful.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out CachingBenchmarksValidation? value)
    {
        try
        {
            value = FromJson(json);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
