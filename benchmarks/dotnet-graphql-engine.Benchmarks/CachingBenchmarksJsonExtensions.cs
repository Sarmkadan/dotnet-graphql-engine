using System.Text.Json;

namespace dotnet_graphql_engine.Benchmarks
{
    public static class CachingBenchmarksJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string ToJson(this CachingBenchmarks value, bool indented = false)
        {
            var options = indented
                ? new JsonSerializerOptions(_options) { WriteIndented = true }
                : _options;

            return JsonSerializer.Serialize(value, options);
        }

        public static CachingBenchmarks? FromJson(string json)
        {
            return JsonSerializer.Deserialize<CachingBenchmarks>(json, _options);
        }

        public static bool TryFromJson(string json, out CachingBenchmarks? value)
        {
            try
            {
                value = FromJson(json);
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
