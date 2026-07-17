using System;
using System.Text.Json;

public static class GraphQLHttpRequestJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Converts the <see cref="GraphQLHttpRequest"/> to a JSON string.
	/// </summary>
	/// <param name="value">The <see cref="GraphQLHttpRequest"/> to convert.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON string representation of the <see cref="GraphQLHttpRequest"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this GraphQLHttpRequest value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);
		return JsonSerializer.Serialize(value, indented ? _jsonSerializerOptions with { WriteIndented = true } : _jsonSerializerOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string into a <see cref="GraphQLHttpRequest"/>.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A deserialized <see cref="GraphQLHttpRequest"/> or <see langword="null"/> if deserialization fails.</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
	/// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized into a <see cref="GraphQLHttpRequest"/>.</exception>
	public static GraphQLHttpRequest? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);
		return JsonSerializer.Deserialize<GraphQLHttpRequest>(json, _jsonSerializerOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string into a <see cref="GraphQLHttpRequest"/>.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">The deserialized <see cref="GraphQLHttpRequest"/> or <see langword="null"/> if deserialization fails.</param>
	/// <returns><see langword="true"/> if deserialization is successful; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
	public static bool TryFromJson(string json, out GraphQLHttpRequest? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);
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
