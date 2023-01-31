namespace Examples.V2BasicUsage;

public static class GraphQLHttpRequestValidationJsonExtensions
{
    private static readonly JsonSerializerOptions s_jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Converts a <see cref="GraphQLHttpRequestValidation"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="GraphQLHttpRequestValidation"/> instance to convert.</param>
    /// <param name="indented">Whether to indent the JSON output.</param>
    /// <returns>A JSON string representation of the <see cref="GraphQLHttpRequestValidation"/> instance.</returns>
    public static string ToJson(this GraphQLHttpRequestValidation value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented ? new JsonSerializerOptions(s_jsonOptions) { WriteIndented = true } : s_jsonOptions;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="GraphQLHttpRequestValidation"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="GraphQLHttpRequestValidation"/> instance deserialized from the JSON string, or <c>null</c> if the JSON string is null or empty.</returns>
    /// <exception cref="JsonException">Thrown if the JSON string is invalid.</exception>
    public static GraphQLHttpRequestValidation? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<GraphQLHttpRequestValidation>(json, s_jsonOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a <see cref="GraphQLHttpRequestValidation"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="GraphQLHttpRequestValidation"/> instance, or <c>null</c> if deserialization fails.</param>
    /// <returns><c>true</c> if deserialization succeeds, <c>false</c> otherwise.</returns>
    public static bool TryFromJson(string json, out GraphQLHttpRequestValidation? value)
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
