using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Utilities
{
    /// <summary>
    /// Provides JSON serialization helpers for collections and objects.
    /// </summary>
    public static class CollectionExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private static readonly JsonSerializerOptions _jsonSerializerOptionsIndented = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        /// <summary>
        /// Converts the specified object to a JSON string.
        /// </summary>
        /// <param name="value">The object to convert to JSON.</param>
        /// <param name="indented">Whether to format the JSON string with indentation.</param>
        /// <returns>A JSON string representation of the object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static string ToJson(this object value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            return JsonSerializer.Serialize(value, indented ? _jsonSerializerOptionsIndented : _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into an object.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An object if deserialization is successful; otherwise, null.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
        public static object? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                return JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into an object.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized object if successful.</param>
        /// <returns>True if deserialization is successful; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
        public static bool TryFromJson(string json, out object? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                value = JsonSerializer.Deserialize<object>(json, _jsonSerializerOptions);
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
