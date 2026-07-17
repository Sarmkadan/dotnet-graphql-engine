using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQLEngine.Common.Utilities;

namespace GraphQLEngine.Common.Utilities
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for the <see cref="DateTimeExtensions"/> type.
    /// </summary>
    public static class DateTimeExtensionsJsonExtensions
    {
        /// <summary>
        /// Private static readonly JSON serializer options with camelCase naming policy.
        /// </summary>
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Serializes a <see cref="DateTime"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The date and time value to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representation of the date and time.</returns>
        public static string ToJson(this DateTime value, bool indented = false)
        {
            
            JsonSerializerOptions options = new(JsonSerializerOptions)
            {
                WriteIndented = indented
            };
            
            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a nullable <see cref="DateTime"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A nullable <see cref="DateTime"/> instance, or null if deserialization fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
        /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
        public static DateTime? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            
            return JsonSerializer.Deserialize<DateTime>(json, JsonSerializerOptions);
        }

        /// <summary>
        /// Tries to deserialize a JSON string to a nullable <see cref="DateTime"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized date and time if successful; otherwise null.</param>
        /// <returns>True if deserialization succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
        public static bool TryFromJson(string json, out DateTime? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            
            try
            {
                DateTime deserialized = JsonSerializer.Deserialize<DateTime>(json, JsonSerializerOptions);
                value = deserialized;
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
