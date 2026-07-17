using System;
using System.Text.Json;

namespace GraphQLEngine.Hosting;

public static class GraphQLHttpRequestValidationJsonExtensions
{
	private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
	};

	/// <summary>
	/// Converts a <see cref="GraphQLHttpRequestValidation"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The <see cref="GraphQLHttpRequestValidation"/> instance to convert. Cannot be null.</param>
	/// <param name="indented">Whether to indent the JSON output.</param>
	/// <returns>A JSON string representation of the <see cref="GraphQLHttpRequestValidation"/> instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this GraphQLHttpRequestValidation value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented ? new JsonSerializerOptions(s_jsonOptions) { WriteIndented = true } : s_jsonOptions;
		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="GraphQLHttpRequestValidation"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
	/// <returns>A <see cref="GraphQLHttpRequestValidation"/> instance deserialized from the JSON string, or <c>null</c> if the JSON cannot be deserialized.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
	/// <exception cref="JsonException">Thrown if the JSON string is invalid or cannot be deserialized.</exception>
	public static GraphQLHttpRequestValidation? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		return JsonSerializer.Deserialize<GraphQLHttpRequestValidation>(json, s_jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="GraphQLHttpRequestValidation"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
	/// <param name="value">The deserialized <see cref="GraphQLHttpRequestValidation"/> instance, or <c>null</c> if deserialization fails.</param>
	/// <returns><c>true</c> if deserialization succeeds; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
	public static bool TryFromJson(string json, out GraphQLHttpRequestValidation? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<GraphQLHttpRequestValidation>(json, s_jsonOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}