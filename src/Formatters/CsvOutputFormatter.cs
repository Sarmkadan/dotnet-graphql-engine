// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;

namespace GraphQLEngine.Formatters;

/// <summary>
/// Formats data as CSV (Comma-Separated Values) output
/// Supports various CSV options and handles special characters
/// </summary>
public class CsvOutputFormatter
{
    private readonly CsvFormatterOptions _options;

    public CsvOutputFormatter(CsvFormatterOptions? options = null)
    {
        _options = options ?? CsvFormatterOptions.Default();
    }

    /// <summary>
    /// Formats a collection of objects as CSV
    /// </summary>
    public string Format<T>(IEnumerable<T> data) where T : class
    {
        if (data == null)
            return string.Empty;

        var items = data.ToList();
        if (items.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        // Get properties for the first item
        var properties = GetProperties<T>();

        // Write header if enabled
        if (_options.IncludeHeader)
        {
            var headerRow = string.Join(_options.Delimiter, properties.Select(p => EscapeField(p.Name)));
            sb.AppendLine(headerRow);
        }

        // Write data rows
        foreach (var item in items)
        {
            var row = string.Join(_options.Delimiter,
                properties.Select(p => EscapeField(GetPropertyValue(item, p))));
            sb.AppendLine(row);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a single object as a CSV row
    /// </summary>
    public string FormatRow<T>(T item) where T : class
    {
        if (item == null)
            return string.Empty;

        var properties = GetProperties<T>();
        return string.Join(_options.Delimiter,
            properties.Select(p => EscapeField(GetPropertyValue(item, p))));
    }

    /// <summary>
    /// Formats data with custom column selection
    /// </summary>
    public string Format<T>(IEnumerable<T> data, params string[] columnNames) where T : class
    {
        if (data == null || columnNames.Length == 0)
            return string.Empty;

        var items = data.ToList();
        if (items.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        var type = typeof(T);

        // Write header
        if (_options.IncludeHeader)
        {
            var headerRow = string.Join(_options.Delimiter,
                columnNames.Select(c => EscapeField(c)));
            sb.AppendLine(headerRow);
        }

        // Write data rows
        foreach (var item in items)
        {
            var row = string.Join(_options.Delimiter,
                columnNames.Select(name =>
                {
                    var property = type.GetProperty(name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    return EscapeField(property?.GetValue(item)?.ToString() ?? "");
                }));
            sb.AppendLine(row);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a list of dictionaries as CSV
    /// </summary>
    public string FormatDictionaries(IEnumerable<Dictionary<string, object?>> data)
    {
        if (data == null)
            return string.Empty;

        var items = data.ToList();
        if (items.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        // Collect all unique keys
        var allKeys = items.SelectMany(d => d.Keys)
            .Distinct()
            .OrderBy(k => k)
            .ToList();

        // Write header
        if (_options.IncludeHeader)
        {
            var headerRow = string.Join(_options.Delimiter,
                allKeys.Select(k => EscapeField(k)));
            sb.AppendLine(headerRow);
        }

        // Write data rows
        foreach (var item in items)
        {
            var row = string.Join(_options.Delimiter,
                allKeys.Select(key =>
                {
                    var value = item.ContainsKey(key) ? item[key] : null;
                    return EscapeField(value?.ToString() ?? "");
                }));
            sb.AppendLine(row);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Writes CSV data to a stream
    /// </summary>
    public async Task WriteToStreamAsync<T>(Stream stream, IEnumerable<T> data) where T : class
    {
        var csv = Format(data);
        var bytes = Encoding.UTF8.GetBytes(csv);

        if (_options.IncludeBom)
        {
            var bom = Encoding.UTF8.GetPreamble();
            await stream.WriteAsync(bom, 0, bom.Length);
        }

        await stream.WriteAsync(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Parses a CSV string back to objects
    /// </summary>
    public List<Dictionary<string, string>> Parse(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return new List<Dictionary<string, string>>();

        var result = new List<Dictionary<string, string>>();
        var lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        if (lines.Length == 0)
            return result;

        string[] headers = null!;

        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            var fields = ParseLine(lines[i]);

            if (i == 0 && _options.IncludeHeader)
            {
                headers = fields;
            }
            else
            {
                if (headers == null)
                    headers = Enumerable.Range(0, fields.Length).Select(x => $"Column{x}").ToArray();

                var row = new Dictionary<string, string>();
                for (int j = 0; j < fields.Length && j < headers.Length; j++)
                {
                    row[headers[j]] = fields[j];
                }
                result.Add(row);
            }
        }

        return result;
    }

    /// <summary>
    /// Escapes a field value for CSV
    /// </summary>
    private string EscapeField(object? value)
    {
        if (value == null)
            return "";

        var str = value.ToString() ?? "";

        // Check if field needs escaping
        if (str.Contains(_options.Delimiter) ||
            str.Contains(_options.Quote) ||
            str.Contains("\n") ||
            str.Contains("\r"))
        {
            // Escape quotes and wrap in quotes
            str = str.Replace(_options.Quote, _options.Quote + _options.Quote);
            str = $"{_options.Quote}{str}{_options.Quote}";
        }

        return str;
    }

    /// <summary>
    /// Parses a CSV line
    /// </summary>
    private string[] ParseLine(string line)
    {
        var fields = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;
        var i = 0;

        while (i < line.Length)
        {
            var ch = line[i];

            if (ch == char.Parse(_options.Quote))
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == char.Parse(_options.Quote))
                {
                    // Escaped quote
                    currentField.Append(_options.Quote);
                    i += 2;
                }
                else
                {
                    // Toggle quote state
                    inQuotes = !inQuotes;
                    i++;
                }
            }
            else if (ch.ToString() == _options.Delimiter && !inQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
                i++;
            }
            else
            {
                currentField.Append(ch);
                i++;
            }
        }

        fields.Add(currentField.ToString());
        return fields.ToArray();
    }

    /// <summary>
    /// Gets properties of a type
    /// </summary>
    private List<PropertyInfo> GetProperties<T>() where T : class
    {
        return typeof(T)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();
    }

    /// <summary>
    /// Gets property value
    /// </summary>
    private string GetPropertyValue<T>(T item, PropertyInfo property) where T : class
    {
        try
        {
            var value = property.GetValue(item);
            return value?.ToString() ?? "";
        }
        catch
        {
            return "";
        }
    }
}

/// <summary>
/// CSV formatting options
/// </summary>
public class CsvFormatterOptions
{
    public string Delimiter { get; set; } = ",";
    public string Quote { get; set; } = "\"";
    public bool IncludeHeader { get; set; } = true;
    public bool IncludeBom { get; set; } = false;

    public static CsvFormatterOptions Default()
    {
        return new CsvFormatterOptions();
    }

    public static CsvFormatterOptions TabDelimited()
    {
        return new CsvFormatterOptions { Delimiter = "\t" };
    }

    public static CsvFormatterOptions SemicolonDelimited()
    {
        return new CsvFormatterOptions { Delimiter = ";" };
    }
}
