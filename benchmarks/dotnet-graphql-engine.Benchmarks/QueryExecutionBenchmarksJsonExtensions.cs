using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace benchmarks.dotnet_graphql_engine.Benchmarks
{
    public static class QueryExecutionBenchmarksJsonExtensions
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public static string ToJson(this QueryExecutionBenchmarks value, bool indented = false)
        {
            options.WriteIndented = indented;
            return JsonSerializer.Serialize(value, options);
        }

        public static QueryExecutionBenchmarks? FromJson(string json)
        {
            return JsonSerializer.Deserialize<QueryExecutionBenchmarks>(json, options);
        }

        public static bool TryFromJson(string json, out QueryExecutionBenchmarks? value)
        {
            try
            {
                value = JsonSerializer.Deserialize<QueryExecutionBenchmarks>(json, options);
                return value != null;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
