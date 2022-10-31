// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// JSON serialization and deserialization helper
/// Provides utilities for working with JSON data
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes an object to JSON string
    /// </summary>
    public static string Serialize(object? obj, bool pretty = false)
    {
        if (obj == null)
            return "null";

        try
        {
            var options = pretty ? PrettyOptions : DefaultOptions;
            return JsonSerializer.Serialize(obj, options);
        }
        catch
        {
            return obj.ToString() ?? "null";
        }
    }

    /// <summary>
    /// Serializes an object with custom options
    /// </summary>
    public static string Serialize(object? obj, JsonSerializerOptions options)
    {
        if (obj == null)
            return "null";

        try
        {
            return JsonSerializer.Serialize(obj, options);
        }
        catch
        {
            return obj.ToString() ?? "null";
        }
    }

    /// <summary>
    /// Deserializes a JSON string to an object
    /// </summary>
    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Deserializes a JSON string with custom options
    /// </summary>
    public static T? Deserialize<T>(string json, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Deserializes a JSON string to a dictionary
    /// </summary>
    public static Dictionary<string, object?>? DeserializeToDictionary(string json)
    {
        return Deserialize<Dictionary<string, object?>>(json);
    }

    /// <summary>
    /// Deserializes a JSON string to a list
    /// </summary>
    public static List<T>? DeserializeToList<T>(string json)
    {
        return Deserialize<List<T>>(json);
    }

    /// <summary>
    /// Converts an object to a dictionary
    /// </summary>
    public static Dictionary<string, object?>? ToDict(object? obj)
    {
        if (obj == null)
            return null;

        try
        {
            var json = Serialize(obj);
            return DeserializeToDictionary(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a dictionary to an object
    /// </summary>
    public static T? FromDict<T>(Dictionary<string, object?>? dict)
    {
        if (dict == null)
            return default;

        try
        {
            var json = Serialize(dict);
            return Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Checks if a string is valid JSON
    /// </summary>
    public static bool IsValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var document = JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets a value from a JSON path
    /// </summary>
    public static object? GetValueByPath(string json, string path)
    {
        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            using var document = JsonDocument.Parse(json);
            var element = document.RootElement;

            foreach (var part in path.Split('.'))
            {
                if (element.TryGetProperty(part, out var property))
                    element = property;
                else
                    return null;
            }

            return element.GetRawText();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Merges multiple objects into a single JSON object
    /// </summary>
    public static Dictionary<string, object?> Merge(params object?[] objects)
    {
        var result = new Dictionary<string, object?>();

        foreach (var obj in objects)
        {
            if (obj == null)
                continue;

            var dict = ToDict(obj);
            if (dict != null)
            {
                foreach (var kvp in dict)
                    result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// Pretty-prints JSON to console (for debugging)
    /// </summary>
    public static void PrintJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var formatted = JsonSerializer.Serialize(document.RootElement, PrettyOptions);
            Console.WriteLine(formatted);
        }
        catch
        {
            Console.WriteLine("Invalid JSON");
        }
    }

    /// <summary>
    /// Compares two JSON objects for equality
    /// </summary>
    public static bool AreEqual(string? json1, string? json2)
    {
        if (string.IsNullOrEmpty(json1) || string.IsNullOrEmpty(json2))
            return json1 == json2;

        try
        {
            using var doc1 = JsonDocument.Parse(json1);
            using var doc2 = JsonDocument.Parse(json2);
            return JsonElement.DeepEquals(doc1.RootElement, doc2.RootElement);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Removes null values from JSON
    /// </summary>
    public static string RemoveNulls(string json)
    {
        var dict = DeserializeToDictionary(json);
        if (dict == null)
            return json;

        var filtered = dict.Where(kvp => kvp.Value != null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return Serialize(filtered);
    }

    /// <summary>
    /// Creates a minimal JSON options for performance
    /// </summary>
    public static JsonSerializerOptions GetMinimalOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = null
        };
    }
}
