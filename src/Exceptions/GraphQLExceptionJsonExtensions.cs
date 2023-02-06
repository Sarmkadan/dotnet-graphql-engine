#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQLEngine.Exceptions;

namespace GraphQLEngine.Exceptions;

public static class GraphQLExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string ToJson(this GraphQLException value, bool indented = false)
    {
        if (indented)
        {
            JsonOptions.WriteIndented = true;
        }

        return JsonSerializer.Serialize(value, JsonOptions);
    }

    public static GraphQLException? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<GraphQLException>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static bool TryFromJson(string json, out GraphQLException? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<GraphQLException>(json, JsonOptions);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
