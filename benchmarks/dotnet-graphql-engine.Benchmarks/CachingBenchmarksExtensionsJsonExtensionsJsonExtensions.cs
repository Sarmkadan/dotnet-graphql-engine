using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dotnet_graphql_engine.Benchmarks
{
    /// <summary>
    /// Provides JSON serialization helpers for <see cref="CachingBenchmarksExtensionsJsonExtensions"/>.
    /// </summary>
    public static class CachingBenchmarksExtensionsJsonExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Converts the specified <see cref="CachingBenchmarksExtensionsJsonExtensions"/> to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="CachingBenchmarksExtensionsJsonExtensions"/> to convert.</param>
        /// <param name="indented">Whether to format the JSON string with indentation.</param>
        /// <returns>A JSON string representation of the specified <see cref="CachingBenchmarksExtensionsJsonExtensions"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static string ToJson(this CachingBenchmarksExtensionsJsonExtensions value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            return JsonSerializer.Serialize(value, indented ? _jsonSerializerOptions : _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to convert the specified JSON string to a <see cref="CachingBenchmarksExtensionsJsonExtensions"/>.
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <returns>A <see cref="CachingBenchmarksExtensionsJsonExtensions"/> instance if the conversion is successful; otherwise, null.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
        public static CachingBenchmarksExtensionsJsonExtensions? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                return JsonSerializer.Deserialize<CachingBenchmarksExtensionsJsonExtensions>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to convert the specified JSON string to a <see cref="CachingBenchmarksExtensionsJsonExtensions"/>.
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <param name="value">The converted <see cref="CachingBenchmarksExtensionsJsonExtensions"/> instance if the conversion is successful; otherwise, null.</param>
        /// <returns>True if the conversion is successful; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
        public static bool TryFromJson(string json, out CachingBenchmarksExtensionsJsonExtensions? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                value = JsonSerializer.Deserialize<CachingBenchmarksExtensionsJsonExtensions>(json, _jsonSerializerOptions);
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
