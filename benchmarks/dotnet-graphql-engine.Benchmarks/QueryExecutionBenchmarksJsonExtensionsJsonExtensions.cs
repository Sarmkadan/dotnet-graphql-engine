using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dotnet-graphql-engine.Benchmarks
{
    public static class QueryExecutionBenchmarksJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public static string ToJson(this QueryExecutionBenchmarksJsonExtensions value, bool indented = false)
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

        public static QueryExecutionBenchmarksJsonExtensions? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            try
            {
                return JsonSerializer.Deserialize<QueryExecutionBenchmarksJsonExtensions>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static bool TryFromJson(string json, out QueryExecutionBenchmarksJsonExtensions? value)
        {
            ArgumentNullException.ThrowIfNull(json);
            try
            {
                value = JsonSerializer.Deserialize<QueryExecutionBenchmarksJsonExtensions>(json, _jsonSerializerOptions);
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
