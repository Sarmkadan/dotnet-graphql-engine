using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace examples.v2_basic_usage
{
    public static class GraphQLHttpRequestExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public static string ToJson(this GraphQLHttpRequestExtensions value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (indented)
            {
                return JsonSerializer.Serialize(value, _jsonSerializerOptions);
            }
            else
            {
                return JsonSerializer.Serialize(value);
            }
        }

        public static GraphQLHttpRequestExtensions? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            try
            {
                return JsonSerializer.Deserialize<GraphQLHttpRequestExtensions>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static bool TryFromJson(string json, out GraphQLHttpRequestExtensions? value)
        {
            ArgumentNullException.ThrowIfNull(json);
            try
            {
                value = JsonSerializer.Deserialize<GraphQLHttpRequestExtensions>(json, _jsonSerializerOptions);
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
