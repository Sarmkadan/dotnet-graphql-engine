using System;
using System.Text.Json;

namespace GraphQLEngine.Benchmarks
{
	/// <summary>
	/// Provides JSON serialization and deserialization extensions for <see cref="CachingBenchmarks"/>
	/// to enable caching benchmark configuration to be persisted or transmitted.
	/// </summary>
	public static class CachingBenchmarksJsonExtensions
	{
		private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		/// <summary>
		/// Serializes the <see cref="CachingBenchmarks"/> instance to a JSON string.
		/// </summary>
		/// <param name="value">The value to serialize.</param>
		/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
		/// <returns>A JSON string representation of the value.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
		public static string ToJson(this CachingBenchmarks value, bool indented = false)
		{
			ArgumentNullException.ThrowIfNull(value);

			var options = indented
				? new JsonSerializerOptions(_options) { WriteIndented = true }
				: _options;

			return JsonSerializer.Serialize(value, options);
		}

		/// <summary>
		/// Deserializes a JSON string to a <see cref="CachingBenchmarks"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize.</param>
		/// <returns>The deserialized instance, or null if the JSON is null or empty.</returns>
		public static CachingBenchmarks? FromJson(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return null;
			}

			return JsonSerializer.Deserialize<CachingBenchmarks>(json, _options);
		}

		/// <summary>
		/// Attempts to deserialize a JSON string to a <see cref="CachingBenchmarks"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize.</param>
		/// <param name="value">Receives the deserialized instance if successful.</param>
		/// <returns>True if deserialization succeeded; otherwise, false.</returns>
		public static bool TryFromJson(string json, out CachingBenchmarks? value)
		{
			value = null;

			if (string.IsNullOrWhiteSpace(json))
			{
				return false;
			}

			try
			{
				value = JsonSerializer.Deserialize<CachingBenchmarks>(json, _options);
				return true;
			}
			catch (JsonException)
			{
				return false;
			}
		}
	}
}