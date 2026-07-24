using System;
using System.Text.Json;

namespace dotnet_graphql_engine.Common.Utilities
{
    /// <summary>
    /// Provides JSON serialization helpers for the ReflectionHelper utilities.
    /// </summary>
    public static class ReflectionHelperJsonExtensionsJsonExtensions
    {
        /// <summary>
        /// Serialises an object to JSON.
        /// </summary>
        /// <param name="value">The object to serialise. May be null.</param>
        /// <param name="pretty">If true, the output is indented.</param>
        /// <returns>A JSON string representing the object.</returns>
        public static string ToJson(object? value, bool pretty = false)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = pretty,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserialises a JSON string to an object of the specified type.
        /// </summary>
        /// <param name="json">The JSON payload.</param>
        /// <param name="type">The target type.</param>
        /// <returns>The deserialized object, or null if the JSON represents null.</returns>
        public static object? FromJson(string json, Type type)
        {
            if (json is null) throw new ArgumentNullException(nameof(json));
            if (type is null) throw new ArgumentNullException(nameof(type));

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize(json, type, options);
        }

        /// <summary>
        /// Tries to deserialise a JSON string to an object of the specified type.
        /// </summary>
        /// <param name="json">The JSON payload.</param>
        /// <param name="type">The target type.</param>
        /// <param name="value">When the method returns, contains the deserialized object if successful; otherwise null.</param>
        /// <returns>True if deserialization succeeded; otherwise false.</returns>
        public static bool TryFromJson(string json, Type type, out object? value)
        {
            try
            {
                value = FromJson(json, type);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// A simple identifier for the helper class.
        /// </summary>
        public static string Type => nameof(ReflectionHelperJsonExtensionsJsonExtensions);
    }
}
