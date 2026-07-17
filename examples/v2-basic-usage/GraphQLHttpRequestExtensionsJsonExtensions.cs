using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace examples.v2_basic_usage
{
/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="GraphQLHttpRequest"/> objects.
/// </summary>
public static class GraphQLHttpRequestExtensionsJsonExtensions
{
private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
{
PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
WriteIndented = false
};

/// <summary>
/// Converts a <see cref="GraphQLHttpRequest"/> to a JSON string representation.
/// </summary>
/// <param name="value">The <see cref="GraphQLHttpRequest"/> to serialize. Cannot be null.</param>
/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
/// <returns>A JSON string representation of the request.</returns>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
public static string ToJson(this GraphQLHttpRequest value, bool indented = false)
{
ArgumentNullException.ThrowIfNull(value);

var options = indented
? _jsonSerializerOptions with { WriteIndented = true }
: _jsonSerializerOptions;

return JsonSerializer.Serialize(value, options);
}

/// <summary>
/// Attempts to deserialize a JSON string into a <see cref="GraphQLHttpRequest"/>.
/// </summary>
/// <param name="json">The JSON string to deserialize. Cannot be null or whitespace.</param>
/// <returns>A deserialized <see cref="GraphQLHttpRequest"/> or null if deserialization fails.</returns>
/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null, empty, or consists only of whitespace.</exception>
public static GraphQLHttpRequest? FromJson(string json)
{
ArgumentException.ThrowIfNullOrWhiteSpace(json);

try
{
return JsonSerializer.Deserialize<GraphQLHttpRequest>(json, _jsonSerializerOptions);
}
catch (JsonException)
{
return null;
}
}

/// <summary>
/// Attempts to deserialize a JSON string into a <see cref="GraphQLHttpRequest"/>.
/// </summary>
/// <param name="json">The JSON string to deserialize. Cannot be null or whitespace.</param>
/// <param name="value">The deserialized <see cref="GraphQLHttpRequest"/> or null if deserialization fails.</param>
/// <returns><see langword="true"/> if deserialization is successful; otherwise, <see langword="false"/>.</returns>
/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null, empty, or consists only of whitespace.</exception>
public static bool TryFromJson(string json, out GraphQLHttpRequest? value)
{
ArgumentException.ThrowIfNullOrWhiteSpace(json);

try
{
value = JsonSerializer.Deserialize<GraphQLHttpRequest>(json, _jsonSerializerOptions);
return value is not null;
}
catch (JsonException)
{
value = null;
return false;
}
}
}
}