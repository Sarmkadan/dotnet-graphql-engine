using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQLEngine.Domain.Entities
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for the <see cref="QueryArgument"/> type.
    /// </summary>
    public static class QueryArgumentJsonExtensions
    {
        /// <summary>
        /// Private static readonly JSON serializer options with camelCase naming policy.
        /// </summary>
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// Serializes a <see cref="QueryArgument"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="QueryArgument"/> to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representation of the <see cref="QueryArgument"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this QueryArgument value, bool indented = false)
            => JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

        /// <summary>
        /// Deserializes a JSON string to a <see cref="QueryArgument"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A <see cref="QueryArgument"/> instance, or null if the JSON represents a null value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
        /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
        public static QueryArgument? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<QueryArgument>(json, _jsonOptions);
        }

        /// <summary>
        /// Tries to deserialize a JSON string to a <see cref="QueryArgument"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized <see cref="QueryArgument"/> if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise false.</returns>
        public static bool TryFromJson(string json, out QueryArgument? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<QueryArgument>(json, _jsonOptions);
                return value is not null;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Serializes a collection of QueryArguments to a JSON array string.
        /// </summary>
        /// <param name="arguments">The collection of QueryArguments to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON array string representation of the QueryArguments.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/> is null.</exception>
        public static string ToJson(this IEnumerable<QueryArgument> arguments, bool indented = false)
            => JsonSerializer.Serialize(arguments, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

        /// <summary>
        /// Deserializes a JSON array string to a collection of QueryArguments.
        /// </summary>
        /// <param name="json">The JSON array string to deserialize.</param>
        /// <returns>A collection of QueryArgument instances.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
        /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
        public static IReadOnlyList<QueryArgument> FromJsonToList(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            var result = JsonSerializer.Deserialize<QueryArgument[]>(json, _jsonOptions);
            return result ?? Array.Empty<QueryArgument>();
        }

        /// <summary>
        /// Tries to deserialize a JSON array string to a collection of QueryArguments.
        /// </summary>
        /// <param name="json">The JSON array string to deserialize.</param>
        /// <param name="arguments">The deserialized collection of QueryArguments if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise false.</returns>
        public static bool TryFromJsonToList(string json, out IReadOnlyList<QueryArgument> arguments)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                var result = JsonSerializer.Deserialize<QueryArgument[]>(json, _jsonOptions);
                arguments = result ?? Array.Empty<QueryArgument>();
                return true;
            }
            catch (JsonException)
            {
                arguments = Array.Empty<QueryArgument>();
                return false;
            }
        }
    }
}