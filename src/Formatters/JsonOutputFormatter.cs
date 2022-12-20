#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Common.Utilities;
using System.Text.Json;

namespace GraphQLEngine.Formatters;

/// <summary>
/// Formats data as JSON output
/// Supports various JSON formatting options and customizations
/// </summary>
sealed public class JsonOutputFormatter
{
    private readonly JsonFormatterOptions _options;

    public JsonOutputFormatter(JsonFormatterOptions? options = null)
    {
        _options = options ?? JsonFormatterOptions.Default();
    }

    /// <summary>
    /// Formats an object as JSON string
    /// </summary>
    public string Format(object? data)
    {
        if (data is null)
            return _options.PrettyPrint ? JsonHelper.Serialize(null, pretty: true) : "null";

        return _options.PrettyPrint
            ? JsonHelper.Serialize(data, pretty: true)
            : JsonHelper.Serialize(data);
    }

    /// <summary>
    /// Formats data with a wrapper object
    /// </summary>
    public string FormatWithWrapper(object? data, string wrapperKey = "data")
    {
        var wrapper = new Dictionary<string, object?>
        {
            { wrapperKey, data }
        };

        return Format(wrapper);
    }

    /// <summary>
    /// Formats data with metadata
    /// </summary>
    public string FormatWithMetadata(object? data, Dictionary<string, object>? metadata = null)
    {
        var result = new Dictionary<string, object?>
        {
            { "data", data }
        };

        if (_options.IncludeMetadata && metadata is not null)
        {
            result["metadata"] = metadata;
        }

        if (_options.IncludeTimestamp)
        {
            result["timestamp"] = DateTime.UtcNow;
        }

        return Format(result);
    }

    /// <summary>
    /// Formats a successful response
    /// </summary>
    public string FormatSuccess(object? data, string? message = null)
    {
        var response = new
        {
            success = true,
            message = message ?? "Operation successful",
            data = data,
            timestamp = _options.IncludeTimestamp ? (DateTime?)DateTime.UtcNow : null
        };

        return Format(response);
    }

    /// <summary>
    /// Formats an error response
    /// </summary>
    public string FormatError(string error, string? code = null, object? details = null)
    {
        var response = new
        {
            success = false,
            error = error,
            code = code,
            details = details,
            timestamp = _options.IncludeTimestamp ? (DateTime?)DateTime.UtcNow : null
        };

        return Format(response);
    }

    /// <summary>
    /// Formats paginated data
    /// </summary>
    public string FormatPaginated(
        IEnumerable<object>? data,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var response = new
        {
            data = data,
            pagination = new
            {
                pageNumber = pageNumber,
                pageSize = pageSize,
                totalCount = totalCount,
                totalPages = totalPages,
                hasNextPage = pageNumber < totalPages,
                hasPreviousPage = pageNumber > 1
            },
            timestamp = _options.IncludeTimestamp ? (DateTime?)DateTime.UtcNow : null
        };

        return Format(response);
    }

    /// <summary>
    /// Formats a batch response
    /// </summary>
    public string FormatBatch(IEnumerable<object>? items, int count)
    {
        var response = new
        {
            data = items,
            batch = new
            {
                count = count,
                processedAt = _options.IncludeTimestamp ? (DateTime?)DateTime.UtcNow : null
            }
        };

        return Format(response);
    }

    /// <summary>
    /// Streams JSON formatted data (for large datasets)
    /// </summary>
    public IAsyncEnumerable<string> FormatAsStream<T>(IAsyncEnumerable<T> items)
    {
        return FormatStreamInternal(items);
    }

    private async IAsyncEnumerable<string> FormatStreamInternal<T>(IAsyncEnumerable<T> items)
    {
        yield return "[";

        var first = true;
        await foreach (var item in items)
        {
            if (!first)
                yield return ",";

            yield return Format(item);
            first = false;
        }

        yield return "]";
    }

    /// <summary>
    /// Formats data with custom serialization options
    /// </summary>
    public string FormatWithOptions(object? data, JsonSerializerOptions options)
    {
        if (data is null)
            return "null";

        return JsonSerializer.Serialize(data, options);
    }

    /// <summary>
    /// Minifies JSON string (removes whitespace)
    /// </summary>
    public string Minify(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        try
        {
            var parsed = JsonHelper.Deserialize<object>(json);
            return JsonHelper.Serialize(parsed, pretty: false);
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// Prettifies JSON string (adds formatting)
    /// </summary>
    public string Prettify(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        try
        {
            var parsed = JsonHelper.Deserialize<object>(json);
            return JsonHelper.Serialize(parsed, pretty: true);
        }
        catch
        {
            return json;
        }
    }
}

/// <summary>
/// Options for JSON formatting
/// </summary>
sealed public class JsonFormatterOptions
{
    public bool PrettyPrint { get; set; } = true;
    public bool IncludeMetadata { get; set; } = false;
    public bool IncludeTimestamp { get; set; } = true;
    public bool IncludeNullValues { get; set; } = false;
    public int? MaxNestingLevel { get; set; } = null;

    public static JsonFormatterOptions Default()
    {
        return new JsonFormatterOptions();
    }

    public static JsonFormatterOptions Compact()
    {
        return new JsonFormatterOptions
        {
            PrettyPrint = false,
            IncludeMetadata = false,
            IncludeTimestamp = false,
            IncludeNullValues = false
        };
    }

    public static JsonFormatterOptions Detailed()
    {
        return new JsonFormatterOptions
        {
            PrettyPrint = true,
            IncludeMetadata = true,
            IncludeTimestamp = true,
            IncludeNullValues = true
        };
    }
}
