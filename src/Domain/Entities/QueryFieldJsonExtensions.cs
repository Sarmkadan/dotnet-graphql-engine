using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQLEngine.Domain.Entities
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for the <see cref="QueryField"/> type.
    /// </summary>
    public static class QueryFieldJsonExtensions
    {
        /// <summary>
        /// Private static readonly JSON serializer options with camelCase naming policy.
        /// </summary>
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// Serializes a <see cref="QueryField"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="QueryField"/> to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representation of the <see cref="QueryField"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this QueryField value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            
            JsonSerializerOptions options = new(JsonSerializerOptions)
            {
                WriteIndented = indented
            };
            
            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="QueryField"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A <see cref="QueryField"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
        /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
        public static QueryField? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            
            return JsonSerializer.Deserialize<QueryField>(json, JsonSerializerOptions);
        }

        /// <summary>
        /// Tries to deserialize a JSON string to a <see cref="QueryField"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized <see cref="QueryField"/> if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise false.</returns>
        public static bool TryFromJson(string json, out QueryField? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            
            try
            {
                value = JsonSerializer.Deserialize<QueryField>(json, JsonSerializerOptions);
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
