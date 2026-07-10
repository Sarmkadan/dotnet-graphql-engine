using System;
using System.Text.Json;

namespace GraphQLEngine.Benchmarks
{
	/// <summary>
	/// Provides JSON serialization and deserialization extensions for <see cref="CachingBenchmarksExtensions"/>
	/// to enable caching benchmark configuration to be persisted or transmitted.
	/// </summary>
	public static class CachingBenchmarksExtensionsJsonExtensions
	{
		private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		};

		/// <summary>
		/// Serializes the <see cref="CachingBenchmarksExtensions"/> instance to a JSON string.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
		/// <returns>A JSON string representation of the value.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
		public static string ToJson(this CachingBenchmarksExtensions value, bool indented = false) =>
			value == null
				? throw new ArgumentNullException(nameof(value))
				: JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

		/// <summary>
		/// Deserializes a JSON string to a <see cref="CachingBenchmarksExtensions"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize.</param>
		/// <returns>The deserialized instance, or null if the JSON is null or empty.
		/// Returns null if deserialization fails due to invalid JSON.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
		public static CachingBenchmarksExtensions? FromJson(string json)
		{
			ArgumentNullException.ThrowIfNull(json);

			if (string.IsNullOrWhiteSpace(json))
			{
				return null;
			}

			try
			{
				return JsonSerializer.Deserialize<CachingBenchmarksExtensions>(json, _jsonOptions);
			}
			catch (JsonException)
			{
				return null;
			}
		}

		/// <summary>
		/// Attempts to deserialize a JSON string to a <see cref="CachingBenchmarksExtensions"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize.</param>
		/// <param name="value">Receives the deserialized instance if successful.</param>
		/// <returns>True if deserialization succeeded; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
		public static bool TryFromJson(string json, out CachingBenchmarksExtensions? value)
		{
			ArgumentNullException.ThrowIfNull(json);

			value = null;

			if (string.IsNullOrWhiteSpace(json))
			{
				return false;
			}

			try
			{
				value = JsonSerializer.Deserialize<CachingBenchmarksExtensions>(json, _jsonOptions);
				return true;
			}
			catch (JsonException)
			{
				return false;
			}
		}
	}
}