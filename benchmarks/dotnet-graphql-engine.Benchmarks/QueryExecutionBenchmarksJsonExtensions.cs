using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace benchmarks.dotnet_graphql_engine.Benchmarks
{
	/// <summary>
	/// Provides JSON serialization and deserialization extensions for <see cref="QueryExecutionBenchmarks"/>.
	/// </summary>
	/// <remarks>
	/// Uses <see cref="JsonSerializerOptions"/> with camelCase property naming and no indentation by default.
	/// </remarks>
	public static class QueryExecutionBenchmarksJsonExtensions
	{
		private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false
		 };

		/// <summary>
		/// Serializes the <see cref="QueryExecutionBenchmarks"/> instance to a JSON string.
		/// </summary>
		/// <param name="value">The instance to serialize. Cannot be null.</param>
		/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
		/// <returns>A JSON string representation of the instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
		public static string ToJson(this QueryExecutionBenchmarks value, bool indented = false)
		{
			ArgumentNullException.ThrowIfNull(value);

			var options = new JsonSerializerOptions(DefaultOptions)
			{
				WriteIndented = indented
			};

			return JsonSerializer.Serialize(value, options);
		}

		/// <summary>
		/// Deserializes a JSON string to a <see cref="QueryExecutionBenchmarks"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
		/// <returns>The deserialized instance, or null if the JSON represents a null value.</returns>
		/// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
		/// <exception cref="JsonException">The JSON is invalid or cannot be deserialized.</exception>
		public static QueryExecutionBenchmarks? FromJson(string json)
		{
			ArgumentException.ThrowIfNullOrEmpty(json);

			return JsonSerializer.Deserialize<QueryExecutionBenchmarks>(json, DefaultOptions);
		}

		/// <summary>
		/// Attempts to deserialize a JSON string to a <see cref="QueryExecutionBenchmarks"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize. Can be null or empty.</param>
		/// <param name="value">Receives the deserialized instance if successful; otherwise, null.</param>
		/// <returns>True if deserialization succeeded; otherwise, false.</returns>
		/// <remarks>
		/// Does not throw for invalid JSON format. Returns false and sets <paramref name="value"/> to null.
		/// </remarks>
		public static bool TryFromJson(string json, out QueryExecutionBenchmarks? value)
		{
			value = null;

			if (string.IsNullOrEmpty(json))
			{
				return false;
			}

			try
			{
				value = JsonSerializer.Deserialize<QueryExecutionBenchmarks>(json, DefaultOptions);
				return value != null;
			}
			catch (JsonException)
			{
				return false;
			}
		}
	}
}
